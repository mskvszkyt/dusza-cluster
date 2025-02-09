using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public static void Monitor(List<Instance> instances)
        {
            foreach (Instance instance in instances)
            {
                Console.WriteLine($"Gép neve: {instance.Name}");
                Console.WriteLine($"Memória használat: {instance.CalculateMemoryUsage()}MB / {instance.MemoryCapacity}MB");
                Console.WriteLine($"Processzor használat: {instance.CalculateProcessorUsage()} milimag / {instance.ProcessorCapacity} milimag");
                Console.WriteLine();
            }

            IEnumerable<string> programNames = instances.SelectMany(i => i.Programs.Select(p => p.ProgramName.Split('-')[0])).Distinct();
            Console.WriteLine("Programok:");
            foreach (string name in programNames)
            {
                Console.WriteLine($" {name}");
            }

            Console.WriteLine("\nAzonosítók és státuszok:");
            foreach (Instance instance in instances)
            {
                foreach (ProgInstance program in instance.Programs)
                {
                    Console.WriteLine($" {program.ProgramName.Split('-').Last()} - {(program.IsRunning ? "AKTÍV" : "INAKTÍV")}");
                }
            }

            Console.WriteLine($"\nÖsszesen {instances.Sum(i => i.Programs.Count)} db program fut.");
        }

        public static void MonitorSearch(List<Instance> instances, string programName)
        {
            Console.WriteLine($"{programName} futó példányai:");
            foreach (Instance instance in instances)
            {
                foreach (ProgInstance program in instance.Programs)
                {
                    if (program.ProgramName.StartsWith(programName))
                    {
                        Console.WriteLine($"Ezen fut: {instance.Name}");
                        Console.WriteLine($"Memória használata: {program.MemoryUsage}MB");
                        Console.WriteLine($"Processzor használata: {program.ProcessorUsage} milimag");
                        Console.WriteLine();
                    }
                }
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
                .OrderBy(i => (i.CalculateProcessorUsage() + i.CalculateMemoryUsage()))
                .FirstOrDefault();

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

        private static string GenerateUniqueKey(List<string> existingKeys)
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
                ShowError("HIBA: NEM FUT ILYEN PROGRAM");
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

        public static void StopProgram(Cluster cluster, string path)
        {
            Console.WriteLine("Válassza ki a törölni kívánt programot");
            var allPrograms = cluster.Instances
                .SelectMany(i => i.Programs)
                .Select(p => new {
                    Id = p.ProgramName.Split('-').Last(),
                    FullName = p.ProgramName
                })
                .ToList();

            allPrograms.ForEach(p => Console.WriteLine($" {p.Id} - {p.FullName}"));
            Console.WriteLine("Adja meg az egyedi azonosítóját a programnak!");
            string programId = Console.ReadLine()?.Trim();

            foreach (Instance instance in cluster.Instances)
            {
                ProgInstance? program = instance.Programs.FirstOrDefault(p =>
                    p.ProgramName.EndsWith($"-{programId}", StringComparison.Ordinal)
                );

                if (program != null)
                {
                    // Delete physical file
                    string filePath = Path.Combine(path, instance.Name, program.ProgramName);
                    if (File.Exists(filePath)) File.Delete(filePath);

                    // Remove from memory
                    instance.Programs.Remove(program);

                    Console.WriteLine($"Sikeresen törölve: {program.ProgramName}");
                    return;
                }
            }

            ShowError("HIBA: NEM TALÁLHATÓ PROGRAM");
        }

        public static void ModifyClusterStartupSettings(Cluster cluster, string path)
        {
            Console.WriteLine("Válassza ki a módosítani kívánt programot");
            for (int i = 0; i < cluster.ScheduledPrograms.Count; i++)
            {
                Console.WriteLine($"{i}. {cluster.ScheduledPrograms[i].ProgramName}");
            }

            if (int.TryParse(Console.ReadLine(), out int programIndex) &&
                programIndex >= 0 &&
                programIndex < cluster.ScheduledPrograms.Count)
            {
                ScheduledProgram program = cluster.ScheduledPrograms[programIndex];
                Console.WriteLine("0. Példányszám módosítása\n1. CPU igény módosítása\n2. Memóriaigény módosítása");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 0 && choice <= 2)
                {
                    Console.WriteLine($"Jelenlegi érték: {GetPropertyValue(program, choice)}");
                    Console.WriteLine("Új érték:");
                    if (int.TryParse(Console.ReadLine(), out int newValue))
                    {
                        SetPropertyValue(program, choice, newValue);
                        FileManager.WriteCluster(path, cluster);
                        Console.WriteLine("Sikeres módosítás!");
                    }
                }
            }
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

        private static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}