using System.Text;
using System.Net.Sockets;
using System.Threading;
using System;

namespace MD5_V4._0_C
{
    public class tcpSlave
    {
        private NetworkStream stream;
        private TcpClient client;
        public event MyEventHandler GotData;
        public event MyEventHandlerEx GotEx;
        public tcpSlave(TcpClient client, NetworkStream stream)
        {
            this.client = client;
            this.stream = stream;
        }
        public void Recieve()
        {
            byte[] buffer = new byte[50000000];
            bool recievedHeader = false;

            while (true)
            {
                if (stream.DataAvailable)
                {
                    int nrbytes = stream.Read(buffer, 0, buffer.Length);

                    //now lets see if there are 4 ':' to be found
                    int count = 0;
                    bool passedCheck = false;

                    for (int i = 0; i < buffer.Length; i++)
                    {
                        if (buffer[i] == ':')
                        {
                            count++;
                            if (count == 4 && i + 1 == buffer.Length)
                            {
                                //get the data and break
                                getHeader(buffer); 
                                passedCheck = true;
                                break;
                            }
                            else if (count == 4)
                            {
                                //filter out the correct part and calc the remaining stuff
                                getHeader(buffer);
                                passedCheck = true;
                            }
                        }
                    }

                    if (passedCheck)
                    {

                    }
                    else
                    {

                    }
                }
            }
            byte[] recieved = new byte[50000000];
            stream.Read(recieved, 0, recieved.Length);
            string final = Encoding.ASCII.GetString(recieved);
            string final2 =  final.Trim('\0');
            return final2;
        }

        private void getHeader(byte[] buffer)
        {
            string FL = "";
            string Ex = "";
            bool first = true;

            for (int i = 0; i < buffer.Length; i++) 
            {
                if (buffer[i] == '=') 
                {
                    for (int j = i + 1; j < buffer.Length; j++) 
                    {

                        if (buffer[i] != ':')
                        {
                            if (first)
                            {
                                FL += buffer[j];
                            }
                            else
                            {
                                Ex += buffer[j];
                            }

                        }
                        else
                        {
                            first = false;
                            break;
                        }
                     }
                }
            }
            

        }

        public void SendStuff(string data)
        {
            string final = ":" + data + ":";
            byte[] send = Encoding.ASCII.GetBytes(final);
            stream.WriteAsync(send, 0, send.Length); //fixed everything for some reason
        }

        private string filter1 = ":FileLenght=";
        private string filter2 = ":Extension=";

    }
}
