using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AbstractTCPlib;
using AbstractTCPlib.UDPdiscovery;
using System.Net.Sockets;
using System.Threading;
using SharedTypes;
using System.IO;

namespace Client
{
    class Program
    {
        static string broadCast = "networkVideoEncoder";
        static string usage = "command line input: udpPort";
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine(usage);
                Console.ReadLine();
                Environment.Exit(0);
            }
            int port;

            bool ok = int.TryParse(args[0], out port);

            if (!ok)
            {
                Console.WriteLine(usage);
                Console.ReadLine();
                Environment.Exit(0);
            }

            UDPclient udp = new UDPclient(broadCast, port);
            TcpClient client;
            while (true)
            {
                client = udp.Broadcast();

                if (client != null)
                {
                    client.SendBufferSize = 64000;
                    client.ReceiveBufferSize = 64000; //needed for linux
                    break;
                }
            }

            clearOutFile();

            TCPgeneral gen = new TCPgeneral(client, 0);

            JobHandler job = new JobHandler(gen);

            while (true)
            {
                job.start();
            }
        }

        private static void clearOutFile()
        {
            //var files = Directory.GetFiles("", "OUT");

            //foreach (var file in files)
            //{
            //    File.Delete(file);
            //}
        }
    }
}
