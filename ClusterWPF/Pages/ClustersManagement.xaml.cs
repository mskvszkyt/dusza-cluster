using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace ClusterWPF.Pages
{
    /// <summary>
    /// Interaction logic for ClustersManagement.xaml
    /// </summary>
    public partial class ClustersManagement : Page
    {
        public ClustersManagement()
        {
            InitializeComponent();
        }

        public void TransferProgramToCluster(ProgInstance program, Cluster sourceCluster, Cluster targetCluster)
        {
            Instance? sourceComputer = sourceCluster.Instances.FirstOrDefault(c => c.Programs.Any(p => p.ProgramName == program.ProgramName));
            if (sourceComputer == null)
            {
                MessageBox.Show("A program nem található a forrás klaszterben!");
                return;
            }

            Instance? targetComputer = FindSuitableTargetComputer(program, targetCluster);
            if (targetComputer == null)
            {
                MessageBox.Show("Nincs megfelelő gép a célklaszterben!");
                return;
            }

            if (targetCluster.Instances.Any(c => c.Programs.Any(p => p.ProgramName == program.ProgramName)))
            {
                string newKey = ProgramManager.GenerateUniqueKey(FileManager.GetExistingKeys(targetCluster.Path));
                program.ProgramName = $"{program.ProgramName}-{newKey}";
                MessageBox.Show($"A program már létezik a célklaszterben, új kulcs hozzáadva! {newKey}");
            }

            ScheduledProgram newScheduledProgram = new ScheduledProgram
            {
                ProgramName = program.ProgramName.Split('-').Last(),
                InstanceCount = 1,
                ProcessorRequirement = program.ProcessorUsage,
                MemoryRequirement = program.MemoryUsage
            };


            ScheduledProgram? scheduledProgram = targetCluster.ScheduledPrograms
                .FirstOrDefault(s => s.ProgramName == newScheduledProgram.ProgramName
                && s.MemoryRequirement == newScheduledProgram.MemoryRequirement
                && s.ProcessorRequirement == newScheduledProgram.ProcessorRequirement);

            if (scheduledProgram != null)
            {
                scheduledProgram.InstanceCount++;
            }
            else
            {
                targetCluster.ScheduledPrograms.Add(newScheduledProgram);
            }

            sourceComputer.Programs.Remove(program);
            targetComputer.Programs.Add(program);
            sourceCluster.ScheduledPrograms.First(s => s.ProgramName == program.ProgramName).InstanceCount--;
            string sourcePath = Path.Combine(sourceCluster.Path, sourceComputer.Name, program.ProgramName);
            string targetPath = Path.Combine(targetCluster.Path, targetComputer.Name, program.ProgramName);
            File.Move(sourcePath, targetPath);
            FileManager.WriteCluster(targetCluster.Path, targetCluster);
            FileManager.WriteCluster(sourceCluster.Path, sourceCluster);
            MessageBox.Show("Program sikeresen áthelyezve!");
        }

        public void TransferComputerToCluster(string computerName, Cluster sourceCluster, Cluster targetCluster)
        {
            string sourceComputerPath = Path.Combine(sourceCluster.Path, computerName);
            string targetComputerPath = Path.Combine(targetCluster.Path, computerName);

            if (!Directory.Exists(sourceComputerPath))
            {
                MessageBox.Show($"Forrás számítógép nem található: {computerName}");
                return;
            }

            if (Directory.Exists(targetComputerPath))
            {
                MessageBox.Show($"Cél számítógép már létezik: {computerName}");
                return;
            }

            Instance? computer = sourceCluster.Instances.FirstOrDefault(c => c.Name == computerName);
            if (computer == null)
            {
                MessageBox.Show("Számítógép nem található a klaszterben");
                return;
            }

            List<ProgInstance> unmovablePrograms = new();
            List<string> deletedPrograms = new();
            Dictionary<ProgInstance, string> movedPrograms = new();

            foreach (var program in computer.Programs.ToList())
            {
                Instance? targetComputer = sourceCluster.Instances
                    .Where(c => c.Name != computerName)
                    .OrderByDescending(c => c.AvailableProcessorCapacity)
                    .ThenByDescending(c => c.AvailableMemoryCapacity)
                    .FirstOrDefault(c => c.CanAccommodateProgram(program));

                if (targetComputer != null)
                {
                    try
                    {
                        string sourcePath = Path.Combine(sourceComputerPath, program.ProgramName);
                        string targetPath = Path.Combine(sourceCluster.Path, targetComputer.Name, program.ProgramName);

                        File.Move(sourcePath, targetPath);
                        movedPrograms.Add(program, targetComputer.Name);

                        targetComputer.Programs.Add(program);
                        computer.Programs.Remove(program);
                    }
                    catch (Exception ex)
                    {
                        unmovablePrograms.Add(program);
                        MessageBox.Show($"Hiba a(z) {program.ProgramName} áthelyezésekor: {ex.Message}");
                    }
                }
                else
                {
                    unmovablePrograms.Add(program);
                }
            }

            if (unmovablePrograms.Count > 0)
            {
                foreach (ProgInstance program in unmovablePrograms)
                {
                    try
                    {
                        string filePath = Path.Combine(sourceComputerPath, program.ProgramName);
                        File.Delete(filePath);
                        sourceCluster.ScheduledPrograms.First(s => s.ProgramName == program.ProgramName).InstanceCount--;
                        computer.Programs.Remove(program);
                        deletedPrograms.Add(program.ProgramName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Nem sikerült törölni {program.ProgramName}: {ex.Message}");
                    }
                }
                MessageBox.Show($"Törölt programok:\n{string.Join("\n", deletedPrograms)}",
                              "Program törlések",
                              MessageBoxButton.OK);
                FileManager.WriteCluster(sourceCluster.Path, sourceCluster);
            }

                sourceCluster.Instances.Remove(computer);
                targetCluster.Instances.Add(computer);

            try
            {
                Directory.Move(sourceComputerPath, targetComputerPath);
                MessageBox.Show("Áthelyezés sikeresen befejeződött!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Végső áthelyezési hiba: {ex.Message}\n" +
                              "A számítógép mappa nem került átmozgatásra, de a klaszter állományok frissültek!");
            }
        }

        private Instance? FindSuitableTargetComputer(ProgInstance program, Cluster cluster, string excludeComputer = "")
        {
            return cluster.Instances
                .Where(c => c.Name != excludeComputer)
                .OrderBy(c => c.ProcessorUsagePercentage)
                .ThenBy(c => c.MemoryUsagePercentage)
                .FirstOrDefault(c => c.MemoryCapacity >= c.CalculateMemoryUsage() + program.MemoryUsage
                                && c.ProcessorCapacity >= c.CalculateProcessorUsage() + program.ProcessorUsage);
        }
    }
}