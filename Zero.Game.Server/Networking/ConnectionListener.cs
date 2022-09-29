using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public class ConnectionListener<TState>
    {
        private readonly Socket _socket;
        private readonly SocketAsyncEventArgs _acceptArgs;
        private readonly ConnectionValidator<TState> _validator = new(TimeSpan.FromSeconds(10), 10);
        private readonly int _port;

        private int _stopped;

        public ConnectionListener(int port)
        {
            _port = port;
            _socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)
            {
                DualMode = true
            };

            _acceptArgs = new SocketAsyncEventArgs();
            _acceptArgs.Completed += ProcessAccept;
        }

        public void GetValidated(List<(Socket, TState)> list)
        {
            _validator.GetValidated(list);
        }

        public StartConnectionResponse OpenConnection(IPAddress ipAddress, TState state)
        {
            var response = _validator.OpenConnection(ipAddress, state);
            response.Port = _port;
            return response;
        }

        public void Start()
        {
            _socket.Bind(new IPEndPoint(IPAddress.IPv6Any, _port));
            _socket.Listen();
            if (!_socket.AcceptAsync(_acceptArgs))
            {
                ThreadPool.QueueUserWorkItem(x => ProcessAccept(null, (SocketAsyncEventArgs)x), _acceptArgs);
            }
        }

        public void Stop()
        {
            if (Interlocked.CompareExchange(ref _stopped, 1, 0) == 1)
            {
                return;
            }

            _validator.StopAll();
            _socket.Close();
        }

        private void ProcessAccept(object sender, SocketAsyncEventArgs args)
        {
            while (_stopped == 0)
            {
                if (args.SocketError != SocketError.Success)
                {
                    Debug.LogError("Failed to accept socket with error: {0}", args.SocketError); // TODO internal error
                }
                else
                {
                    var socket = args.AcceptSocket;
                    _validator.StartValidation(socket);
                }

                args.AcceptSocket = null;
                try
                {
                    if (_socket.AcceptAsync(args))
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e, "An error occurred during {0}", nameof(ProcessAccept)); // TODO internal error
                }
            }
        }
    }
}
