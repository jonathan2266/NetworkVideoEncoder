using AbstractTCPlib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkVideoEncoder
{
    public class SlaveObject
    {
        public TCPgeneral slave { get; set; }
        public bool HasJob { get; set; }
        public DateTime LastSeen { get; set; }
        public string CurrentJob { get; set; }
        public DateTime JobStarted { get; set; }
    }
}
