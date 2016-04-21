using MD5_V4._0_C;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

        private string[] videos; //vids found in dir that needs to be processed
        private int[] CompletedVideos; //keeps track of the onces that are done  0 not sended 1 sended but not finished 2 finished

        private List<tcpMaster> listOfConnections;
        private bool[] slaveIsBusy;
        private bool[] recievedHeader;
        private int[] jobGivenToSlave; //stores int of videos

        public master(List<string> iP, string directory, int masterRender, string ffmpegCommand, int port)
        {
            this.iP = iP;
            this.directory = directory;
            this.masterRender = masterRender;
            this.ffmpegCommand = ffmpegCommand;
            this.port = port;

            fileCheck();
            connectToSlaves();
            sizeArrays();
            while (true)
            {
                startJobs();
            }
            
        }

        private void startJobs()
        {
            for (int i = 0; i < slaveIsBusy.Length; i++)
            {
                if (slaveIsBusy[i] == false)
                {
                    int videoToSend = -1;
                    for (int j = 0; j < CompletedVideos.Length; j++)
                    {
                        if (CompletedVideos[j] == 0)
                        {
                            videoToSend = j;
                            CompletedVideos[j] = 1;
                            break;
                        }
                    }
                    if (videoToSend != -1)
                    {
                        listOfConnections[i].sendData(videos[videoToSend],ffmpegCommand);
                        jobGivenToSlave[i] = videoToSend;
                        slaveIsBusy[i] = true;
                    }
                    else
                    {
                        //all vids are done
                    }

                }
            }

            Thread.Sleep(100);
        }

        private void sizeArrays()
        {
            slaveIsBusy = new bool[listOfConnections.Count];
            recievedHeader = new bool[listOfConnections.Count];
            jobGivenToSlave = new int[listOfConnections.Count];

            CompletedVideos = new int[videos.Length];

            for (int i = 0; i < listOfConnections.Count; i++)
            {
                slaveIsBusy[i] = false;
                recievedHeader[i] = false;
                jobGivenToSlave[i] = 0;
            }

            for (int i = 0; i < videos.Length; i++)
            {
                CompletedVideos[i] = 0;
            }

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
