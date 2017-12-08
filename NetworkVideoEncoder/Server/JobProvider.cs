using AbstractTCPlib;
using System;
using System.Text;
using System.IO;
using SharedTypes;
using System.Linq;
using System.Threading;

namespace Server
{
    public class JobProvider
    {
        private string output;
        private string ffmpegCommand;
        private StreamHelper streamer;
        private ManualResetEvent mainLoopWait;
        private string extension;

        public JobProvider(string ffmpeg, string source, string output)
        {
            this.output = output;

            string[] files = Directory.GetFiles(source);

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }
            
            for (int i = 0; i < files.Length; i++)
            {
                JobDataBlock.Jobs.Add(new Job
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
            GiveJobs();

            mainLoopWait.WaitOne();
        }
        private void GiveJobs()
        {
            lock (ClientDataBlock.Clients)
            {
                lock (JobDataBlock.Jobs)
                {
                    foreach (var client in ClientDataBlock.Clients)
                    {
                        if (!client.HasJob && client.socket.IsAlive)
                        {
                            foreach (var job in JobDataBlock.Jobs)
                            {
                                if (!job.IsGivenAsJob)
                                {
                                    job.IsGivenAsJob = true;
                                    job.ClientID = client.socket.ID;
                                    client.HasJob = true;
                                    client.CurrentJob = job.JobURL;
                                    client.socket.SendTCP(Headers.AssembleHeader(Headers.Job, Encoding.ASCII.GetBytes(job.JobURL)));
                                    client.socket.SendTCP(Headers.AssembleHeader(Headers.FfmpegCommand, Encoding.ASCII.GetBytes(ffmpegCommand)));
                                    streamer.AddClientToSendQeue(client);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AddClient(TCPgeneral client)
        {
            lock (ClientDataBlock.Clients)
            {
                client.OnError = OnError;
                client.OnRawDataRecieved += RecievedData;
                client.Start();

                ClientObject obj = new ClientObject
                {
                    socket = client,
                    HasJob = false,
                    LastSeen = DateTime.Now,
                    JobStarted = DateTime.MinValue,
                    isDone = false
                };
                obj.OnFinished += ClientFinished;
                ClientDataBlock.Clients.Add(obj);

                GiveJobs();
            }
        }

        private void RecievedData(int id, byte[] data)
        {
            byte[] header = Headers.GetHeaderFromData(data);

            if (header.SequenceEqual(Headers.HelloUpdate))
            {
                lock (ClientDataBlock.Clients)
                {
                    ClientObject obj = ClientDataBlock.Clients.Where(s => s.socket.ID == id).FirstOrDefault();
                    obj.LastSeen = DateTime.Now;
                }
            }
            else if (header.SequenceEqual(Headers.RenderCompleted))
            {
                lock (ClientDataBlock.Clients)
                {
                    streamer.AddClientToRecieveQeue(ClientDataBlock.Clients.Where(s => s.socket.ID == id).FirstOrDefault());
                    Console.WriteLine("ID: " + id + " renderCompleted");
                }
            }
            else if (header.SequenceEqual(Headers.RenderError))
            {
                lock (ClientDataBlock.Clients)
                {
                    lock (JobDataBlock.Jobs)
                    {
                        // log job from being bad or maybe ffmpeg command being bad 
                        Console.WriteLine("ID: " + id + " renderError");

                        ClientObject obj = ClientDataBlock.Clients.Where(client => client.socket.ID == id).FirstOrDefault();
                        if (obj != null)
                        {
                            Job jo = JobDataBlock.Jobs.Where(j => j.ClientID == obj.socket.ID).FirstOrDefault();

                            if (jo != null)
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
            lock (ClientDataBlock.Clients)
            {
                lock (JobDataBlock.Jobs)
                {
                    Console.WriteLine("lost connection with:" + id + "  " + type.ToString() + "       " + message);

                    ClientObject obj = ClientDataBlock.Clients.Where(client => client.socket.ID == id).FirstOrDefault();
                    if (obj != null)
                    {
                        lock (obj)
                        {
                            Job jo = JobDataBlock.Jobs.Where(j => j.ClientID == obj.socket.ID).FirstOrDefault();

                            if (jo != null)
                            {
                                jo.IsGivenAsJob = false;
                                jo.IsDone = false;
                                jo.ClientID = -1;
                            }
                            obj.socket.Dispose();

                            ClientDataBlock.Clients.Remove(obj);
                        }
                    }
                }
            }
        }
        private void ClientFinished(ClientObject obj)
        {
            lock (ClientDataBlock.Clients)
            {
                lock (JobDataBlock.Jobs)
                {
                    Job jo = JobDataBlock.Jobs.Where(j => j.ClientID == obj.socket.ID).FirstOrDefault();

                    if (jo != null)
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
            }

            GiveJobs();

            IsAllDone();
        }
        private void IsAllDone()
        {
            int incompleted = JobDataBlock.Jobs.Where(job => job.IsDone == false).Count();

            if (incompleted > 0)
            {
                return;
            }

            int stillWorking = ClientDataBlock.Clients.Where(client => client.HasJob == false).Count();

            if (stillWorking > 0)
            {
                return;
            }

            Console.WriteLine("All jobs completed");

            mainLoopWait.Set();
        }
    }
}
