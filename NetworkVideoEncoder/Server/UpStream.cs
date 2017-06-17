using AbstractTCPlib;
using SharedTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class UpStream
    {
        private ClientObject obj;
        private FileStream stream;
        private volatile bool isDone;
        private int l = 10000000; // = 10MB
        private byte[] data;
        private AutoResetEvent reset;

        public UpStream(ClientObject obj, string source)
        {
            reset = new AutoResetEvent(false);
            data = new byte[l];
            this.obj = obj;
            obj.socket.OnRawDataRecieved += OnRecieve;
            obj.socket.OnError += OnError;
            try
            {
                stream = File.OpenRead(Path.Combine(source, obj.CurrentJob));
            }
            catch (Exception)
            {
                //stop 
            }

            isDone = false;
        }
        public void start()
        {
            sendNextPiece();

            reset.WaitOne();

            stream.Close();

            lock (obj)
            {
                obj.socket.OnRawDataRecieved -= OnRecieve;
                obj.socket.OnError -= OnError;
            }
        }
        private void sendNextPiece()
        {
            if (isDone)
            {
                obj.socket.SendTCP(Headers.SendCompleted);
                obj.JobStarted = DateTime.Now;
                reset.Set();
            }
            else
            {
                try
                {
                    int read = stream.Read(data, 0, l);
                    if (read != l)
                    {
                        byte[] cropped = new byte[read];
                        Array.Copy(data, 0, cropped, 0, read);
                        obj.socket.SendTCP(Headers.AssembleHeader(Headers.PieceOfVideo, cropped));
                        isDone = true;
                    }
                    else
                    {
                        obj.socket.SendTCP(Headers.AssembleHeader(Headers.PieceOfVideo, data));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in upstream: " + e.Message);
                    stream.Close();
                }
            }


        }
        private void OnRecieve(int id, byte[] rawData)
        {
            byte[] header = Headers.GetHeaderFromData(rawData);

            if (Headers.SendNext.SequenceEqual(header))
            {
                sendNextPiece();
            }
        }
        private void OnError(int id, ErrorTypes type, string message)
        {
            reset.Set();
        }
    }
}
