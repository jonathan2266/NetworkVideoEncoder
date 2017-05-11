using AbstractTCPlib;
using System;

namespace NetworkVideoEncoder
{
    public class SlaveObject
    {
        public TCPgeneral socket { get; set; }
        public bool HasJob { get; set; }
        public DateTime LastSeen { get; set; }
        public string CurrentJob { get; set; }
        public DateTime JobStarted { get; set; }
        public bool isDone { get; set; }
    }
}
