﻿using AbstractTCPlib;
using SharedTypes;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Client
{
    public class JobHandler
    {
        private TCPgeneral gen;
        private string ffmpegCommand;
        private string job;
        private FileStream stream;
        private RunFFMPEG ffmpeg;
        private string outputFile;
        private bool sendCompleted;
        private ManualResetEvent reset;

        public JobHandler(TCPgeneral gen)
        {
            reset = new ManualResetEvent(false);
            sendCompleted = false;
            this.gen = gen;
            gen.OnRawDataRecieved += OnRecieved;
            gen.OnError += OnError;
        }
        public void Start()
        {
            gen.Start();


            reset.WaitOne();

            reset.Reset();

        }
        private void SendVidPiece()
        {
            if (sendCompleted)
            {
                gen.SendTCP(Headers.SendCompleted);
                stream.Close();
                File.Delete(Path.Combine(Resources.OutputFolder, outputFile));

                sendCompleted = false;
                ffmpegCommand = null;
                job = null;

                reset.Set();

            }
            else
            {
                int l = 10000000; // = 10MB
                byte[] data = new byte[l];

                int read = stream.Read(data, 0, l);
                if (read != l)
                {
                    byte[] cropped = new byte[read];
                    Array.Copy(data, 0, cropped, 0, read);
                    gen.SendTCP(Headers.AssembleHeader(Headers.PieceOfVideo, cropped));
                    sendCompleted = true;
                }
                else
                {
                    gen.SendTCP(Headers.AssembleHeader(Headers.PieceOfVideo, data));
                }
            }
        }
        private void OnRecieved(int id, byte[] rawData)
        {
            byte[] header = Headers.GetHeaderFromData(rawData);

            if (Headers.FfmpegCommand.SequenceEqual(header))
            {
                byte[] data;
                Headers.SplitData(rawData, out header, out data);
                ffmpegCommand = @Encoding.ASCII.GetString(data);

                string formatted = ffmpegCommand.Replace(Resources.ffmpegDATA, Path.Combine(Resources.InputFolder, @job));

                Guid uniqueName = Guid.NewGuid();
                outputFile = uniqueName.ToString();
                int ind = formatted.IndexOf(Resources.ffmpegOUT);
                ind += 3;

                for (int i = ind; i < formatted.Length; i++)
                {
                    if (formatted[i] != '"')
                    {
                        outputFile += formatted[i];
                    }
                    else
                    {
                        break;
                    }
                }

                formatted = formatted.Replace(Resources.ffmpegOUT, Path.Combine(Resources.OutputFolder, uniqueName.ToString()));

                ffmpeg = new RunFFMPEG(formatted);
                Console.WriteLine("command:" + formatted);
            }
            else if (Headers.Job.SequenceEqual(header))
            {
                byte[] data;
                Headers.SplitData(rawData, out header, out data);
                stream = new FileStream(Path.Combine(Resources.InputFolder, Encoding.ASCII.GetString(data)), FileMode.Append);
                job = @Encoding.ASCII.GetString(data);
                Console.WriteLine("job " + job);
            }
            else if (Headers.PieceOfVideo.SequenceEqual(header))
            {
                byte[] vid;
                Headers.SplitData(rawData, out header, out vid);
                stream.Write(vid, 0, vid.Length);
                gen.SendTCP(Headers.SendNext);
            }
            else if (Headers.SendCompleted.SequenceEqual(header))
            {
                stream.Close();
                stream = null;
                Console.WriteLine("Recieved all");
                ffmpeg.Start();
                stream = new FileStream(Path.Combine(Resources.OutputFolder, outputFile), FileMode.Open);
                File.Delete(Path.Combine(Resources.InputFolder, @job));
                gen.SendTCP(Headers.RenderCompleted);
            }
            else if (Headers.SendNext.SequenceEqual(header))
            {
                SendVidPiece();
            }
        }

        private void OnError(int id, ErrorTypes type, string error)
        {
            Console.WriteLine("error: " + type.ToString() + " " + error);
        }
    }
}
