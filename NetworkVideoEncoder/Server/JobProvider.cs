using AbstractTCPlib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharedTypes;
using System.Linq;
using System.Threading;

namespace Server
{
    public class JobProvider
    {
        private List<ClientObject> clients;
        private List<Job> jobs;
        private string output;
        private string ffmpegCommand;
        private StreamHelper streamer;
        private ManualResetEvent mainLoopWait;
        private string extension;

        public JobProvider(string ffmpeg, string source, string output)
        {
            this.output = output;

            clients = new List<ClientObject>();
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
                    IsGivenAsJob = false,
                    JobURL = files[i],
                    ClientID = -1,
                    IsDone = false
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
            foreach (var client in clients)
            {
                if (!client.HasJob && client.socket.IsAlive)
                {
                    foreach (var job in jobs)
                    {
                        lock (job)
                        {
                            if (!job.IsGivenAsJob)
                            {
                                job.IsGivenAsJob = true;
                                job.ClientID = client.socket.ID;
                                client.HasJob = true;
                                client.CurrentJob = job.JobURL;
                                client.socket.SendTCP(Headers.AssembleHeader(Headers.Job, Encoding.ASCII.GetBytes(job.JobURL)));
                                client.socket.SendTCP(Headers.AssembleHeader(Headers.ffmpegCommand, Encoding.ASCII.GetBytes(ffmpegCommand)));
                                streamer.AddClientToSendQeue(client);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void AddClient(TCPgeneral client)
        {
            lock (clients)
            {
                client.OnError = OnError;
                client.OnRawDataRecieved += RecievedData;
                client.Start();

                ClientObject obj = new ClientObject();
                obj.socket = client;
                obj.HasJob = false;
                obj.LastSeen = DateTime.Now;
                obj.JobStarted = DateTime.MinValue;
                obj.isDone = false;
                obj.OnFinished += clientFinished;
                clients.Add(obj);

                giveJobs();
            }
        }

        private void RecievedData(int id, byte[] data)
        {
            byte[] header = Headers.GetHeaderFromData(data);

            if (header.SequenceEqual(Headers.HelloUpdate))
            {
                lock (clients)
                {
                    ClientObject obj = clients.Where(s => s.socket.ID == id).FirstOrDefault();
                    obj.LastSeen = DateTime.Now;
                }
            }
            else if (header.SequenceEqual(Headers.RenderCompleted))
            {
                lock (clients)
                {
                    streamer.AddClientToRecieveQeue(clients.Where(s => s.socket.ID == id).FirstOrDefault());
                    Console.WriteLine("ID: " + id + " renderCompleted");
                }
            }
            else if (header.SequenceEqual(Headers.RenderError))
            {
                // log job from being bad or maybe ffmpeg command being bad 
                Console.WriteLine("ID: " + id + " renderError");

                ClientObject obj = clients.Where(client => client.socket.ID == id).FirstOrDefault();
                if (obj != null)
                {
                    lock (obj)
                    {
                        Job jo = jobs.Where(j => j.ClientID == obj.socket.ID).FirstOrDefault();

                        if (jo != null)
                        {
                            lock (jo)
                            {
                                jo.IsGivenAsJob = false;
                                jo.IsDone = false;
                                jo.ClientID = -1;
                            }
                        }
                    }
                }
            }
        }

        private void OnError(int id, ErrorTypes type, string message)
        {
            Console.WriteLine("lost connection with:" + id + "  " + type.ToString() + "       " + message);

            ClientObject obj = clients.Where(client => client.socket.ID == id).FirstOrDefault();
            if (obj != null)
            {
                lock (obj)
                {
                    Job jo = jobs.Where(j => j.ClientID == obj.socket.ID).FirstOrDefault();

                    if (jo != null)
                    {
                        lock (jo)
                        {
                            jo.IsGivenAsJob = false;
                            jo.IsDone = false;
                            jo.ClientID = -1;
                        }
                    }
                    obj.socket.Dispose();

                    clients.Remove(obj);
                }
            }
        }
        private void clientFinished(ClientObject obj)
        {
            Job jo = jobs.Where(j => j.ClientID == obj.socket.ID).FirstOrDefault();

            if (jo != null)
            {
                lock (jo)
                {
                    Console.WriteLine("job: " + jo.JobURL + " is done");
                    jo.IsDone = true;
                    jo.ClientID = -1;
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
            int incompleted = jobs.Where(job => job.IsDone == false).Count();

            if (incompleted > 0)
            {
                return;
            }

            int stillWorking = clients.Where(client => client.HasJob == false).Count();

            if (stillWorking > 0)
            {
                return;
            }

            Console.WriteLine("All jobs completed");

            mainLoopWait.Set();
        }
    }
}
