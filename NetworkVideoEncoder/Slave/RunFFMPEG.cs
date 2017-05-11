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
        private string binLocationL = @"/usr/bin/";
        private Process ffmpegProcess;
        private string binaryL = "ffmpeg";
        private string binaryW = @"ffmpeg.exe";
        private StreamReader error;
        private StreamReader output;

        public RunFFMPEG(string command)
        {
            ffmpegProcess = new Process();
            if (IsLinux())
            {
                ffmpegProcess.StartInfo.FileName = binLocationL + binaryL;
            }
            else
            {
                ffmpegProcess.StartInfo.FileName = binaryW;
            }
            ffmpegProcess.StartInfo.Arguments = command;
            ffmpegProcess.StartInfo.CreateNoWindow = false;
            ffmpegProcess.StartInfo.UseShellExecute = true;
            //ffmpegProcess.StartInfo.RedirectStandardError = true;
            //ffmpegProcess.StartInfo.RedirectStandardOutput = true;
        }

        public void Start()
        {
            ffmpegProcess.Start();

            ffmpegProcess.WaitForExit();
            //error = ffmpegProcess.StandardError;
            //output = ffmpegProcess.StandardOutput;
            //ffmpegProcess.WaitForExit();

            //string er = error.ReadToEnd();
            //string outs = output.ReadToEnd();


        }

        private bool IsLinux()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
    }
}
