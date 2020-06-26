using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace GhettoPipes
{
    /// <summary>
    ///     Original version from https://www.pinvoke.net/default.aspx/kernel32/CreateNamedPipe.html
    ///     This is a minor refactor of the original NamedPipe implementation that uses `unsafe` for faster
    ///     writes and reads. In addition, some documentation was rewritten for clarity.
    ///     Finally, added pipe transmission type specifier and access control configuration via SDDL.
    ///     This implementation of the pipe is always blocking. No async pipe is implemented.
    /// </summary>
    public class NamedPipeStream : Stream
    {
        /// <summary>
        ///     Available server modes
        /// </summary>
        [Flags]
        public enum PipeDirection
        {
            /// <summary>
            ///     Allow only to receive data from the client.
            /// </summary>
            In = 1 << 0,

            /// <summary>
            ///     Allow only to send data to the clients.
            /// </summary>
            Out = 1 << 1,

            /// <summary>
            ///     Allow to both send and receive data.
            /// </summary>
            InOut = In | Out
        }

        /// <summary>
        ///     How the data in a pipe is transferred
        /// </summary>
        public enum PipeReadMode
        {
            /// <summary>
            ///     Data is read as a stream of bytes (i.e. all data seen as a stream of bytes)
            /// </summary>
            Byte = 0,

            /// <summary>
            ///     Data is as a stream of messages (i.e. each write is its own message)
            /// </summary>
            Message = 2
        }

        /// <summary>
        ///     Type of the pipe
        /// </summary>
        public enum PipeType
        {
            /// <summary>
            ///     The pipe is a client pipe.
            /// </summary>
            Client = 0,

            /// <summary>
            ///     The pipe is a server pipe.
            /// </summary>
            Server = 1
        }

        /// <summary>
        ///     How the data in a pipe is transferred
        /// </summary>
        public enum PipeWriteMode
        {
            /// <summary>
            ///     Data is written as a stream of bytes (i.e. all data seen as a stream of bytes)
            /// </summary>
            Byte = 0,

            /// <summary>
            ///     Data is written as a stream of messages (i.e. each write is its own message)
            /// </summary>
            Message = 4
        }

        readonly PipeType pipeType;
        PipeDirection direction;
        IntPtr handle;

        protected NamedPipeStream(PipeType type)
        {
            handle = IntPtr.Zero;
            pipeType = type;
        }

        /// <inheritdoc />
        public override bool CanRead => (direction & PipeDirection.In) != 0;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <inheritdoc />
        public override bool CanWrite => (direction & PipeDirection.Out) != 0;

        /// <summary>
        ///     Returns true if there is data available to read.
        /// </summary>
        public bool DataAvailable => Win32Api.PeekNamedPipe(handle, null, 0, out _, out var avail, out _) && avail > 0;

        /// <summary>
        ///     Returns true if client is connected.  Should only be called after WaitForConnection() succeeds.
        /// </summary>
        /// <returns></returns>
        public bool IsConnected
        {
            get
            {
                if (pipeType != PipeType.Server)
                    throw new Exception("IsConnected() is only for server-side streams");

                if (Win32Api.ConnectNamedPipe(handle, IntPtr.Zero))
                    return false;
                return (uint)Marshal.GetLastWin32Error() == Win32Api.ERROR_PIPE_CONNECTED;
            }
        }

        /// <inheritdoc />
        public override long Length => throw new NotSupportedException("NamedPipeStream does not support seeking");

        /// <inheritdoc />
        public override long Position
        {
            get => throw new NotSupportedException("NamedPipeStream does not support seeking");
            set => throw new NotSupportedException("NamedPipeStream does not support seeking");
        }

        /// <inheritdoc />
        public override void Close()
        {
            Win32Api.CloseHandle(handle);
            handle = IntPtr.Zero;
        }

        /// <summary>
        ///     Creates a new pipe.
        /// </summary>
        /// <param name="pipeName">Name of the pipe.</param>
        /// <param name="pipeDirection">Data transfer direction.</param>
        /// <param name="writeMode">How data is written to the pipe.</param>
        /// <param name="readMode">How data is read from the part.</param>
        /// <param name="securityDescriptor">Pipe's security descriptor written is SDDL.</param>
        /// <returns>A named pipe stream.</returns>
        public static NamedPipeStream Create(string pipeName,
                                             PipeDirection pipeDirection,
                                             PipeWriteMode writeMode = PipeWriteMode.Byte,
                                             PipeReadMode readMode = PipeReadMode.Byte,
                                             string securityDescriptor = null)
        {
            var sa = securityDescriptor != null
                ? Win32Api.GetSecurityAttributesFromSDDL(securityDescriptor)
                : Win32Api.NullAttributes;

            var handle = Win32Api.CreateNamedPipe($@"\\.\pipe\{pipeName}", (uint)pipeDirection,
                                                  (uint)writeMode | (uint)readMode | Win32Api.PIPE_WAIT,
                                                  Win32Api.PIPE_UNLIMITED_INSTANCES, 0, // outBuffer,
                                                  1024, // inBuffer,
                                                  Win32Api.NMPWAIT_WAIT_FOREVER, ref sa);

            if (sa.SecurityDescriptor != IntPtr.Zero)
                Win32Api.LocalFree(sa.SecurityDescriptor);

            if (handle.ToInt32() == Win32Api.INVALID_HANDLE_VALUE)
                ThrowWin32Error($"Error creating named pipe {pipeName}.");

            var self = new NamedPipeStream(PipeType.Server) { handle = handle, direction = pipeDirection };

            return self;
        }

        /// <summary>
        ///     Server only: disconnect the pipe.  For most applications, you should just call WaitForConnection()
        ///     instead, which automatically does a disconnect of any old connection.
        /// </summary>
        public void Disconnect()
        {
            if (pipeType != PipeType.Server)
                throw new NotSupportedException("Disconnect() is only for server-side streams");
            Win32Api.DisconnectNamedPipe(handle);
        }

        /// <summary>
        ///     Flushes the stream and waits until the other end of the pipe has read all the bytes.
        /// </summary>
        public override void Flush()
        {
            if (!CanWrite)
                throw new NotSupportedException("The pipe must be allowed to write to flush");
            if (handle == IntPtr.Zero)
                throw new ObjectDisposedException("NamedPipeStream", "The stream has already been closed");
            Win32Api.FlushFileBuffers(handle);
        }

        /// <summary>
        ///     Opens an existing pipe on the local machine.
        /// </summary>
        /// <param name="pipeName">Name of the pipe to open.</param>
        /// <param name="direction">Data transfer direction.</param>
        /// <returns>Pipe stream that represents the opened pipe.</returns>
        public static NamedPipeStream Open(string pipeName, PipeDirection direction)
        {
            return Open(pipeName, ".", direction);
        }

        /// <summary>
        ///     Open an existing pipe on a specified machine.
        /// </summary>
        /// <param name="pipeName">Name of the pipe to open.</param>
        /// <param name="serverName">Name of the machine where the pipe is located.</param>
        /// <param name="pipeDirection">Data transfer direction.</param>
        /// <returns>Pipe stream that represents the opened pipe.</returns>
        public static NamedPipeStream Open(string pipeName, string serverName, PipeDirection pipeDirection)
        {
            uint pipeMode = 0;
            if ((pipeDirection & PipeDirection.In) != 0)
                pipeMode |= Win32Api.GENERIC_READ;
            if ((pipeDirection & PipeDirection.Out) != 0)
                pipeMode |= Win32Api.GENERIC_WRITE;
            var pipeHandle = Win32Api.CreateFile($@"\\{serverName}\pipe\{pipeName}", pipeMode, 0, IntPtr.Zero,
                                                 Win32Api.OPEN_EXISTING, 0, IntPtr.Zero);

            if (pipeHandle.ToInt32() != Win32Api.INVALID_HANDLE_VALUE)
                return new NamedPipeStream(PipeType.Client) { handle = pipeHandle, direction = pipeDirection };
            
            ThrowWin32Error($@"Failed to open pipe \\{serverName}\pipe\{pipeName}");
            return null;
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "The buffer to read into cannot be null");
            if (buffer.Length < offset + count)
                throw new ArgumentException("Buffer is not large enough to hold requested data", nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset cannot be negative");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count cannot be negative");
            if (!CanRead)
                throw new NotSupportedException("The stream does not support reading");
            if (handle == IntPtr.Zero)
                throw new ObjectDisposedException("NamedPipeStream", "The stream has already been closed");

            unsafe
            {
                fixed (byte* buf = buffer)
                {
                    if (!Win32Api.ReadFile(handle, buf, count, out var read, IntPtr.Zero))
                        ThrowWin32Error("ReadFile failed");
                    return read;
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("NamedPipeStream doesn't support seeking");
        }

        /// <inheritdoc />
        public override void SetLength(long length)
        {
            throw new NotSupportedException("NamedPipeStream doesn't support SetLength");
        }

        /// <summary>
        ///     Server only: block until client connects
        /// </summary>
        /// <returns></returns>
        public bool WaitForConnection()
        {
            if (pipeType != PipeType.Server)
                throw new Exception("WaitForConnection() is only for server-side streams");
            Win32Api.DisconnectNamedPipe(handle);
            if (Win32Api.ConnectNamedPipe(handle, IntPtr.Zero))
                return true;

            return (uint)Marshal.GetLastWin32Error() == Win32Api.ERROR_PIPE_CONNECTED;
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "The buffer to write into cannot be null");
            if (buffer.Length < offset + count)
                throw new ArgumentException("Buffer does not contain amount of requested data", nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset cannot be negative");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count cannot be negative");
            if (!CanWrite)
                throw new NotSupportedException("The stream does not support writing");
            if (handle == IntPtr.Zero)
                throw new ObjectDisposedException("NamedPipeStream", "The stream has already been closed");

            unsafe
            {
                fixed (byte* buf = buffer)
                {
                    if (!Win32Api.WriteFile(handle, buf + offset, count, out var written, IntPtr.Zero))
                        ThrowWin32Error("Writing to the stream failed");

                    if (written < count)
                        throw new IOException("Failed to write entire buffer to pipe");
                }
            }
        }

        private const int ERROR_BROKEN_PIPE = 0x6d;
        private const int ERROR_SUCCESS = 0x00;
        
        private static void ThrowWin32Error(string message = null)
        {
            int err = Marshal.GetLastWin32Error();
            switch (err)
            {
                case ERROR_SUCCESS:
                    return;
                case ERROR_BROKEN_PIPE:
                    throw new EndOfStreamException("The pipe has been ended");
                default:
                    throw new Win32Exception(err, message);
            }
        }

        static class Win32Api
        {
            public const uint GENERIC_READ = 0x80000000;
            public const uint GENERIC_WRITE = 0x40000000;
            public const int INVALID_HANDLE_VALUE = -1;
            public const uint OPEN_EXISTING = 3;
            public const uint PIPE_WAIT = 0x00000000;
            public const uint PIPE_UNLIMITED_INSTANCES = 255;
            public const uint NMPWAIT_WAIT_FOREVER = 0xffffffff;
            public const ulong ERROR_PIPE_CONNECTED = 535;
            const int SDDL_REVISION_1 = 1;

            public static readonly SecurityAttributes NullAttributes =
                new SecurityAttributes { Inheritable = true, Length = 0, SecurityDescriptor = IntPtr.Zero };

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool CloseHandle(IntPtr handle);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ConnectNamedPipe(IntPtr hHandle, // handle to named pipe
                                                       IntPtr lpOverlapped // overlapped structure
            );

            [DllImport("kernel32.dll", EntryPoint = "CreateFile", SetLastError = true)]
            public static extern IntPtr CreateFile(string lpFileName,
                                                   uint dwDesiredAccess,
                                                   uint dwShareMode,
                                                   IntPtr lpSecurityAttributes,
                                                   uint dwCreationDisposition,
                                                   uint dwFlagsAndAttributes,
                                                   IntPtr hTemplateFile);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr CreateNamedPipe(string lpName, // pipe name
                                                        uint dwOpenMode, // pipe open mode
                                                        uint dwPipeMode, // pipe-specific modes
                                                        uint nMaxInstances, // maximum number of instances
                                                        uint nOutBufferSize, // output buffer size
                                                        uint nInBufferSize, // input buffer size
                                                        uint nDefaultTimeOut, // time-out interval
                                                        ref SecurityAttributes pipeSecurityDescriptor // SD
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool DisconnectNamedPipe(IntPtr hHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool FlushFileBuffers(IntPtr handle);

            public static SecurityAttributes GetSecurityAttributesFromSDDL(string sddl)
            {
                var result = new SecurityAttributes
                {
                    Length = Marshal.SizeOf(typeof(SecurityAttributes)),
                    Inheritable = true
                };

                ConvertStringSecurityDescriptorToSecurityDescriptor(sddl, SDDL_REVISION_1,
                                                                    out result.SecurityDescriptor, IntPtr.Zero);
                return result;
            }

            [DllImport("kernel32.dll")]
            public static extern IntPtr LocalFree(IntPtr hMem);

            [DllImport("kernel32.dll", EntryPoint = "PeekNamedPipe", SetLastError = true)]
            public static extern bool PeekNamedPipe(IntPtr handle,
                                                    byte[] buffer,
                                                    int nBufferSize,
                                                    out int bytesRead,
                                                    out int bytesAvail,
                                                    out int bytesLeft);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern unsafe bool ReadFile(IntPtr handle,
                                                      byte* buffer,
                                                      int toRead,
                                                      out int read,
                                                      IntPtr zero);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern unsafe bool WriteFile(IntPtr handle,
                                                       byte* buffer,
                                                       int count,
                                                       out int written,
                                                       IntPtr zero);

            [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
            static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
                string stringSecurityDescriptor,
                int stringSdRevision,
                out IntPtr pSecurityDescriptor,
                IntPtr securityDescriptorSize);

            [StructLayout(LayoutKind.Sequential)]
            public struct SecurityAttributes
            {
                public int Length;
                public IntPtr SecurityDescriptor;
                public bool Inheritable;
            }
        }
    }
}