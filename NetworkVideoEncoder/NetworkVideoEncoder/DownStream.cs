using AbstractTCPlib;
using SharedTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkVideoEncoder
{
    public class DownStream
    {
        private SlaveObject obj;
        private FileStream stream;
        private byte[] pieceOfVideo;
        private string output;
        private AutoResetEvent waitHandle;

        public DownStream(SlaveObject obj, string output)
        {
            waitHandle = new AutoResetEvent(false);
            this.output = output;
            pieceOfVideo = null;
            this.obj = obj;
            stream = File.OpenWrite(Path.Combine(output, obj.CurrentJob));
            obj.socket.OnRawDataRecieved += onRecieved;
            obj.socket.OnError += OnError;
        }
        public void start()
        {
            obj.socket.SendTCP(Headers.SendNext);

            waitHandle.WaitOne();

            stream.Close();
            obj.socket.OnRawDataRecieved -= onRecieved;
            obj.socket.OnError -= OnError;
            obj.isDone = true;
        }
        private void onRecieved(int id, byte[] rawData)
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
                    Console.WriteLine("write exception");
                    stream.Close();
                }
            }
            else if (Headers.SendCompleted.SequenceEqual(header))
            {
                waitHandle.Set();
            }
        }
        private void OnError(int id, ErrorTypes type, string message)
        {
            waitHandle.Set();
        }
    }
}
