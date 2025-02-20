namespace ConsoleApp1
{
    /// <summary>
    /// Represents a program instance running on an <see cref="Instance"/>.
    /// </summary>
    public class ProgInstance
    {
        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the program is currently running.
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Gets or sets the processor usage of the program.
        /// </summary>
        public int ProcessorUsage { get; set; }

        /// <summary>
        /// Gets or sets the memory usage of the program.
        /// </summary>
        public int MemoryUsage { get; set; }

        /// <summary>
        /// Gets or sets the start date of the program.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgInstance"/> class.
        /// </summary>
        public ProgInstance() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgInstance"/> class with the specified parameters.
        /// </summary>
        /// <param name="programName">The name of the program.</param>
        /// <param name="isRunning">A value indicating whether the program is running.</param>
        /// <param name="processorUsage">The processor usage of the program.</param>
        /// <param name="memoryUsage">The memory usage of the program.</param>
        /// <param name="startDate">The start date of the program.</param>
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
