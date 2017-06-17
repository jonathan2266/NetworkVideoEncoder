using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class RunFFMPEG
    {
        private string binLocationL = @"/usr/bin/ffmpeg";
        private Process ffmpegProcess;
        private string binaryW = @"ffmpeg.exe";

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
        }

        public void Start()
        {
            ffmpegProcess.Start();
            ffmpegProcess.WaitForExit();
        }

        private bool IsLinux()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
    }
}
