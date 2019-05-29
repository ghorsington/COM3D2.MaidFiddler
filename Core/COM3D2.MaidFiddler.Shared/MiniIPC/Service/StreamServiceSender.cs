using System;
using System.IO;
using System.Text;
using MessagePack;
using MessagePack.Resolvers;

namespace MiniIPC.Service
{
    public class StreamServiceSender<T> : IDisposable
    {
        public StreamServiceSender(Stream stream)
        {
            Stream = stream;
            Service = ProxyFactory.CreateSenderProxy<T>(SendMessage);
        }

        public T Service { get; }
        public Stream Stream { get; }

        public void Dispose()
        {
            Stream?.Dispose();
        }

        private byte[] SendMessage(string methodName, byte[] data)
        {
            // Send out the data
            var methodNameBytes = Encoding.UTF8.GetBytes(methodName);
            Stream.Write(BitConverter.GetBytes(methodNameBytes.Length), 0, 4);
            Stream.Write(methodNameBytes, 0, methodNameBytes.Length);
            var size = BitConverter.GetBytes(data?.Length ?? 0);

            Stream.Write(size, 0, size.Length);
            if (data != null && data.Length != 0)
            {
                Stream.Write(data, 0, data.Length);
            }

            Stream.Flush();

            // Read the new size back
            Stream.Read(size, 0, size.Length);
            var receiveBuffer = new byte[BitConverter.ToInt32(size, 0)];

            if (receiveBuffer.Length != 0)
                Stream.Read(receiveBuffer, 0, receiveBuffer.Length);
            
            if (receiveBuffer.Length == 0 || receiveBuffer[0] != 0x00)
                return receiveBuffer;

            var error = MessagePackSerializer.Deserialize<Error>(receiveBuffer, 0, StandardResolver.Instance, out int _);
            throw new RemoteException(error.Message, error.StackTrace);
        }
    }
}