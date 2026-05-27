using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LoteriaTwo.Services
{
    public enum ConnectionState { Disconnected, Connecting, Connected }

    public class BrainstormConnection : IDisposable
    {
        private const int Port = 5123;
        private const int TimeoutSeconds = 5;

        private TcpClient? _client;
        private string _ip;

        public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
        public event Action<ConnectionState>? StateChanged;

        public string Ip => _ip;

        public BrainstormConnection(string ip)
        {
            _ip = ip;
        }

        public async Task<bool> ConnectAsync(CancellationToken ct = default)
        {
            Disconnect();

            State = ConnectionState.Connecting;
            StateChanged?.Invoke(State);

            try
            {
                _client = new TcpClient();
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);

                await _client.ConnectAsync(_ip, Port, linked.Token);

                State = ConnectionState.Connected;
                StateChanged?.Invoke(State);
                return true;
            }
            catch
            {
                _client?.Dispose();
                _client = null;
                State = ConnectionState.Disconnected;
                StateChanged?.Invoke(State);
                return false;
            }
        }

        public void Disconnect()
        {
            _client?.Dispose();
            _client = null;

            if (State != ConnectionState.Disconnected)
            {
                State = ConnectionState.Disconnected;
                StateChanged?.Invoke(State);
            }
        }

        public void UpdateIp(string ip) => _ip = ip;

        public bool Send(string command)
        {
            if (_client is null || !_client.Connected) return false;
            try
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(command);
                _client.GetStream().Write(bytes, 0, bytes.Length);
                return true;
            }
            catch
            {
                Disconnect();
                return false;
            }
        }

        public void Dispose() => Disconnect();
    }
}
