using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace test
{
    public class FolderScanner
    {
        public FolderScanner(string Directory1){
            directory1 = Directory1;
            deeperDir = directory1;
        }

        private string directory1;
        private string deeperDir;
        private List<string> files1 = new List<string>();
        private List<DateTime> files1Date = new List<DateTime>();
        public string Directory1
        {
            set
            {
                files1.Clear();
                files1Date.Clear();
                directory1 = value;
            }
            get { return directory1; }
        }
        public void RefreshIndex()
        {
            files1.Clear();
            files1Date.Clear();
            refresh(directory1,files1,files1Date);
        }
        public void getAlIndex(out string[] index, out DateTime[] indexDate) {
            index = files1.ToArray();
            indexDate = files1Date.ToArray<DateTime>();
        }
        private void refresh(string dir, List<string> listContainer,List<DateTime> listDate)
        {
            string[] files = Directory.GetFiles(dir);
            foreach (var file in files)
            {
                if (file != deeperDir + "\\Thumbs.db" && file != deeperDir + "\\LansIndexFile.jon")
                {
                    DateTime date = File.GetLastWriteTime(file);
                    listDate.Add(date);
                    listContainer.Add(file);
                }
                
            }
            string[] deeperDirectories = Directory.GetDirectories(dir);
            foreach (string foundedDirs in deeperDirectories)
            {
                deeperDir = foundedDirs;
                refresh(foundedDirs,listContainer,listDate);
            }
        }
    }
}
