using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace H.Pipes.IO
{
    /// <summary>
    /// Wraps a <see cref="PipeStream"/> object and writes to it.
    /// </summary>
    public sealed class PipeStreamWriter : IDisposable, IAsyncDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the underlying <c>PipeStream</c> object.
        /// </summary>
        private PipeStream BaseStream { get; }
        private SemaphoreSlim SemaphoreSlim { get; } = new SemaphoreSlim(1, 1);

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new <c>PipeStreamWriter</c> object that writes to given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Pipe to write to</param>
        public PipeStreamWriter(PipeStream stream)
        {
            BaseStream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        #endregion

        #region Private stream writers

        private async Task WriteLengthAsync(int length, CancellationToken cancellationToken = default)
        {
            var buffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(length));

            await BaseStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes an object to the pipe.
        /// </summary>
        /// <param name="buffer">Object to write to the pipe</param>
        /// <param name="cancellationToken"></param>
        public async Task WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
        {
            try
            {
                await SemaphoreSlim.WaitAsync(cancellationToken);

                await WriteLengthAsync(buffer.Length, cancellationToken).ConfigureAwait(false);

                await BaseStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

                await BaseStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        /// <summary>
        /// Waits for the other end of the pipe to read all sent bytes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The pipe is closed.</exception>
        /// <exception cref="NotSupportedException">The pipe does not support write operations.</exception>
        /// <exception cref="IOException">The pipe is broken or another I/O error occurred.</exception>
        public void WaitForPipeDrain()
        {
            BaseStream.WaitForPipeDrain();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Dispose internal <see cref="PipeStream"/>
        /// </summary>
        public void Dispose()
        {
            BaseStream.Dispose();
            SemaphoreSlim.Dispose();
        }

        /// <summary>
        /// Dispose internal <see cref="PipeStream"/>
        /// </summary>
        public async ValueTask DisposeAsync()
        {
#if NETSTANDARD2_0
            BaseStream.Dispose();

            await Task.Delay(0).ConfigureAwait(false);
#else
            await BaseStream.DisposeAsync().ConfigureAwait(false);
#endif
            SemaphoreSlim.Dispose();
        }

        #endregion
    }
}