using MD5_V4._0_C;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using test;

namespace NetworkVideoEncoder
{
    class master
    {
        private string directory;
        private List<string> iP;
        private int masterRender;
        private string ffmpegCommand;
        private int port;

        private string[] videos;
        private List<tcpMaster> listOfConnections;

        public master(List<string> iP, string directory, int masterRender, string ffmpegCommand, int port)
        {
            this.iP = iP;
            this.directory = directory;
            this.masterRender = masterRender;
            this.ffmpegCommand = ffmpegCommand;
            this.port = port;

            fileCheck();
            connectToSlaves();
            while (true)
            {
                startJobs();
            }
            
        }

        private void startJobs()
        {
            
        }

        private void connectToSlaves()
        {
            int nr = -1;

            for (int i = 0; i < iP.Count; i++)
            {
                try
                {
                    TcpClient client = new TcpClient(iP[i].ToString(), port); // also open this port on linux :p
                    NetworkStream stream = client.GetStream();
                    nr++;
                    tcpMaster connection = new tcpMaster(client, stream, nr);
                    connection.GotData += Connection_GotData;
                    connection.GotEx += Connection_GotEx;
                    listOfConnections.Add(connection);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("connectToSlave failed " + e);
                }
            }
        }

        private void Connection_GotEx(object source, MyEventArgsEx e)
        {
            throw new NotImplementedException();
        }

        private void Connection_GotData(object source, MyeventArgs e)
        {
            throw new NotImplementedException();
        }

        private void fileCheck()
        {
            DateTime[] swag; //useless :p

            FolderScanner scanner = new FolderScanner(directory);
            scanner.RefreshIndex();
            scanner.getAlIndex(out videos, out swag);
            scanner = null;
            swag = null;
        }
    }
}
