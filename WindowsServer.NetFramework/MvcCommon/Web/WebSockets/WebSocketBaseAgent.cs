using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebSockets;
using WindowsServer.Log;

namespace WindowsServer.Web.WebSockets
{
    public class WebSocketBaseAgent
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private CancellationTokenSource _receiveCancellationTokenSource = new CancellationTokenSource();

        private AspNetWebSocketContext _context { get; set; }
        public AspNetWebSocketContext Context
        {
            get
            {
                return this._context;
            }
        }

        private WebSocket _socket { get; set; }
        public WebSocket Socket
        {
            get
            {
                return this._socket;
            }
        }

        public Action<WebSocketBaseAgent> Closed { get; set; }

        public WebSocketState State
        {
            get
            {
                return this._socket.State;
            }
        }

        public WebSocketBaseAgent(AspNetWebSocketContext context)
        {
            this._context = context;
            this._socket = this._context.WebSocket;
        }

        public async Task Run()
        {
            while (true)
            {
                var buffer = new ArraySegment<byte>(new byte[4096]);

                WebSocketReceiveResult result;
                try
                {
                    result = await this._socket.ReceiveAsync(buffer, _receiveCancellationTokenSource.Token);
                }
                catch (WebSocketException ex)
                {
                    _logger.InfoException("WebSocket: Failed to receive data.", ex);
                    break;
                }
                catch(OperationCanceledException)
                {
                    break;
                }

                // If the socket is still open, echo the message back to the client
                if (this._socket.State == WebSocketState.Open)
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string text = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        await HandleText(text);
                    }
                }
                else
                {
                    break;
                }
            }

            await Close();
        }

        public async Task Close(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.Empty, string statusDescription = null)
        {
            // Simply solution to avoid Close() being called from multiple threads.
            var socket = this._socket;
            this._socket = null;
            if (socket == null)
            {
                return;
            }

            _receiveCancellationTokenSource.Cancel();

            try
            {
                await socket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
                socket.Dispose();
            }
            catch (Exception ex)
            {
                _logger.InfoException("WebSocket: Failed to close the websocket.", ex);
            }

            if (Closed != null)
            {
                Closed(this);
            }

        }

        public virtual async Task SendText(string text)
        {
            //await this._socket.SendAsync(
            this._socket.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None)
                .Forget(false);
            await Task.FromResult(0);
        }

        protected virtual async Task HandleText(string text)
        {
            // Dummy implementation. Should be overrided and the subclass should not call this implementation.
            await SendText("Echo:" + text);
        }

    }

}
