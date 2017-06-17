using AbstractTCPlib;
using System;

namespace Server
{
    public class ClientObject
    {
        public Action<ClientObject> OnFinished;

        public TCPgeneral socket { get; set; }
        public bool HasJob { get; set; }
        public DateTime LastSeen { get; set; }
        public string CurrentJob { get; set; }
        public DateTime JobStarted { get; set; }
        public bool isDone { get; set; }

        public void Finished()
        {
            if (OnFinished != null)
            {
                OnFinished(this);
            }
        }
    }
}
