using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NetworkVideoEncoder
{
    public class StreamHelper: IDisposable
    {
        private ConcurrentQueue<SlaveObject> SendWaiting;
        private ConcurrentQueue<SlaveObject> RecieveWaiting;
        private int maxStreams;
        private Thread streamSendWorker;
        private Thread streamRecieveWorker;
        private volatile int currentStreams;
        private string source;
        private string output;

        public StreamHelper(int maxConcurrentStreams, string source, string output)
        {
            this.source = source;
            this.output = output;
            SendWaiting = new ConcurrentQueue<SlaveObject>();
            RecieveWaiting = new ConcurrentQueue<SlaveObject>();
            maxStreams = maxConcurrentStreams;
            currentStreams = 0;

            streamSendWorker = new Thread(new ThreadStart(sendWorker));
            streamSendWorker.IsBackground = true;
            streamSendWorker.Start();

            streamRecieveWorker = new Thread(new ThreadStart(recieveWorker));
            streamRecieveWorker.IsBackground = true;
            streamRecieveWorker.Start();
        }
        private void recieveWorker()
        {
            SlaveObject obj;

            while (true)
            {
                if (currentStreams < maxStreams && RecieveWaiting.Count > 0 && RecieveWaiting.TryDequeue(out obj))
                {
                    currentStreams++;
                    Thread streamThread = new Thread(() => { new DownStream(obj, output).start(); });
                    streamThread.IsBackground = true;
                    streamThread.Start();
                }
                Thread.Sleep(1);
            }
        }
        private void sendWorker()
        {
            SlaveObject obj;

            while (true)
            {
                if (currentStreams < maxStreams && SendWaiting.Count > 0 && SendWaiting.TryDequeue(out obj))
                {
                    currentStreams++;
                    Thread streamThread = new Thread(() => { new UpStream(obj, source).start(); });
                    streamThread.IsBackground = true;
                    streamThread.Start();
                }
                Thread.Sleep(1);
            }
        }
        public void AddSlaveToSendQeue(SlaveObject obj)
        {
            if (obj != null)
            {
                SendWaiting.Enqueue(obj);
            }
        }
        public void AddSlaveToRecieveQeue(SlaveObject obj)
        {
            if (obj != null)
            {
                RecieveWaiting.Enqueue(obj);
            }
        }
        public void Dispose()
        {
            streamSendWorker.Abort();
            streamRecieveWorker.Abort();
        }
    }
}
