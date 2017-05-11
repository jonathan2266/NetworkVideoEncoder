using System;

namespace SharedTypes
{
    public static class Headers
    {
        public static byte[] ID { get { return new byte[] { 0, 0, 0, 0 }; } }
        public static byte[] Job { get { return new byte[] { 0, 0, 0, 1 }; } }
        public static byte[] PieceOfVideo { get { return new byte[] { 0, 0, 0, 2 }; } }
        public static byte[] SendNext { get { return new byte[] { 0, 0, 0, 3 }; } }
        public static byte[] HelloUpdate { get { return new byte[] { 0, 0, 0, 4 }; } }
        public static byte[] SendCompleted { get { return new byte[] { 0, 0, 0, 5 }; } }
        public static byte[] RenderCompleted { get { return new byte[] { 0, 0, 0, 6 }; } }
        public static byte[] RenderError { get { return new byte[] { 0, 0, 0, 7 }; } }
        public static byte[] ffmpegCommand { get { return new byte[] { 0, 0, 0, 8 }; } }

        public static byte[] AssembleHeader(byte[] header, byte[] data)
        {
            byte[] fin = new byte[header.Length + data.Length];
            Array.Copy(header, 0, fin, 0, header.Length);
            Array.Copy(data, 0, fin, header.Length, data.Length);
            return fin;
        }
        public static byte[] GetHeaderFromData(byte[] data)
        {
            byte[] header = new byte[4];

            if (data.Length >= 4)
            {
                Array.Copy(data, 0, header, 0, 4);
                return header;
            }
            else
            {
                return null;
            }
        }
        public static void SplitData(byte[] rawData, out byte[] header, out byte[] data)
        {
            header = new byte[4];
            data = new byte[rawData.Length - 4];

            if (rawData.Length >= 4)
            {
                Array.Copy(rawData, 0, header, 0, 4);
                Array.Copy(rawData, 4, data, 0, rawData.Length - 4);
            }
            else
            {
                header = null;
                data = null;
            }
        }
    }
}
