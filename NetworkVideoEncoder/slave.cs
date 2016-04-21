using MD5_V4._0_C;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkVideoEncoder
{
    class slave
    {
        private int port;
        private tcpSlave s;
        private string ffmpegCommand = "";
        string extension = "";
        bool recieving = true;

        public slave(int port)
        {
            this.port = port;
            listenToMaster();

            while (true)
            {
                recieveJob();
            }
        }

        private void S_GotData(object source, MyeventArgs e)
        {


            object[] eventInfo = e.GetInfo();
            int nr = Convert.ToInt16(eventInfo[0]);
            byte[] buffer = eventInfo[1] as byte[];

            if (nr == 0)
            {
                string temp = Encoding.ASCII.GetString(buffer);
                string[] data = temp.Split(':');

                long size = Convert.ToInt64(data[0]);
                extension = data[1];
                ffmpegCommand = data[2];
            }
            if (nr == 1)
            {
                if (!File.Exists("original" + extension))
                {
                    File.Create("original" + extension);
                }
                else
                {
                    StreamWriter writer = File.AppendText("original" + extension);
                    writer.Write(buffer);
                    writer.Close();
                }
            }
            if (nr == 2)
            {
                recieving = false;
            }
        }

        private void listenToMaster()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();
            NetworkStream NStream = client.GetStream();
            listener = null;

            s = new tcpSlave(client, NStream);
            s.GotData += S_GotData;
        }

        private void recieveJob()
        {
            extension = "";
            ffmpegCommand = "";
            recieving = true;

            s.Recieve();

            while (recieving)
            {
                Thread.Sleep(10);
            }

            ProcessStartInfo ffmpeg = new ProcessStartInfo();
            ffmpeg.Arguments = ffmpegCommand;
            ffmpeg.FileName = "ffmpeg.exe";
            ffmpeg.WindowStyle = ProcessWindowStyle.Hidden;
            ffmpeg.CreateNoWindow = true;

            using (Process proc = Process.Start(ffmpeg))
            {
                proc.WaitForExit();
            }
            File.Delete("original" + extension);
            s.sendData("new");
        }
    }
}
