namespace NetworkVideoEncoder
{
    public class Job
    {
        public string jobURL { get; set; }
        public int slaveID { get; set; }
        public bool isGivenAsJob { get; set; }
        public bool isDone { get; set; }
    }
}
