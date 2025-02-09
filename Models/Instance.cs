namespace ConsoleApp1
{
    public class Instance
    {
        public string Name { get; set; }
        public int MemoryCapacity { get; set; }
        public int ProcessorCapacity { get; set; }
        public List<ProgInstance> Programs { get; set; } = new List<ProgInstance>();

        public int CalculateMemoryUsage() => Programs.Sum(prog => prog.MemoryUsage);
        public int CalculateProcessorUsage() => Programs.Sum(prog => prog.ProcessorUsage);
    }
}