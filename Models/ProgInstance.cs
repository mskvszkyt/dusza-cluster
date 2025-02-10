namespace ConsoleApp1
{
    public class ProgInstance
    {
        public string ProgramName { get; set; }
        public bool IsRunning { get; set; }
        public int ProcessorUsage { get; set; }
        public int MemoryUsage { get; set; }
        public string StartDate { get; set; }

        public ProgInstance() { }
        public ProgInstance(string programName, bool isRunning, int processorUsage, int memoryUsage, string startDate)
        {
            ProgramName = programName;
            IsRunning = isRunning;
            ProcessorUsage = processorUsage;
            MemoryUsage = memoryUsage;
            StartDate = startDate;
        }
    }
}