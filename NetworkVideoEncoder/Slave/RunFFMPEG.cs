using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slave
{
    public class RunFFMPEG
    {
        private string binLocationL = @"/usr/bin/ffmpeg";
        private Process ffmpegProcess;
        private string binaryW = @"ffmpeg.exe";
        private StreamWriter input;

        public RunFFMPEG(string command)
        {
            ffmpegProcess = new Process();
            if (IsLinux())
            {
                ffmpegProcess.StartInfo.FileName = binLocationL;
                ffmpegProcess.StartInfo.CreateNoWindow = true;
                ffmpegProcess.StartInfo.UseShellExecute = false;
            }
            else
            {
                ffmpegProcess.StartInfo.FileName = binaryW;
                ffmpegProcess.StartInfo.CreateNoWindow = false;
                ffmpegProcess.StartInfo.UseShellExecute = true;
            }
            ffmpegProcess.StartInfo.Arguments = command;

            //ffmpegProcess.StartInfo.RedirectStandardInput = true;
            ffmpegProcess.EnableRaisingEvents = true;
            ffmpegProcess.ErrorDataReceived += FfmpegProcess_ErrorDataReceived;
            ffmpegProcess.OutputDataReceived += FfmpegProcess_OutputDataReceived;
        }

        private void FfmpegProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data.Contains("already exists."))
            {
                //input.WriteLine("y");
                Console.WriteLine("File already exists... overwriting");
            }
        }

        private void FfmpegProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("ffmpeg error: " + e.Data);
        }

        public void Start()
        {
            ffmpegProcess.Start();
            //input = ffmpegProcess.StandardInput;
            ffmpegProcess.WaitForExit();
        }

        private bool IsLinux()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
    }
}
