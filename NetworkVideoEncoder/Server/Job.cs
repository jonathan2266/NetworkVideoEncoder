namespace Server
{
    public class Job
    {
        public string JobURL { get; set; }
        public int ClientID { get; set; }
        public bool IsGivenAsJob { get; set; }
        public bool IsDone { get; set; }
    }
}
