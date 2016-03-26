using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace MD5_V4._0_C
{
    public class tcpSlave
    {
        private NetworkStream stream;
        private TcpClient client;
        public tcpSlave(TcpClient client, NetworkStream stream)
        {
            this.client = client;
            this.stream = stream;
        }
        public string Recieve()
        {
            while (true)
            {
                if (stream.DataAvailable)
                {
                    break;
                }
                Thread.Sleep(10);
            }
            byte[] recieved = new byte[80];
            stream.Read(recieved, 0, recieved.Length);
            string final = Encoding.ASCII.GetString(recieved);
            string final2 =  final.Trim('\0');
            return final2;
        }
        public void SendStuff(string data)
        {
            string final = ":" + data + ":";
            byte[] send = Encoding.ASCII.GetBytes(final);
            stream.WriteAsync(send, 0, send.Length); //fixed everything for some reason
        }
    }
}
