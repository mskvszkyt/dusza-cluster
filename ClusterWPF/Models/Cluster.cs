namespace ConsoleApp1
{
    public class Cluster
    {
        public string Path { get; set; }
        public List<ScheduledProgram> ScheduledPrograms { get; set; } = new List<ScheduledProgram>();
        public List<Instance> Instances { get; set; } = new List<Instance>();
    }
}