﻿using System;
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
        private string extenstion;
        private ManualResetEvent recieveBlock;
        private ManualResetEvent sendBlock;

        public StreamHelper(int maxConcurrentStreams, string source, string output, string extenstion)
        {
            this.source = source;
            this.output = output;
            this.extenstion = extenstion;
            SendWaiting = new ConcurrentQueue<SlaveObject>();
            RecieveWaiting = new ConcurrentQueue<SlaveObject>();
            maxStreams = maxConcurrentStreams;
            currentStreams = 0;

            recieveBlock = new ManualResetEvent(false);
            sendBlock = new ManualResetEvent(false);

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
                recieveBlock.WaitOne();

                if (currentStreams < maxStreams && RecieveWaiting.Count > 0 && RecieveWaiting.TryDequeue(out obj))
                {
                    currentStreams++;
                    Thread streamThread = new Thread(() => {
                        recieveBlock.Reset();
                        new DownStream(obj, output, extenstion).start();
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
        private void sendWorker()
        {
            SlaveObject obj;

            while (true)
            {
                sendBlock.WaitOne();

                if (currentStreams < maxStreams && SendWaiting.Count > 0 && SendWaiting.TryDequeue(out obj))
                {
                    currentStreams++;
                    Thread streamThread = new Thread(() => {
                        new UpStream(obj, source).start(); });
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
        public void AddSlaveToSendQeue(SlaveObject obj)
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
        public void AddSlaveToRecieveQeue(SlaveObject obj)
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
