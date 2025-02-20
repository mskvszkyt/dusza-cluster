using ConsoleApp1;

/// <summary>
/// Represents a computer instance with allocated memory, processor capacity, and running programs.
/// </summary>
public class Instance
{
    /// <summary>
    /// Gets or sets the name of the instance.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the total memory capacity of the instance.
    /// </summary>
    public int MemoryCapacity { get; set; }

    /// <summary>
    /// Gets or sets the total processor capacity of the instance.
    /// </summary>
    public int ProcessorCapacity { get; set; }

    /// <summary>
    /// Gets the list of running programs within the instance.
    /// </summary>
    public List<ProgInstance> Programs { get; set; } = new List<ProgInstance>();

    /// <summary>
    /// Calculates the total memory usage of all running programs.
    /// </summary>
    /// <returns>The total memory usage in MB.</returns>
    public int CalculateMemoryUsage() => Programs.Sum(prog => prog.MemoryUsage);

    /// <summary>
    /// Calculates the total processor usage of all running programs.
    /// </summary>
    /// <returns>The total processor usage in percentage or units.</returns>
    public int CalculateProcessorUsage() => Programs.Sum(prog => prog.ProcessorUsage);

    /// <summary>
    /// Gets the percentage of memory usage relative to the total memory capacity.
    /// </summary>
    public double MemoryUsagePercentage => MemoryCapacity > 0 ? (double)CalculateMemoryUsage() / MemoryCapacity : 0;

    /// <summary>
    /// Gets the percentage of processor usage relative to the total processor capacity.
    /// </summary>
    public double ProcessorUsagePercentage => ProcessorCapacity > 0 ? (double)CalculateProcessorUsage() / ProcessorCapacity : 0;

    /// <summary>
    /// Gets the available processor capacity after considering the usage of running programs.
    /// </summary>
    public double AvailableProcessorCapacity => ProcessorCapacity - CalculateProcessorUsage();

    /// <summary>
    /// Gets the available memory capacity after considering the usage of running programs.
    /// </summary>
    public double AvailableMemoryCapacity => MemoryCapacity - CalculateMemoryUsage();

    /// <summary>
    /// Determines whether the instance can accommodate a given program based on available resources.
    /// </summary>
    /// <param name="program">The program to check.</param>
    /// <returns>True if the program can be accommodated; otherwise, false.</returns>
    public bool CanAccommodateProgram(ProgInstance program) =>
        AvailableProcessorCapacity >= program.ProcessorUsage &&
        AvailableMemoryCapacity >= program.MemoryUsage;
}
