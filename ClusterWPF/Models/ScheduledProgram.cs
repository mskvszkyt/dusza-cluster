namespace ConsoleApp1
{
    /// <summary>
    /// Represents a scheduled program that requires allocation across multiple instances.
    /// </summary>
    public class ScheduledProgram
    {
        /// <summary>
        /// Gets or sets the name of the scheduled program.
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the number of instances that this program is set to run on
        /// </summary>
        public int InstanceCount { get; set; }

        /// <summary>
        /// Gets or sets the processor capacity required to run this program on an instance.
        /// </summary>
        public int ProcessorRequirement { get; set; }

        /// <summary>
        /// Gets or sets the memory capacity required to run this program on an instance.
        /// </summary>
        public int MemoryRequirement { get; set; }
    }
}
