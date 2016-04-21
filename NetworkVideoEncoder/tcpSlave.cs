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
        public tcpSlave(TcpClient client, NetworkStream stream)
        {
            this.client = client;
            this.stream = stream;
        }
        public void Recieve()
        {
            byte[] buffer = new byte[50000000];
            byte[] videoBuffer = new byte[100000000];
            int videoBufferFilled = 0;
            while (true)
            {
                if (stream.DataAvailable)
                {
                    int nrbytes = stream.Read(buffer, 0, buffer.Length);

                    //now lets see if there are 6 ':' to be found
                    int count = 0;
                    bool passedCheck = false;

                    if (passedCheck)
                    {
                        if (buffer[0] == ':')
                        {
                            GotData(this, new MyeventArgs(2, buffer));
                            break;
                        }
                        else
                        {
                            for (int i = 0; i < nrbytes; i++)
                            {
                                videoBuffer[i + videoBufferFilled] = buffer[i];
                            }
                            videoBufferFilled += nrbytes;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            if (buffer[i] == ':')
                            {
                                count++;
                                if (count == 4 && i + 1 == buffer.Length)
                                {
                                    //get the data and break
                                    GotData(this, new MyeventArgs(0, buffer));
                                    buffer = new byte[50000000];
                                    passedCheck = true;
                                    break;
                                }
                                else if (count == 4)
                                {
                                    //filter out the correct part and calc the remaining stuff
                                    passedCheck = true;
                                }
                            }
                        }
                    }
                }
            }
            //byte[] recieved = new byte[50000000];
            //stream.Read(recieved, 0, recieved.Length);
            //string final = Encoding.ASCII.GetString(recieved);
            //string final2 =  final.Trim('\0');
        }

        public void sendData(string data)
        {
            string final = ":" + data + ":";
            byte[] send = Encoding.ASCII.GetBytes(final);
            stream.WriteAsync(send, 0, send.Length); //fixed everything for some reason
        }
    }
}
