using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TcpListenerWeb.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
