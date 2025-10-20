using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDesktopApp.Configuration.POCO
{
    public class KeycloakSettings
    {
        public string Server { get; set; }
        public string Realm { get; set; }
        public string Client { get; set; }
        public int RedirectPort { get; set; }
        public bool NeedRole { get; set; }
    }
}
