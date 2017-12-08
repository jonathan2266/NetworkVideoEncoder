using System.Collections.Generic;

namespace Server
{
    public static class ClientDataBlock
    {
        private static object locker = new object();

        private static List<ClientObject> _clients = new List<ClientObject>();

        public static List<ClientObject> Clients
        {
            get
            {
                lock (locker)
                {
                    return _clients;
                }
            }
            set
            {
                lock (locker)
                {
                    _clients = value;
                }
            }
        }
    }
}
