namespace ConsoleApp1
{
    public class Instance
    {
        public string Name { get; set; }
        public int MemoryCapacity { get; set; }
        public int ProcessorCapacity { get; set; }
        public List<ProgInstance> Programs { get; set; } = new List<ProgInstance>();

        public int CalculateMemoryUsage() => Programs.Sum(prog => prog.MemoryUsage); 
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int CalculateProcessorUsage() => Programs.Sum(prog => prog.ProcessorUsage);

        public double MemoryUsagePercentage => MemoryCapacity > 0 ? (double)CalculateMemoryUsage() / MemoryCapacity : 0;
        public double ProcessorUsagePercentage => ProcessorCapacity > 0 ? (double)CalculateProcessorUsage() / ProcessorCapacity : 0;
    }
}