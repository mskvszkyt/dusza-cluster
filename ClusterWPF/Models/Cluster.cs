namespace ConsoleApp1
{
    /// <summary>
    /// Represents a cluster containing multiple instances and scheduled programs.
    /// </summary>
    public class Cluster
    {
        /// <summary>
        /// Gets or sets the path associated with the cluster.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets the list of scheduled programs within the cluster.
        /// </summary>
        public List<ScheduledProgram> ScheduledPrograms { get; set; } = new List<ScheduledProgram>();

        /// <summary>
        /// Gets the list of instances within the cluster.
        /// </summary>
        public List<Instance> Instances { get; set; } = new List<Instance>();
    }
}
