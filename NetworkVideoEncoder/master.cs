using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkVideoEncoder
{
    class master
    {
        private string directory;
        private List<string> iP;
        private int masterRender;
        private string ffmpegCommand;

        public master(List<string> iP, string directory, int masterRender, string ffmpegCommand)
        {
            this.iP = iP;
            this.directory = directory;
            this.masterRender = masterRender;
            this.ffmpegCommand = ffmpegCommand;
        }
    }
}
