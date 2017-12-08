using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Server
{
    public class StreamHelper: IDisposable
    {
        private ConcurrentQueue<ClientObject> SendWaiting;
        private ConcurrentQueue<ClientObject> RecieveWaiting;
        private int maxStreams;
        private Thread streamSendWorker;
        private Thread streamRecieveWorker;
        private volatile int currentStreams;
        private string source;
        private string output;
        private string extenstion;
        private ManualResetEvent recieveBlock;
        private ManualResetEvent sendBlock;

        public StreamHelper(int maxConcurrentStreams, string source, string output, string extenstion)
        {
            this.source = source;
            this.output = output;
            this.extenstion = extenstion;
            SendWaiting = new ConcurrentQueue<ClientObject>();
            RecieveWaiting = new ConcurrentQueue<ClientObject>();
            maxStreams = maxConcurrentStreams;
            currentStreams = 0;

            recieveBlock = new ManualResetEvent(false);
            sendBlock = new ManualResetEvent(false);

            streamSendWorker = new Thread(new ThreadStart(SendWorker))
            {
                IsBackground = true
            };
            streamSendWorker.Start();

            streamRecieveWorker = new Thread(new ThreadStart(RecieveWorker))
            {
                IsBackground = true
            };
            streamRecieveWorker.Start();
        }
        private void RecieveWorker()
        {
            ClientObject obj;

            while (true)
            {
                recieveBlock.WaitOne();

                if (currentStreams < maxStreams && RecieveWaiting.Count > 0 && RecieveWaiting.TryDequeue(out obj))
                {
                    currentStreams++;
                    Thread streamThread = new Thread(() => {
                        recieveBlock.Reset();
                        new DownStream(obj, output, extenstion).Start();
                        currentStreams--;
                    });
                    streamThread.IsBackground = true;
                    streamThread.Start();

                    lock (RecieveWaiting)
                    {
                        if (RecieveWaiting.IsEmpty)
                        {
                            recieveBlock.Reset();
                        }
                    }
                }
            }
        }
        private void SendWorker()
        {
            ClientObject obj;

            while (true)
            {
                sendBlock.WaitOne();

                if (currentStreams < maxStreams && SendWaiting.Count > 0 && SendWaiting.TryDequeue(out obj))
                {
                    currentStreams++;
                    Thread streamThread = new Thread(() => {
                        new UpStream(obj, source).Start(); });
                    currentStreams--;
                    streamThread.IsBackground = true;
                    streamThread.Start();

                    lock (SendWaiting)
                    {
                        if (SendWaiting.IsEmpty)
                        {
                            sendBlock.Reset();
                        }
                    }
                }
            }
        }
        public void AddClientToSendQeue(ClientObject obj)
        {
            if (obj != null)
            {
                lock (SendWaiting)
                {
                    SendWaiting.Enqueue(obj);
                    sendBlock.Set();
                }
            }
        }
        public void AddClientToRecieveQeue(ClientObject obj)
        {
            if (obj != null)
            {
                lock (RecieveWaiting)
                {
                    RecieveWaiting.Enqueue(obj);
                    recieveBlock.Set();
                }
            }
        }
        public void Dispose()
        {
            streamSendWorker.Abort();
            streamRecieveWorker.Abort();
        }
    }
}
