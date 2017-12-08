using System.Collections.Generic;

namespace Server
{
    public static class JobDataBlock
    {
        private static object locker = new object();

        private static List<Job> _jobs = new List<Job>();
        public static List<Job> Jobs
        {
            get
            {
                lock (locker)
                {
                    return _jobs;
                }
            }
            set
            {
                lock (locker)
                {
                    _jobs = value;
                }
            }
        }
    }
}
