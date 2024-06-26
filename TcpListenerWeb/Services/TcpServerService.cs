//using System.Net.Sockets;
//using System.Net;
//using System.Text;
//using System.Threading;
//using Microsoft.AspNetCore.SignalR;
//using TcpListenerWeb.Hubs;

//namespace TcpListenerWeb.Services
//{
//    public class TcpServerService : BackgroundService
//    {
//        private readonly ILogger<TcpServerService> _logger;
//        private TcpListener _listener;
//        private const string SecretPassword = "DensoServerFIOT";
//        private readonly string _ip = "192.168.31.196";
//        private readonly int _port = 11000;
//        private readonly IHubContext<NotificationHub> _hubContext;

//        public TcpServerService(ILogger<TcpServerService> logger, IHubContext<NotificationHub> hubContext)
//        {
//            _logger = logger;
//            _hubContext = hubContext;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            _listener = new TcpListener(IPAddress.Parse(_ip), _port);
//            _listener.Start();
//            _logger.LogInformation($"Server started on {_ip}:{_port}");

//            while (!stoppingToken.IsCancellationRequested)
//            {
//                if (_listener.Pending())
//                {
//                    TcpClient client = await _listener.AcceptTcpClientAsync();
//                    _ = HandleClientAsync(client, stoppingToken);
//                }
//                await Task.Delay(1000, stoppingToken);
//            }

//            _listener.Stop();
//            _logger.LogInformation("Server stopped.");
//        }

//        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
//        {
//            using (client)
//            using (NetworkStream stream = client.GetStream())
//            {
//                byte[] buffer = new byte[1024];
//                int bytesRead;

//                // Receive password
//                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
//                string receivedPassword = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

//                if (receivedPassword != SecretPassword)
//                {
//                    _logger.LogWarning("Invalid password received.");
//                    client.Close();
//                    return;
//                }

//                // Send confirmation
//                string confirmationMessage = "Password accepted";
//                byte[] confirmationData = Encoding.UTF8.GetBytes(confirmationMessage);
//                await stream.WriteAsync(confirmationData, 0, confirmationData.Length, cancellationToken);
//                _logger.LogInformation("Confirmation sent to client.");

//                // Receive message
//                using (MemoryStream ms = new MemoryStream())
//                {
//                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
//                    ms.Write(buffer, 0, bytesRead);

//                    string receivedData = Encoding.UTF8.GetString(ms.ToArray());
//                    _logger.LogInformation($"Data received: {receivedData}");

//                    string filePath = Path.Combine("wwwroot", "receivedData.txt");
//                    await File.AppendAllTextAsync(filePath, $"{DateTime.Now} -- {receivedData}{Environment.NewLine}");
//                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"{DateTime.Now} -- {receivedData}");

//                    _logger.LogInformation($"Data appended to file {filePath}");
//                }
//            }
//        }
//    }
//}