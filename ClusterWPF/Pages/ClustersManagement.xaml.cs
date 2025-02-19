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

        public void TransferProgramToCluster(string programName, Cluster sourceCluster, Cluster targetCluster)
        {
            Instance sourceComputer = sourceCluster.Instances.FirstOrDefault(c => c.Programs.Any(p => p.ProgramName == programName));
            if (sourceComputer == null)
            {
                MessageBox.Show("A program nem található a forrás klaszterben!");
                return;
            }

            if (targetCluster.Instances.Any(c => c.Programs.Any(p => p.ProgramName == programName)))
            {
                
            }
        }

        public void TransferComputerToCluster(string computerName, Cluster sourceCluster, Cluster targetCluster)
        {
            // Validate clusters are different
            if (sourceCluster.Path == targetCluster.Path)
            {
                MessageBox.Show("Forrás és célklaszter azonos!");
                return;
            }

            // Initial validation
            if (!ValidateTransferPrerequisites(computerName, sourceCluster, targetCluster))
            {
                MessageBox.Show("Áthelyezés előfeltételei nem teljesülnek!");
                return;
            }

            // Create operation context
            var operation = new TransferOperationContext
            {
                ComputerName = computerName,
                SourceCluster = sourceCluster,
                TargetCluster = targetCluster,
                MovedFiles = new Dictionary<string, string>(),
                OriginalProgramLocations = new Dictionary<ProgInstance, string>()
            };

            try
            {
                if (!PrepareProgramTransfers(operation))
                {
                    //RollbackProgramTransfers(operation);
                    return;
                }

                // Perform physical computer transfer
                MoveComputerFiles(operation);

                // Finalize cluster changes
                FinalizeClusterTransfer(operation);

                MessageBox.Show("Áthelyezés sikeresen befejeződött!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}");
                //RollbackFullOperation(operation);
            }
        }

        private bool ValidateTransferPrerequisites(string computerName, Cluster sourceCluster, Cluster targetCluster)
        {
            if (sourceCluster == null || targetCluster == null)
            {
                MessageBox.Show("Érvénytelen klaszter kiválasztás!");
                return false;
            }

            if (targetCluster.Instances.Any(c => c.Name == computerName))
            {
                MessageBox.Show("A gép már létezik a célklaszterben!");
                return false;
            }

            return true;
        }

        private bool PrepareProgramTransfers(TransferOperationContext context)
        {
            var computer = context.SourceCluster.Instances.First(c => c.Name == context.ComputerName);
            var programsToMove = new List<ProgInstance>(computer.Programs);
            var unmovablePrograms = new List<ProgInstance>();

            foreach (var program in programsToMove)
            {
                try
                {
                    string originalPath = Path.Combine(context.SourceCluster.Path, computer.Name, program.ProgramName);
                    context.OriginalProgramLocations.Add(program, originalPath);

                    var targetComputer = FindSuitableTargetComputer(program, context.SourceCluster, computer.Name);
                    if (targetComputer == null)
                    {
                        unmovablePrograms.Add(program);
                        continue;
                    }

                    // Move physical file
                    string newPath = Path.Combine(context.SourceCluster.Path, targetComputer.Name, program.ProgramName);
                    File.Move(originalPath, newPath);

                    context.MovedFiles.Add(originalPath, newPath);

                    targetComputer.Programs.Add(program);
                    computer.Programs.Remove(program);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba a(z) {program.ProgramName} áthelyezésekor: {ex.Message}");
                    return false;
                }
            }

            if (unmovablePrograms.Count > 0)
            {
                var result = ShowUnmovableProgramsDialog(unmovablePrograms);
                if (result != MessageBoxResult.Yes)
                {
                    return false;
                }

                // Delete unmovable programs
                foreach (var program in unmovablePrograms)
                {
                    try
                    {
                        string path = context.OriginalProgramLocations[program];
                        File.Delete(path);
                        computer.Programs.Remove(program);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Nem sikerült törölni {program.ProgramName}: {ex.Message}");
                        return false;
                    }
                }
            }

            return true;
        }

        private Instance FindSuitableTargetComputer(ProgInstance program, Cluster cluster, string excludeComputer)
        {
            return cluster.Instances
                .Where(c => c.Name != excludeComputer)
                .OrderBy(c => c.ProcessorUsagePercentage)
                .ThenBy(c => c.MemoryUsagePercentage)
                .FirstOrDefault(c => c.MemoryCapacity >= c.CalculateMemoryUsage() + program.MemoryUsage 
                                && c.ProcessorCapacity >= c.CalculateProcessorUsage() + program.ProcessorUsage);
        }

        private void MoveComputerFiles(TransferOperationContext context)
        {
            string sourcePath = Path.Combine(context.SourceCluster.Path, context.ComputerName);
            string targetPath = Path.Combine(context.TargetCluster.Path, context.ComputerName);

            if (!Directory.Exists(sourcePath))
            {
                throw new DirectoryNotFoundException($"Forrás mappa nem található: {sourcePath}");
            }

            Directory.CreateDirectory(targetPath);

            foreach (var file in Directory.GetFiles(sourcePath))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetPath, fileName);
                File.Move(file, destFile);
            }

            Directory.Delete(sourcePath);
        }

        private void FinalizeClusterTransfer(TransferOperationContext context)
        {
            var computer = context.SourceCluster.Instances.First(c => c.Name == context.ComputerName);

            context.SourceCluster.Instances.Remove(computer);
            context.TargetCluster.Instances.Add(computer);
        }

        //private void RollbackFullOperation(TransferOperationContext context)
        //{
        //    try
        //    {
        //        // Rollback file movements
        //        foreach (var kvp in context.MovedFiles)
        //        {
        //            if (File.Exists(kvp.Value))
        //            {
        //                File.Move(kvp.Value, kvp.Key);
        //            }
        //        }

        //        // Rollback program associations
        //        var computer = context.SourceCluster.Instances.FirstOrDefault(c => c.Name == context.ComputerName);
        //        if (computer != null)
        //        {
        //            foreach (var program in context.OriginalProgramLocations.Keys)
        //            {
        //                var targetComputer = context.SourceCluster.Instances
        //                    .FirstOrDefault(c => c.Programs.Contains(program));

        //                if (targetComputer != null)
        //                {
        //                    targetComputer.Programs.Remove(program);
        //                    computer.Programs.Add(program);
        //                }
        //            }
        //        }

        //        // Remove from target cluster if added
        //        if (context.TargetCluster.Instances.Contains(computer))
        //        {
        //            context.TargetCluster.Instances.Remove(computer);
        //            context.SourceCluster.Instances.Add(computer);
        //        }

        //        MessageBox.Show("Áthelyezés visszavonva. Az eredeti állapot helyreállt.");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Visszavonási hiba: {ex.Message}\nKézi beavatkozás szükséges!");
        //    }
        //}

        private MessageBoxResult ShowUnmovableProgramsDialog(List<ProgInstance> programs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Nem áthelyezhető programok:");
            foreach (var program in programs)
            {
                sb.AppendLine($"- {program.ProgramName}");
            }
            sb.AppendLine("\nSzeretné törölni ezeket a programokat?");

            return MessageBox.Show(sb.ToString(), "Nem áthelyezhető programok",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
        }

        private class TransferOperationContext
        {
            public string ComputerName { get; set; }
            public Cluster SourceCluster { get; set; }
            public Cluster TargetCluster { get; set; }
            public Dictionary<string, string> MovedFiles { get; set; } 
            public Dictionary<ProgInstance, string> OriginalProgramLocations { get; set; }
        }
    }
}