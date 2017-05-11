using AbstractTCPlib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharedTypes;
using System.Linq;
using System.Threading;

namespace NetworkVideoEncoder
{
    public class JobProvider
    {
        private List<SlaveObject> slaves;
        private List<Job> jobs;
        private string output;
        private string ffmpegCommand;
        private StreamHelper streamer;
        private bool isDone;

        public JobProvider(string ffmpeg, string source, string output)
        {
            this.output = output;
            streamer = new StreamHelper(4, source, output);
            isDone = false;

            slaves = new List<SlaveObject>();
            jobs = new List<Job>();

            string[] files = Directory.GetFiles(source);

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }

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
            while (!isDone)
            {
                lock (slaves)
                {
                    foreach (var slave in slaves)
                    {
                        if (!slave.HasJob && slave.socket.IsAlive)
                        {
                            giveJob(slave);
                            slave.HasJob = true;
                            streamer.AddSlaveToSendQeue(slave);
                        }
                        else if (slave.isDone && slave.socket.IsAlive)
                        {
                            Job jo = jobs.Where(j => j.slaveID == slave.socket.ID).FirstOrDefault();
                            Console.WriteLine("job: " + jo.jobURL + " is done");
                            jo.isDone = true;
                            jo.slaveID = -1;
                            slave.HasJob = false;
                            slave.isDone = false;
                            slave.JobStarted = DateTime.MinValue;
                            slave.LastSeen = DateTime.Now;
                        }
                        else if (!slave.socket.IsAlive)
                        {
                            Console.WriteLine("lost connection with " + slave.socket.ID);
                            Job jo = jobs.Where(j => j.slaveID == slave.socket.ID).FirstOrDefault();

                            if (jo != null)
                            {
                                jo.isGivenAsJob = false;
                                jo.isDone = false;
                                jo.slaveID = -1;
                            }
                            slave.socket.Dispose();
                        }
                    }

                    for (int i = 0; i < slaves.Count; i++)
                    {
                        if (slaves[i].socket.IsAlive == false)
                        {
                            slaves.RemoveAt(i);
                            i--;
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }

        private void giveJob(SlaveObject obj)
        {
            foreach (var job in jobs)
            {
                if (!job.isGivenAsJob)
                {
                    job.isGivenAsJob = true;
                    job.slaveID = obj.socket.ID;
                    obj.CurrentJob = job.jobURL;
                    obj.socket.SendTCP(Headers.AssembleHeader(Headers.Job, Encoding.ASCII.GetBytes(job.jobURL)));
                    obj.socket.SendTCP(Headers.AssembleHeader(Headers.ffmpegCommand, Encoding.ASCII.GetBytes(ffmpegCommand)));
                    break;
                }
            }
        }

        public void AddSlave(TCPgeneral slave)
        {
            lock (slaves)
            {
                slave.OnError = OnError;
                slave.OnRawDataRecieved += RecievedData;
                slave.Start();

                SlaveObject obj = new SlaveObject();
                obj.socket = slave;
                obj.HasJob = false;
                obj.LastSeen = DateTime.Now;
                obj.JobStarted = DateTime.MinValue;
                obj.isDone = false;
                slaves.Add(obj);
            }
        }

        private void RecievedData(int id, byte[] data)
        {
            byte[] header = Headers.GetHeaderFromData(data);

            if (header.SequenceEqual(Headers.HelloUpdate))
            {
                lock (slaves)
                {
                    SlaveObject obj = slaves.Where(s => s.socket.ID == id).FirstOrDefault();
                    obj.LastSeen = DateTime.Now;
                }
            }
            else if (header.SequenceEqual(Headers.RenderCompleted))
            {
                lock (slaves)
                {
                    streamer.AddSlaveToRecieveQeue(slaves.Where(s => s.socket.ID == id).FirstOrDefault());
                    Console.WriteLine("ID: " + id + " renderCompleted");
                }

                //send to recieveclass
            }
            else if (header.SequenceEqual(Headers.RenderError))
            {
                // log job from being bad or maybe ffmpeg command being bad 
                Console.WriteLine("ID: " + id + " renderError");
            }

        }

        private void OnError(int id, ErrorTypes type, string message)
        {
            Console.WriteLine("lost connection with:" + id + "  " + type.ToString() + "       " + message);
        }
    }
}
