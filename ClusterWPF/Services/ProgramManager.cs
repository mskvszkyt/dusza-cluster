using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace ConsoleApp1
{
    public static class ProgramManager
    {
        public static void ValidateClusterState(Cluster cluster)
        {
            bool clusterValid = true;

            // 1. Verify program instance counts
            foreach (ScheduledProgram scheduled in cluster.ScheduledPrograms)
            {
                int activeCount = 0;
                int totalCount = 0;

                foreach (Instance instance in cluster.Instances)
                {
                    foreach (ProgInstance prog in instance.Programs)
                    {
                        if (prog.ProgramName.StartsWith(scheduled.ProgramName))
                        {
                            totalCount++;
                            if (prog.IsRunning) activeCount++;
                        }
                    }
                }

                // Check minimum active instances
                if (activeCount < scheduled.InstanceCount)
                {
                    clusterValid = false;
                    ShowError($"HIBA: {scheduled.ProgramName}\n" +
                              $"  Kívánt AKTÍV példányszám: {scheduled.InstanceCount}\n" +
                              $"  Aktuális AKTÍV: {activeCount}");
                }

                // Check maximum total instances
                if (totalCount > scheduled.InstanceCount)
                {
                    clusterValid = false;
                    ShowError($"HIBA: {scheduled.ProgramName}\n" +
                              $"  Megengedett maximális példányszám: {scheduled.InstanceCount}\n" +
                              $"  Aktuális összes példány: {totalCount}");
                }
            }

            // 2. Verify resource capacities
            foreach (Instance instance in cluster.Instances)
            {
                int totalProcessor = instance.Programs.Sum(p => p.ProcessorUsage);
                int totalMemory = instance.Programs.Sum(p => p.MemoryUsage);

                if (totalProcessor > instance.ProcessorCapacity ||
                    totalMemory > instance.MemoryCapacity)
                {
                    clusterValid = false;
                    ShowError($"HIBA: {instance.Name} erőforrás túlcsordulás\n" +
                              $"  Processzor: {totalProcessor}/{instance.ProcessorCapacity} milimag\n" +
                              $"  Memória: {totalMemory}/{instance.MemoryCapacity} MB");
                }
            }

            if (clusterValid)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("A klaszter állapota megfelelő");
                Console.ResetColor();
            }
        }


        public static void RunProgram(Cluster cluster, string path)
        {
            // Get available programs and validate selection
            List<string> availablePrograms = cluster.ScheduledPrograms.Select(sp => sp.ProgramName).ToList();
            if (!availablePrograms.Any())
            {
                ShowError("Nincsenek ütemezett programok!");
                return;
            }

            Console.WriteLine("Válassza ki a futtatni kívánt programpéldányt:");
            for (int i = 0; i < availablePrograms.Count; i++)
            {
                Console.WriteLine($"{i}. {cluster.ScheduledPrograms[i].ProgramName}");
            }

            if (!int.TryParse(Console.ReadLine(), out int input) || input < 0 || input >= availablePrograms.Count)
            {
                ShowError("Érvénytelen választás.");
                return;
            }

            ScheduledProgram selectedProgram = cluster.ScheduledPrograms[input];

            // 1. Check instance count limits
            int existingInstances = cluster.Instances
                .SelectMany(i => i.Programs)
                .Count(p => p.ProgramName.StartsWith(selectedProgram.ProgramName));

            if (existingInstances >= selectedProgram.InstanceCount)
            {
                ShowError($"HIBA: Elérted a maximális példányszámot ({selectedProgram.InstanceCount})");
                return;
            }

            // 2. Create new program instance
            ProgInstance progInstance = new ProgInstance(
                selectedProgram.ProgramName,
                true,
                selectedProgram.ProcessorRequirement,
                selectedProgram.MemoryRequirement,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            );

            // 3. Find optimal instance
            Instance? optimalInstance = cluster.Instances
            .Where(i => i.CalculateProcessorUsage() + progInstance.ProcessorUsage <= i.ProcessorCapacity &&
                        i.CalculateMemoryUsage() + progInstance.MemoryUsage <= i.MemoryCapacity)
            .OrderBy(i => i.CalculateProcessorUsage() + i.CalculateMemoryUsage())
            .FirstOrDefault(i =>
                i.CalculateProcessorUsage() + progInstance.ProcessorUsage <= i.ProcessorCapacity &&
                i.CalculateMemoryUsage() + progInstance.MemoryUsage <= i.MemoryCapacity);

            if (optimalInstance == null)
            {
                ShowError("HIBA: Nincs elegendő szabad erőforrás a klaszteren!");
                return;
            }

            string key = GenerateUniqueKey(FileManager.GetExistingKeys(path));
            progInstance.ProgramName = $"{progInstance.ProgramName}-{key}";
            optimalInstance.Programs.Add(progInstance);

            // 5. Update scheduled program configuration
            FileManager.WriteCluster(path, cluster);

            // 6. Create physical file
            File.WriteAllText(
                Path.Combine(path, optimalInstance.Name, progInstance.ProgramName),
                $"{progInstance.StartDate}\nAKTÍV\n{progInstance.ProcessorUsage}\n{progInstance.MemoryUsage}"
            );

            Console.WriteLine($"Sikeresen indítva: {progInstance.ProgramName} ({optimalInstance.Name})");
        }

        /// <summary>
        /// Generates a unique 6-character lowercase string key that does not exist in the provided list of existing keys.
        /// </summary>
        /// <param name="existingKeys">A list of keys to ensure uniqueness against.</param>
        /// <returns>A unique 6-character lowercase string key.</returns>
        public static string GenerateUniqueKey(List<string> existingKeys)
        {
            Random rnd = new Random();
            string key;
            do
            {
                key = new string(Enumerable.Range(0, 6).Select(_ => (char)rnd.Next(97, 123)).ToArray());
            } while (existingKeys.Contains(key));

            return key;
        }


        public static void ShutDownProgram(string programName, string path, Cluster cluster)
        {
            // 1. Remove from ScheduledPrograms
            ScheduledProgram? scheduledProgram = cluster.ScheduledPrograms.FirstOrDefault(sp => sp.ProgramName == programName);
            if (scheduledProgram == null)
            {
                ShowError("HIBA: Nem fut ilyen program");
                return;
            }
            cluster.ScheduledPrograms.Remove(scheduledProgram);

            // 2. Delete all related program instances and files
            bool anyDeleted = false;
            foreach (Instance instance in cluster.Instances)
            {
                List<ProgInstance> programsToRemove = instance.Programs
                    .Where(p => p.ProgramName.StartsWith($"{programName}-", StringComparison.Ordinal))
                    .ToList();

                foreach (ProgInstance program in programsToRemove)
                {
                    string filePath = Path.Combine(path, instance.Name, program.ProgramName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        anyDeleted = true;
                    }

                    instance.Programs.Remove(program);
                }
            }

            // 3. Update cluster configuration file
            FileManager.WriteCluster(path, cluster);

            Console.WriteLine(anyDeleted
                ? "Sikeresen törölve a program és minden példánya!"
                : "Figyelem: A program nem futott egyetlen gépen sem.");
        }

     

        private static int GetPropertyValue(ScheduledProgram program, int choice) => choice switch
        {
            0 => program.InstanceCount,
            1 => program.ProcessorRequirement,
            2 => program.MemoryRequirement,
            _ => throw new ArgumentOutOfRangeException()
        };

        private static void SetPropertyValue(ScheduledProgram program, int choice, int value)
        {
            switch (choice)
            {
                case 0: program.InstanceCount = value; break;
                case 1: program.ProcessorRequirement = value; break;
                case 2: program.MemoryRequirement = value; break;
            }
        }

        /// <summary>
        /// Displays an error message in a message box.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private static void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}