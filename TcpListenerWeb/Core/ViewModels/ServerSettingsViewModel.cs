using System.ComponentModel.DataAnnotations;

namespace TcpListenerWeb.Core.ViewModels
{
    public class ServerSettingsViewModel
    {
        [Display(Name = "IP Adress")]
        public string? IpAddress { get; set; }
        public int Port { get; set; }
        public string? Password { get; set; }
        public string LastReceivedMessage { get; set; }
    }
}
