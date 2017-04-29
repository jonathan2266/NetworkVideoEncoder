﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AbstractTCPlib;
using AbstractTCPlib.UDPdiscovery;
using System.Threading;
using System.Net.Sockets;

namespace NetworkVideoEncoder
{
    class Program
    {
        static string usage = "command line input: ffmpeg command file  source folder output folder udpPort";
        static string broadCast = "networkVideoEncoder";
        static string ffmpeg;
        static string source;
        static string output;
        static int port;

        static JobProvider provider;

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine(usage);
            }

            ffmpeg = args[0];
            source = args[1];
            if (!Directory.Exists(source))
            {
                Console.WriteLine("input directory does not excist");
                Console.WriteLine(usage);
                Environment.Exit(0);
            }
            output = args[2];
            if (!Directory.Exists(output))
            {
                Console.WriteLine("output directory does not excist");
                Console.WriteLine(usage);
                Environment.Exit(0);
            }
            if (!int.TryParse(args[3], out port))
            {
                Console.WriteLine("port shoudl be in an int");
                Console.WriteLine(usage);
                Environment.Exit(0);
            }

            provider = new JobProvider(ffmpeg, source, output);

            Thread listentoSlaves = new Thread(new ThreadStart(listen));
            listentoSlaves.IsBackground = true;
            listentoSlaves.Start();

            provider.RunJobs();

        }

        private static void listen()
        {
            UDPmaster master = new UDPmaster(broadCast, port);

            while (true)
            {
                TcpClient client = master.Listen();
                provider.AddSlave(new TCPgeneral(client));
                Thread.Sleep(1);
            }
        }
    }
}