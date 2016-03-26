using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MD5_V4._0_C
{

    public delegate void MyEventHandler(object source, MyEventArgs e);
    public class tcpMaster
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool run = true;
        private byte[] buffer;
        private int sizeOfbyte = 100000000;
        public event MyEventHandler GotData;
        private int nr;
        
        public tcpMaster(TcpClient client, NetworkStream stream,int nr)
        {
            this.client = client;
            this.stream = stream;
            this.nr = nr;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork; //check if it recieves stuff //cancelasync
            worker.RunWorkerAsync();
        }

        public void sendData(string data)
        {
            byte[] bdata = Encoding.ASCII.GetBytes(data);
            stream.WriteAsync(bdata, 0, bdata.Length); //probably have to communicate the lenght of the file first. Name does not realy matter when the master remembers it
        }
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            buffer = new byte[sizeOfbyte];
            StringBuilder data = new StringBuilder();

            while (run)
            {
                try
                {
                    if (stream.DataAvailable)
                    {
                        int nrbytes = stream.Read(buffer, 0, buffer.Length);
                        data.Append(Encoding.ASCII.GetString(buffer, 0, nrbytes));
                        buffer = new byte[nrbytes + 100];

                        B: //to check if there is more usefull stuff to write away at this time

                        int count = 0;
                        for (int i = 0; i < data.Length; i++)
                        {
                            if (data[i] == ':')
                            {
                                count++;
                                if (count == 2)
                                {
                                    string responseData = data.ToString(1, i - 1); // other borders to remove the :
                                    if (GotData != null)
                                    {
                                        GotData(this, new MyEventArgs(responseData, nr)); //raise event

                                    }
                                    data.Remove(0, i + 1);
                                    goto B;
                                }
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                    throw;
                }

                Thread.Sleep(2);
            }
        }
    }

    public class MyEventArgs : EventArgs
    {
        private object[] EventInfo = new object[2];
        public MyEventArgs(string recieved, int nr)
        {
            EventInfo[0] = recieved;
            EventInfo[1] = nr;
        }
        public object[] GetInfo()
        {
            return EventInfo;
        }
    }
}
