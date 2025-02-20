using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp1
{
    public static class FileManager
    {
        public static List<string> GetExistingKeys(string path)
        {
            return Directory.GetDirectories(path)
                .SelectMany(dir => Directory.GetFiles(dir)
                    .Select(file => Path.GetFileName(file).Split('-').Last()))
                .ToList();
        }

        /// <summary>
        /// Reads instance directories from the specified path and creates a list of <see cref="Instance"/> objects, 
        /// each containing information about running programs and system resources.
        /// </summary>
        /// <param name="path">The root directory containing instance subdirectories.</param>
        /// <returns>A list of <see cref="Instance"/> objects representing the instances found in the specified path.</returns>

        public static List<Instance> ReadInstances(string path)
        {
            return Directory.GetDirectories(path).Select(dir =>
            {
                Instance instance = new Instance
                {
                    Name = Path.GetFileName(dir),
                    Programs = Directory.GetFiles(dir)
                        .Where(file => !file.EndsWith(".szamitogep_config"))
                        .Select(file => new ProgInstance
                        {
                            ProgramName = Path.GetFileName(file),
                            StartDate = File.ReadAllLines(file).First(),
                            IsRunning = File.ReadAllLines(file).ElementAt(1) == "AKTÍV",
                            ProcessorUsage = int.Parse(File.ReadAllLines(file).ElementAt(2)),
                            MemoryUsage = int.Parse(File.ReadAllLines(file).ElementAt(3))
                        }).ToList()
                };

                string configFile = Path.Combine(dir, ".szamitogep_config");
                if (File.Exists(configFile))
                {
                    string[] config = File.ReadAllLines(configFile);
                    instance.ProcessorCapacity = int.Parse(config[0]);
                    instance.MemoryCapacity = int.Parse(config[1]);
                }

                return instance;
            }).ToList();
        }

        /// <summary>
        /// Reads the cluster requirements from a specified path and constructs a <see cref="Cluster"/> object.
        /// </summary>
        /// <param name="path">The directory path containing the cluster configuration file.</param>
        /// <returns>A <see cref="Cluster"/> object populated with scheduled programs from the cluster file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the cluster file is not found.</exception>

        public static Cluster GetClusterRequirements(string path)
        {
            Cluster cluster = new Cluster();
            cluster.Path = path;
            string clusterFile = Path.Combine(path, ".klaszter");

            if (File.Exists(clusterFile))
            {
                string[] lines = File.ReadAllLines(clusterFile);
                for (int i = 0; i < lines.Length; i += 4)
                {
                    cluster.ScheduledPrograms.Add(new ScheduledProgram
                    {
                        ProgramName = lines[i],
                        InstanceCount = int.Parse(lines[i + 1]),
                        ProcessorRequirement = int.Parse(lines[i + 2]),
                        MemoryRequirement = int.Parse(lines[i + 3])
                    });
                }
            }
            else
            {
                throw   new FileNotFoundException("The cluster file could not be found.");
            }

            return cluster;
        }

        /// <summary>
        /// Writes the cluster requirements to a file in the specified directory.
        /// </summary>
        /// <param name="path">The directory path where the cluster file will be created or overwritten.</param>
        /// <param name="cluster">The <see cref="Cluster"/> object containing scheduled programs to be written.</param>
        public static void WriteCluster(string path, Cluster cluster)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(path, ".klaszter"), false))
            {
                foreach (ScheduledProgram program in cluster.ScheduledPrograms)
                {
                    sw.WriteLine(program.ProgramName);
                    sw.WriteLine(program.InstanceCount);
                    sw.WriteLine(program.ProcessorRequirement);
                    sw.WriteLine(program.MemoryRequirement);
                }
            }
        }
    }
}