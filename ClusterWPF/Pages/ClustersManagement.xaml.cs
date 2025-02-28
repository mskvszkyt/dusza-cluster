using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        MainWindow mainWindow;
        public ClustersManagement()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            ObservableCollection<string> clusterNames = new(mainWindow.clusters.Select(c => c.Path.Split('\\').Last()));
            lbToCluster.ItemsSource = clusterNames;
            lbFromCluster.ItemsSource = clusterNames;
            lbClustersForInstance.ItemsSource = clusterNames.Where(cn => cn != mainWindow.cluster.Path.Split('\\').Last());
            lbClustersForPC.ItemsSource = clusterNames.Where(cn => cn != mainWindow.cluster.Path.Split('\\').Last());
            cbProgramInstances.ItemsSource = mainWindow.cluster?.Instances.SelectMany(c => c.Programs.Select(p => p.ProgramName));
            cbPCs.ItemsSource = mainWindow.cluster?.Instances.Select(c => c.Name);
        }

        public Cluster MergeClusters(Cluster cluster1, Cluster cluster2, string mergedClusterPath)
        {
            if (!ValidateMerge(cluster1, cluster2, mergedClusterPath))
                throw new InvalidOperationException("Hibás egyesítési kísérlet.");

            Cluster mergedCluster = new Cluster
            {
                Path = mergedClusterPath,
                Instances = new(),
                ScheduledPrograms = new()
            };

            MergeInstances(cluster1, mergedCluster);
            MergeInstances(cluster2, mergedCluster);

            MergeScheduledPrograms(cluster1, mergedCluster);
            MergeScheduledPrograms(cluster2, mergedCluster);

            Directory.CreateDirectory(mergedClusterPath);
            MoveComputerFiles(cluster1, mergedClusterPath);
            MoveComputerFiles(cluster2, mergedClusterPath);

            FileManager.WriteCluster(mergedClusterPath, mergedCluster);

            Directory.Delete(cluster1.Path, true);
            Directory.Delete(cluster2.Path, true);

            mainWindow.clusters.Remove(cluster2);
            mainWindow.clusters.Remove(cluster1);

            return mergedCluster;
        }

        private bool ValidateMerge(Cluster cluster1, Cluster cluster2, string targetPath)
        {
            if (cluster1.Path == cluster2.Path)
                throw new ArgumentException("Cannot merge identical clusters");

            if (Directory.Exists(targetPath) && Directory.GetFiles(targetPath).Any())
                throw new InvalidOperationException("Target path must be empty");

            if (cluster1.Instances.Select(c => c.Name)
                .Intersect(cluster2.Instances.Select(c => c.Name)).Any())
                throw new InvalidOperationException("Duplicate computer names detected");

            return true;
        }

        private void MergeInstances(Cluster source, Cluster target)
        {
            foreach (var instance in source.Instances)
            {
                // Create clone to avoid reference issues
                var newInstance = new Instance
                {
                    Name = instance.Name,
                    ProcessorCapacity = instance.ProcessorCapacity,
                    MemoryCapacity = instance.MemoryCapacity,
                    Programs = new()
                };

                foreach (var program in instance.Programs)
                {
                    newInstance.Programs.Add(new ProgInstance
                    {
                        ProgramName = program.ProgramName,
                        ProcessorUsage = program.ProcessorUsage,
                        MemoryUsage = program.MemoryUsage
                    });
                }

                target.Instances.Add(newInstance);
            }
        }

        private void MergeScheduledPrograms(Cluster source, Cluster target)
        {
            foreach (ScheduledProgram sourceProgram in source.ScheduledPrograms)
            {
                ScheduledProgram? existingProgram = target.ScheduledPrograms
                    .FirstOrDefault(p => p.ProgramName == sourceProgram.ProgramName);

                if (existingProgram != null)
                {
                    // Merge program requirements using maximum values
                    existingProgram.ProcessorRequirement = Math.Max(
                        existingProgram.ProcessorRequirement,
                        sourceProgram.ProcessorRequirement);

                    existingProgram.MemoryRequirement = Math.Max(
                        existingProgram.MemoryRequirement,
                        sourceProgram.MemoryRequirement);

                    existingProgram.InstanceCount += sourceProgram.InstanceCount;
                }
                else
                {
                    target.ScheduledPrograms.Add(new ScheduledProgram
                    {
                        ProgramName = sourceProgram.ProgramName,
                        ProcessorRequirement = sourceProgram.ProcessorRequirement,
                        MemoryRequirement = sourceProgram.MemoryRequirement,
                        InstanceCount = sourceProgram.InstanceCount
                    });
                }
            }
        }

        private void MoveComputerFiles(Cluster source, string targetPath)
        {
            foreach (var instance in source.Instances)
            {
                string sourceDir = Path.Combine(source.Path, instance.Name);
                string targetDir = Path.Combine(targetPath, instance.Name);

                if (!Directory.Exists(sourceDir))
                    throw new DirectoryNotFoundException($"Forrásmappa nem található: {sourceDir}");

                if (Directory.Exists(targetDir))
                    throw new IOException($"A mappa már létezik: {targetDir}");

                Directory.Move(sourceDir, targetDir);
            }
        }
        public static void RollbackMerge(string mergedClusterPath)
        {
            try
            {
                if (Directory.Exists(mergedClusterPath))
                    Directory.Delete(mergedClusterPath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rollback failed: {ex.Message}");
            }
        }

        public void TransferProgramToCluster(ProgInstance program, Cluster sourceCluster, Cluster targetCluster)
        {
            sourceCluster = mainWindow.cluster;
            if (sourceCluster.Path == targetCluster.Path)
            {
                MessageBox.Show("Válassz ki egy különböző");
            }
            Instance? sourceComputer = sourceCluster.Instances.FirstOrDefault(c => c.Programs.Any(p => p.ProgramName == program.ProgramName));
            if (sourceComputer == null)
            {
                MessageBox.Show("A program nem található a forrás klaszterben!");
                return;
            }

            ScheduledProgram newScheduledProgram = new ScheduledProgram
            {
                ProgramName = program.ProgramName.Split('-').First(),
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
                program = new ProgInstance
                {
                    ProgramName = program.ProgramName,
                    ProcessorUsage = scheduledProgram.ProcessorRequirement,
                    MemoryUsage = scheduledProgram.MemoryRequirement,
                    IsRunning = program.IsRunning,
                    StartDate = program.StartDate
                };
                scheduledProgram.InstanceCount++;
            }
            else
            {
                targetCluster.ScheduledPrograms.Add(newScheduledProgram);
            }

            Instance? targetComputer = FindSuitableTargetComputer(program, targetCluster);
            if (targetComputer == null)
            {
                MessageBox.Show("Nincs megfelelő gép a célklaszterben!");
                return;
            }

            if (targetCluster.Instances.Any(c => c.Programs.Any(p => p.ProgramName.Split('-').Last() == program.ProgramName.Split('-').Last())))
            {
                string newKey = ProgramManager.GenerateUniqueKey(FileManager.GetExistingKeys(targetCluster.Path));
                program.ProgramName = $"{program.ProgramName}-{newKey}";
                MessageBox.Show($"A program már létezik a célklaszterben, új kulcs hozzáadva! {newKey}");
            }



            string sourcePath = Path.Combine(sourceCluster.Path, sourceComputer.Name, program.ProgramName);
            string targetPath = Path.Combine(targetCluster.Path, targetComputer.Name, program.ProgramName);
            File.Move(sourcePath, targetPath);
            sourceCluster.ScheduledPrograms.First(s => s.ProgramName == program.ProgramName.Split('-').First()).InstanceCount--;
            sourceComputer.Programs.Remove(program);
            targetComputer.Programs.Add(program);
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

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            Cluster? cluster1 = mainWindow.clusters.First(c => c.Path.Split('\\').Last() == lbFromCluster.SelectedItem.ToString());
            Cluster? cluster2 = mainWindow.clusters.First(c => c.Path.Split('\\').Last() == lbToCluster.SelectedItem.ToString());
            if (cluster1 == null || cluster2 == null)
            {
                MessageBox.Show("Válassz egy klasztert előbb!");
                return;
            }

            try
            {
                if (Directory.Exists(Path.Combine(Path.GetDirectoryName(cluster2.Path), $"{cluster1.Path.Split('\\').Last()}-{cluster2.Path.Split("\\").Last()}")))
                {
                    MessageBox.Show("Ezeknek a klasztereknek már létezik egyesített példánya.");
                    return;
                }
                string mergePath = Path.Combine(Path.GetDirectoryName(cluster2.Path), $"{cluster1.Path.Split('\\').Last()}-{cluster2.Path.Split("\\").Last()}");
                MergeClusters(cluster1, cluster2, mergePath);
                lbFromCluster.ItemsSource = mainWindow.clusters.Select(c => c.Path.Split('\\').Last());
                lbToCluster.ItemsSource = mainWindow.clusters.Select(c => c.Path.Split('\\').Last());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnMoveInstance_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.cluster == null || lbClustersForInstance.SelectedItem == null)
            {
                MessageBox.Show("Válassz ki egy klasztert előbb!");
                return;
            }
            if (cbProgramInstances.SelectedItem == null)
            {
                MessageBox.Show("Válassz ki egy programot előbb!");
                return;
            }
            ProgInstance program = mainWindow.cluster.Instances.SelectMany(c => c.Programs).First(p => p.ProgramName == cbProgramInstances.SelectedItem.ToString());
            TransferProgramToCluster(program, mainWindow.cluster, mainWindow.clusters.First(c => c.Path.Split('\\').Last() == lbClustersForInstance.SelectedItem.ToString()));

            cbProgramInstances.ItemsSource = mainWindow.cluster?.Instances.SelectMany(c => c.Programs.Select(p => p.ProgramName));
        }

        private void btnMovePC_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.cluster == null || lbClustersForPC.SelectedItem == null)
            {
                MessageBox.Show("Válassz ki egy klasztert előbb!");
                return;
            }
            if (cbPCs.SelectedItem == null)
            {
                MessageBox.Show("Válassz ki egy gépet előbb!");
                return;
            }
            TransferComputerToCluster(cbPCs.SelectedItem.ToString(), mainWindow.cluster, mainWindow.clusters.First(c => c.Path.Split('\\').Last() == lbClustersForPC.SelectedItem.ToString()));

            cbPCs.ItemsSource = mainWindow.cluster?.Instances.Select(c => c.Name);
        }
    }
}
