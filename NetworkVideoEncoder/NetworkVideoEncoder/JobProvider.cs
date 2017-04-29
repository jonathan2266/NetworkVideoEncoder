using AbstractTCPlib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;
using System.IO;

namespace NetworkVideoEncoder
{
    public class JobProvider
    {
        private int currentID = 0;

        private ConcurrentDictionary<int, SlaveObject> slaves;
        private List<Job> jobs;
        private string output;
        private string ffmpegCommand;

        public JobProvider(string ffmpeg, string source, string output)
        {
            this.output = output;

            slaves = new ConcurrentDictionary<int, SlaveObject>();
            jobs = new List<Job>();

            string[] files = Directory.GetFiles(source);

            for (int i = 0; i < files.Length; i++)
            {
                jobs.Add(new Job
                {
                    isGivenAsJob = false,
                    jobURL = files[i],
                    slaveID = -1,
                    isDone = false
                });
            }

            StreamReader reader = new StreamReader(ffmpeg);
            ffmpegCommand = reader.ReadToEnd();
            reader.Close();

        }
        public void RunJobs()
        {
            while (true)
            {
                
            }
        }
        public void AddSlave(TCPgeneral slave)
        {
            int id = generateUniqueID(slave);
            slave.OnError = OnError;
            slave.OnRawDataRecieved = RecievedData;

            SlaveObject obj = new SlaveObject();
            obj.slave = slave;
            obj.HasJob = false;
            obj.LastSeen = DateTime.Now;
            slaves.TryAdd(id, obj);
        }

        private int generateUniqueID(TCPgeneral slave)
        {
            string message = Header.ID.ToString() + ":" + currentID;
            slave.SendTCP(Encoding.ASCII.GetBytes(message));
            currentID++;
            return currentID - 1;
        }
        private void RecievedData(byte[] data)
        {
            int id = getID(data);
        }
        private void OnError(ErrorTypes type, string message)
        {

        }
    }
}
