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
        private ManualResetEvent mainLoopWait;
        private string extension;

        public JobProvider(string ffmpeg, string source, string output)
        {
            this.output = output;

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

            int ind = ffmpegCommand.IndexOf("OUT");
            ind += 3;

            for (int i = ind; i < ffmpegCommand.Length; i++)
            {
                if (ffmpegCommand[i] != '"')
                {
                    extension += ffmpegCommand[i];
                }
                else
                {
                    break;
                }
            }

            streamer = new StreamHelper(4, source, output, extension);

            mainLoopWait = new ManualResetEvent(false);

        }
        public void RunJobs()
        {
            giveJobs();

            mainLoopWait.WaitOne();
        }
        private void giveJobs()
        {
            foreach (var slave in slaves)
            {
                if (!slave.HasJob && slave.socket.IsAlive)
                {
                    foreach (var job in jobs)
                    {
                        lock (job)
                        {
                            if (!job.isGivenAsJob)
                            {
                                job.isGivenAsJob = true;
                                job.slaveID = slave.socket.ID;
                                slave.HasJob = true;
                                slave.CurrentJob = job.jobURL;
                                slave.socket.SendTCP(Headers.AssembleHeader(Headers.Job, Encoding.ASCII.GetBytes(job.jobURL)));
                                slave.socket.SendTCP(Headers.AssembleHeader(Headers.ffmpegCommand, Encoding.ASCII.GetBytes(ffmpegCommand)));
                                streamer.AddSlaveToSendQeue(slave);
                                break;
                            }
                        }
                    }
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
                obj.OnFinished += slaveFinished;
                slaves.Add(obj);

                giveJobs();
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
            }
            else if (header.SequenceEqual(Headers.RenderError))
            {
                // log job from being bad or maybe ffmpeg command being bad 
                Console.WriteLine("ID: " + id + " renderError");

                SlaveObject obj = slaves.Where(slave => slave.socket.ID == id).FirstOrDefault();
                if (obj != null)
                {
                    lock (obj)
                    {
                        Job jo = jobs.Where(j => j.slaveID == obj.socket.ID).FirstOrDefault();

                        if (jo != null)
                        {
                            lock (jo)
                            {
                                jo.isGivenAsJob = false;
                                jo.isDone = false;
                                jo.slaveID = -1;
                            }
                        }
                    }
                }
            }
        }

        private void OnError(int id, ErrorTypes type, string message)
        {
            Console.WriteLine("lost connection with:" + id + "  " + type.ToString() + "       " + message);

            SlaveObject obj = slaves.Where(slave => slave.socket.ID == id).FirstOrDefault();
            if (obj != null)
            {
                lock (obj)
                {
                    Job jo = jobs.Where(j => j.slaveID == obj.socket.ID).FirstOrDefault();

                    if (jo != null)
                    {
                        lock (jo)
                        {
                            jo.isGivenAsJob = false;
                            jo.isDone = false;
                            jo.slaveID = -1;
                        }
                    }
                    obj.socket.Dispose();

                    slaves.Remove(obj);
                }
            }
        }
        private void slaveFinished(SlaveObject obj)
        {
            Job jo = jobs.Where(j => j.slaveID == obj.socket.ID).FirstOrDefault();

            if (jo != null)
            {
                lock (jo)
                {
                    Console.WriteLine("job: " + jo.jobURL + " is done");
                    jo.isDone = true;
                    jo.slaveID = -1;
                    obj.HasJob = false;
                    obj.isDone = false;
                    obj.JobStarted = DateTime.MinValue;
                    obj.LastSeen = DateTime.Now;
                }
            }

            giveJobs();

            isAllDone();
        }
        private void isAllDone()
        {
            int incompleted = jobs.Where(job => job.isDone == false).Count();

            if (incompleted > 0)
            {
                return;
            }

            int stillWorking = slaves.Where(slave => slave.HasJob == false).Count();

            if (stillWorking > 0)
            {
                return;
            }

            Console.WriteLine("All jobs completed");

            mainLoopWait.Set();
        }
    }
}
