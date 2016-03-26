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
        private string extension;
        private List<string> iP;
        private int masterRender;

        public master(List<string> iP, string directory, string extension, int masterRender)
        {
            this.iP = iP;
            this.directory = directory;
            this.extension = extension;
            this.masterRender = masterRender;
        }
    }
}
