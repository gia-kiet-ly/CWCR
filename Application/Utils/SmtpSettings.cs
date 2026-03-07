using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utils
{
    public class SmtpSettings
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; }
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public bool EnableSsl { get; set; } = true;
        public string FromEmail { get; set; } = default!;
        public string FromName { get; set; } = default!;
    }
}
