using System.Net;
using System.Net.Mail;

namespace Polyclinic.Models.Email
{
    public partial class EmailSetting
    {
        public string AddressEmail { get; set; }

        public string AddressSMPT { get; set; }

        public int Port { get; set; }

        public string Password { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}