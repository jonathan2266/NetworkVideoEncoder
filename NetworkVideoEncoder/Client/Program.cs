using System;
using AbstractTCPlib;
using AbstractTCPlib.UDPdiscovery;
using System.Net.Sockets;
using System.IO;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine(Resources.Usage);
                Console.ReadLine();
                Environment.Exit(0);
            }
            int port;

            bool ok = int.TryParse(args[0], out port);

            if (!ok)
            {
                Console.WriteLine(Resources.Usage);
                Console.ReadLine();
                Environment.Exit(0);
            }

            UDPclient udp = new UDPclient(Resources.BroadCast, port);
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

            CreateFolders(Resources.InputFolder);
            CreateFolders(Resources.OutputFolder);

            ClearFolder(Resources.InputFolder);
            ClearFolder(Resources.OutputFolder);

            TCPgeneral gen = new TCPgeneral(client, 0);

            JobHandler job = new JobHandler(gen);

            while (true)
            {
                job.Start();
            }
        }

        private static void CreateFolders(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        private static void ClearFolder(string folder)
        {
            var files = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder));

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
