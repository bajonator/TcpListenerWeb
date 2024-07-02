using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TcpListenerWeb.Core
{
    public class NotificationHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
