using AbstractTCPlib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;
using System.IO;
using System.Threading;

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
                foreach (var slave in slaves)
                {
                    SlaveObject obj = slave.Value;

                    if (!obj.HasJob && obj.slave.IsAlive)
                    {
                        giveJob(slave);
                        slave.Value.HasJob = true;
                        sendJob(slave);
                    }
                }
            }
        }

        private void sendJob(KeyValuePair<int, SlaveObject> slave) //run on manage thread need class to manage send
        {
            FileStream stream = File.OpenRead(slave.Value.CurrentJob);
            int l = 100000000; // = 100MB
            byte[] data = new byte[l];

            while (true)
            {
                int read = stream.Read(data, 0, l);
                slave.Value.slave.SendTCP(data);

                if (read != l)
                {
                    break;
                }
            }
        }

        private void giveJob(KeyValuePair<int, SlaveObject> slave)
        {
            foreach (var job in jobs)
            {
                if (!job.isGivenAsJob)
                {
                    job.isGivenAsJob = true;
                    job.slaveID = slave.Key;
                    slave.Value.CurrentJob = job.jobURL;
                    string command = Header.job.ToString() + ":" + job.jobURL;
                    slave.Value.slave.SendTCP(Encoding.ASCII.GetBytes(command));
                }
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

        private int getID(byte[] data)
        {
            
        }

        private void OnError(ErrorTypes type, string message)
        {

        }
    }
}
