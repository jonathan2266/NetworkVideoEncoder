using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MD5_V4._0_C
{

    public delegate void MyEventHandler(object source, MyeventArgs e);
    public delegate void MyEventHandlerEx(object source, MyEventArgsEx e);
    public class tcpMaster
    {
        private TcpClient client;
        private NetworkStream stream;
        public event MyEventHandler GotData;
        public event MyEventHandlerEx GotEx;
        private int nr;
        private string fileLocation;
        
        public tcpMaster(TcpClient client, NetworkStream stream,int nr)
        {
            this.client = client;
            this.stream = stream;
            this.nr = nr;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += recieveHelper; //check if it recieves stuff //cancelasync
            worker.RunWorkerAsync();
        }

        public void sendData(string fileDirectory, string ffmpegCommand)
        {

            fileLocation = fileDirectory;
            BackgroundWorker sendHelper = new BackgroundWorker();
            sendHelper.DoWork += SendHelper_DoWork;
            object[] info = new object[2];
            info[1] = fileDirectory;
            info[2] = ffmpegCommand;
            sendHelper.RunWorkerAsync(info);
        }

        private void SendHelper_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] arguments = e.Argument as object[];

            string fileDirectory = arguments[0] as string;
            string ffmpegCommand = arguments[1] as string;

            FileInfo info = new FileInfo(fileDirectory);
            long size = info.Length;
            string extension = Path.GetExtension(fileDirectory);
            string fileInfo = ":" + size + ":" + extension + ":" + ffmpegCommand + ":";

            byte[] data = Encoding.ASCII.GetBytes(fileInfo);
            stream.Write(data,0,data.Length);
            
            Thread.Sleep(50);

            Stream s = File.OpenRead(fileDirectory);
            byte[] swag = new byte[50000000];
            int bytesread;
            while ((bytesread = s.Read(swag, 0, swag.Length)) > 0)
            {
                stream.Write(swag, 0, bytesread);
            }

            //data = Encoding.ASCII.GetBytes(":END:");
            //stream.Write(data, 0, data.Length);
        }

        private void recieveHelper(object sender, DoWorkEventArgs e)
        {
            byte[] buffer = new byte[500];
            bool firstRecieved = false;
            long size = 0;
            string extension;
            long amountRecieved = 0;

            while (true)
            {
                try
                {
                    if (stream.DataAvailable)
                    {
                        int nrbytes = stream.Read(buffer, 0, buffer.Length);
                        if (firstRecieved == false)
                        {
                            analyzeRecieved(nrbytes, buffer, out size, out extension);
                            GotEx(this, new MyEventArgsEx(nr, extension, size));
                            buffer = new byte[100 * 1000 * 1000]; //same as 100MB not dem M$ MiB -__-
                            firstRecieved = true;
                        }
                        else
                        {
                            amountRecieved += nrbytes;
                            GotData(this, new MyeventArgs(nr, buffer));
                            if (amountRecieved == size)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private void analyzeRecieved(int nrbytes, byte[] buffer, out long size, out string extension)
        {
            long sizeT = 0;
            string extensionT = "";
            string temp = "";

            int counter = 0;
            string data = Encoding.ASCII.GetString(buffer, 0, nrbytes);
            char[] convertedData = data.ToCharArray();
            char[] fileL = filter1.ToCharArray();


            for (int i = 0; i < convertedData.Length; i++)
            {
                A:
                if (convertedData[i] == filter1[counter])
                {
                    counter++;
                    if (counter == filter1.Length)
                    {
                        #region
                        for (int j = i; j < convertedData.Length; j++)
                        {
                            if (convertedData[j] != ':')
                            {
                                temp += convertedData[j];
                            }
                            else
                            {
                                sizeT = Convert.ToInt64(temp);
                                temp = "";
                                break;
                            }
                        }
                        #endregion
                    }
                    i++;
                    goto A;
                }
                else
                {
                    counter = 0;
                }

                B:
                if (convertedData[i] == filter2[counter])
                {
                    counter++;
                    if (counter == filter2.Length)
                    {
                        #region
                        for (int j = i; j < convertedData.Length; j++)
                        {
                            if (convertedData[j] != ':')
                            {
                                temp += convertedData[j];
                            }
                            else
                            {
                                extensionT = temp;
                                temp = "";
                                break;
                            }
                        }
                        #endregion
                    }
                    i++;
                    goto B;
                }
                else
                {
                    counter = 0;
                }
            }

            size = sizeT;
            extension = extensionT;

        }

        private string filter1 = ":FileLenght=";
        private string filter2 = ":Extension=";

    }



    public class MyEventArgsEx : EventArgs
    {
        private object[] EventInfo = new object[3];
        public MyEventArgsEx(int nr , string extension, long size)
        {
            EventInfo[0] = nr;
            EventInfo[1] = extension;
            EventInfo[2] = size;
        }
        public object[] GetInfo()
        {
            return EventInfo;
        }
    }

    public class MyeventArgs : EventArgs
    {
        private object[] EventInfo = new object[2];
        public MyeventArgs(int nr, byte[] recieved)
        {
            EventInfo[0] = nr;
            EventInfo[1] = recieved;
        }
        public object[] GetInfo()
        {
            return EventInfo;
        }
    }
}
