using AbstractTCPlib;
using SharedTypes;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Server
{
    public class DownStream
    {
        private ClientObject obj;
        private FileStream stream;
        private byte[] pieceOfVideo;
        private string output;
        private AutoResetEvent waitHandle;

        public DownStream(ClientObject obj, string output, string extenstion)
        {
            waitHandle = new AutoResetEvent(false);
            this.output = output;
            pieceOfVideo = null;
            this.obj = obj;
            stream = File.OpenWrite(Path.Combine(output, Path.GetFileNameWithoutExtension(obj.CurrentJob) + extenstion));
            obj.socket.OnRawDataRecieved += OnRecieved;
            obj.socket.OnError += OnError;
        }
        public void Start()
        {
            obj.socket.SendTCP(Headers.SendNext);

            waitHandle.WaitOne();

            stream.Close();
            
        }
        private void OnRecieved(int id, byte[] rawData)
        {
            byte[] header = Headers.GetHeaderFromData(rawData);

            if (Headers.PieceOfVideo.SequenceEqual(header))
            {
                Headers.SplitData(rawData, out header, out pieceOfVideo);

                try
                {
                    stream.Write(pieceOfVideo, 0, pieceOfVideo.Length);
                    obj.socket.SendTCP(Headers.SendNext);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Downstream write exception: " + e.Message);
                    stream.Close();
                }
            }
            else if (Headers.SendCompleted.SequenceEqual(header))
            {
                waitHandle.Set();

                lock (ClientDataBlock.Clients)
                {
                    obj.socket.OnRawDataRecieved -= OnRecieved;
                    obj.socket.OnError -= OnError;
                    obj.isDone = true;
                    obj.Finished();
                }
            }
        }
        private void OnError(int id, ErrorTypes type, string message)
        {
            waitHandle.Set();

            lock (ClientDataBlock.Clients)
            {
                obj.socket.OnRawDataRecieved -= OnRecieved;
                obj.socket.OnError -= OnError;
            }
        }
    }
}
