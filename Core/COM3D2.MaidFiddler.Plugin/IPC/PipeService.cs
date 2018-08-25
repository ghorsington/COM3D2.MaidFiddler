using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading;
using COM3D2.MaidFiddler.Core.IPC.Util;
using COM3D2.MaidFiddler.Core.Utils;

namespace COM3D2.MaidFiddler.Core.IPC
{
    public class PipeService<T> : IDisposable where T : IDisposable
    {
        private ulong currentID;
        private uint loopThreadId;
        private readonly Thread messageThread;
        private readonly NamedPipeServerStream pipeStream;
        private bool running;
        private readonly T service;

        public PipeService(T service, string name)
        {
            this.service = service;
            pipeStream = PipeFactory.CreatePipe(name);
            messageThread = new Thread(RunInternal);
            Methods = new Dictionary<string, MethodData>();
            InitService();
        }

        public bool IsConnected { get; private set; }

        private Dictionary<string, MethodData> Methods { get; }

        public void Dispose()
        {
            service.Dispose();
            if (IsConnected)
                pipeStream.Disconnect();
            Debugger.Debug(LogLevel.Info, "PipeService: Closing server...");
            pipeStream.Close();
            Debugger.Debug(LogLevel.Info, "PipeService: Closed!");
            Stop();
        }

        public event EventHandler ConnectionLost;

        public void Run()
        {
            if (running)
                return;
            running = true;
            messageThread.Start();
        }

        public void Stop()
        {
            if (!running)
                return;
            running = false;
            ThreadHelpers.CancelSynchronousIo(loopThreadId);
            messageThread.Join();
        }

        protected virtual void AddMethod(MethodInfo methodInfo)
        {
            if (methodInfo.ContainsGenericParameters)
                    // Currently don't support generic parameters to keep it simple for now
                return;

            var data = new MethodData {method = methodInfo, parameters = methodInfo.GetParameters()};
            data.NeededParameterCount = data.parameters.Count(p => !p.IsOptional);

            int i = 0;
            string name;
            do
            {
                name = $"{methodInfo.Name}{(i == 0 ? string.Empty : $"_{i}")}";
                i++;
            } while (Methods.ContainsKey(name));

            Methods.Add(name, data);
        }

        private object Invoke(string eventName, object[] args)
        {
            if (!Methods.TryGetValue(eventName, out MethodData data))
                throw new MissingMethodException($"The service does not provide {eventName}().");

            if (args.Length < data.NeededParameterCount || args.Length > data.parameters.Length)
            {
                string argsString = data.NeededParameterCount == data.parameters.Length
                                            ? data.NeededParameterCount.ToString()
                                            : $"{data.NeededParameterCount} to {data.parameters.Length}";
                throw new ArgumentException($"{eventName}() can take only {argsString} arguments");
            }

            for (int i = 0; i < args.Length; i++)
            {
                Type t1 = args[i].GetType();
                Type t2 = data.parameters[i].ParameterType;
                if (!t2.IsAssignableFrom(t1))
                    throw new ArgumentException($"Argument #{i} is of type {t1.FullName} but should be {t2.FullName}");
            }

            var passArgs = args;
            if (args.Length < data.parameters.Length)
            {
                passArgs = new object[data.parameters.Length];
                for (int i = 0; i < passArgs.Length; i++)
                    passArgs[i] = i < args.Length ? args[i] : data.parameters[i].DefaultValue;
            }

            object instance = data.method.IsStatic ? null : (object) service;
            try
            {
                object result = data.method.Invoke(instance, passArgs);
                if (data.method.ReturnType == typeof(void))
                    result = true; // Return true to specify that the side-effect was completed nicely
                return result;
            }
            catch (TargetInvocationException te)
            {
                throw te.InnerException ?? new Exception($"Error when invoking {eventName}()!");
            }
        }

        private void InitService()
        {
            Type objectType = typeof(T);

            var publicMethods = objectType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            foreach (MethodInfo methodInfo in publicMethods.Where(m => m.DeclaringType != typeof(object)))
                AddMethod(methodInfo);
        }

        private void RunInternal()
        {
            loopThreadId = ThreadHelpers.GetCurrentThreadId();
            Debugger.Debug(LogLevel.Info, $"PipeService: Got server ID: {loopThreadId}");

            var bw = new BinaryWriter(pipeStream);
            var br = new BinaryReader(pipeStream);

            while (running)
            {
                Debugger.WriteLine(LogLevel.Info, "PipeService: Waiting for connection");
                try
                {
                    pipeStream.WaitForConnection();
                }
                catch (Exception)
                {
                    Debugger.WriteLine(LogLevel.Info, "PipeService: Aborting waiting!");
                    ((IDisposable) bw).Dispose();
                    ((IDisposable) br).Dispose();
                    return;
                }

                Debugger.WriteLine(LogLevel.Info, "PipeService: Connected!");
                IsConnected = true;
                pipeStream.Flush();

                while (running)
                    try
                    {
                        int len = (int) br.ReadUInt32();
                        var data = br.ReadBytes(len);

                        Message message;
                        try
                        {
                            message = SerializerUtils.Deserialize(data);
                            currentID = message.ID;
                        }
                        catch (Exception e)
                        {
                            Error(e, bw, 1);
                            continue;
                        }

                        if (message.Data is Call call)
                        {
                            try
                            {
                                Debugger.Debug(LogLevel.Info, $"Got invoke request: {call.Method}");
                                object response = Invoke(call.Method, ArgumentUnpacker.Unpack(call.Args));

                                var responseMsg = new Message {ID = currentID, Data = new Response {Result = response}};

                                var responseData = SerializerUtils.Serialize(responseMsg);

                                Debugger.Debug(LogLevel.Info, $"Writing {responseData.Length} bytes of response");
                                bw.Write((uint) responseData.Length);
                                bw.Write(responseData);
                                Debugger.Debug(LogLevel.Info, "Response sent!");
                            }
                            catch (Exception e)
                            {
                                Error(e, bw);
                            }
                        }
                        else if (message.Data is Ping ping)
                        {
                            var responseMsg = new Message {ID = 0, Data = new Ping {Pong = true}};

                            var responseData = SerializerUtils.Serialize(responseMsg);

                            bw.Write((uint) responseData.Length);
                            bw.Write(responseData);
                        }
                        else
                        {
                            Error(new Exception("Only calls are supported by a server!"), bw);
                        }
                    }
                    catch (EndOfStreamException e)
                    {
                        pipeStream.Flush();
                        pipeStream.Disconnect();
                        IsConnected = false;
                        ConnectionLost?.Invoke(null, EventArgs.Empty);
                        break;
                    }
            }
        }

        private void Error(Exception e, BinaryWriter writer, ulong increment = 0)
        {
            currentID += increment;
            Debugger.WriteLine(LogLevel.Error, $"Remote error: {e}");
            var err = new Message
            {
                    ID = currentID,
                    Data = new Error {ErrorName = e.GetType().FullName, ErrorMessage = e.Message, StackTrace = e.StackTrace}
            };

            var data = SerializerUtils.Serialize(err);

            writer.Write((uint) data.Length);
            writer.Write(data);
        }

        private struct MethodData
        {
            public MethodInfo method;
            public ParameterInfo[] parameters;
            public int NeededParameterCount;
        }
    }
}