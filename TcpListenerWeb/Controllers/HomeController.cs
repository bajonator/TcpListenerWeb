using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NuGet.Configuration;
using NuGet.Protocol.Plugins;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TcpListenerWeb.Core;
using TcpListenerWeb.Core.ViewModels;
using TcpListenerWeb.Extensions;
using TcpListenerWeb.Helpers;

namespace TcpListenerWeb.Controllers
{
    public class HomeController : Controller
    {
        private static TcpListener? listener;
        private static bool isListening = false;
        public static string FilePathSettings = Path.Combine(Environment.CurrentDirectory, "settings.xml");
        private FileHelper<ServerSettingsViewModel> _fileHelperSettings = new FileHelper<ServerSettingsViewModel>(FilePathSettings);

        private readonly IHubContext<NotificationHub> _hubContext;
        public HomeController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public IActionResult Index()
        {
            var model = _fileHelperSettings.DeserializeFromFile();

            if (string.IsNullOrEmpty(model.IpAddress))
                model.IpAddress = "192.168.0.1";

            if (model.Port == 0)
                model.Port = 11000;

            if (string.IsNullOrEmpty(model.Password))
                model.Password = "DefaultPassword";

            return View(model);
        }

        [HttpPost("home/start")]
        public IActionResult Start(ServerSettingsViewModel model)
        {
            if (!isListening)
            {
                Task.Run(() => StartListening(model.IpAddress, model.Port, model.Password));
                isListening = true;
                _fileHelperSettings.SerializeToFile(model);
                return Json(new { status = "running" });
            }
            return Json(new { status = "already running" });
        }

        [HttpPost("home/stop")]
        public IActionResult Stop()
        {
            if (isListening && listener != null)
            {
                listener.Stop();
                isListening = false;
                return Json(new { status = "stopped" });
            }
            return Json(new { status = "not running" });
        }

        private async Task StartListening(string ipAddress, int port, string password)
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse(ipAddress), port);
                listener.Start();
                Console.WriteLine($"Listening on {ipAddress}:{port}"); // Dodaj log

                while (isListening)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("Client connected"); // Dodaj log
                    _ = Task.Run(() => HandleClient(client, password));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StartListening: {ex.Message}"); // Dodaj log błędu
            }
        }

        private async Task HandleClient(TcpClient client, string password)
        {
            try
            {

                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    // Odebranie hasła
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string receivedPassword = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                    if (receivedPassword != password)
                    {
                        client.Close();
                        return;
                    }

                    // Odesłanie potwierdzenia
                    string confirmationMessage = "Password accepted";
                    byte[] confirmationData = Encoding.UTF8.GetBytes(confirmationMessage);
                    await stream.WriteAsync(confirmationData, 0, confirmationData.Length);

                    // Odebranie wiadomości
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"{DateTime.Now} -- {receivedData}");


                    string filePath = "receivedData.txt";
                    using (StreamWriter writer = new StreamWriter(filePath, true, Encoding.UTF8))
                    {
                        await writer.WriteLineAsync($"{DateTime.Now} -- {receivedData}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleClient: {ex.Message}"); // Dodaj log błędu
            }
            finally
            {
                client.Close();
            }
        }

        [HttpPost]
        public IActionResult LoadDataFromFile()
        {
            string filePath = "receivedData.txt";
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    var model = System.IO.File.ReadAllLines(filePath);

                    return Json(new { status = "success", model });
                }
                return Json(new { status = "fail", Error = "Nenalezen soubor." });
            }
            catch (Exception ex)
            {
                return Json(new { status = "fail", Error = ex.Message });
            }


        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
