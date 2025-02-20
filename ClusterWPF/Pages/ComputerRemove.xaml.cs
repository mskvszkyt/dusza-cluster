using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClusterWPF.Pages
{
    public partial class ComputerRemove : Page
    {
        private Cluster _cluster;
        private List<Instance> _allInstances;
        private readonly string _clusterPath;

        public ComputerRemove(Cluster cluster, string clusterPath)
        {
            InitializeComponent();
            _cluster = cluster;
            _clusterPath = clusterPath;
            _allInstances = _cluster.Instances.ToList();
            cbComputers.ItemsSource = _allInstances.Select(i => i.Name).ToList();
            btnRemoveComputer.Click += BtnRemoveComputer_Click;
        }

        private void BtnRemoveComputer_Click(object sender, RoutedEventArgs e)
        {
            if (cbComputers.SelectedItem is not string selectedInstanceName)
            {
                MessageBox.Show("Válassz egy számítógépet az eltávolításhoz!", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var instanceToRemove = _cluster.Instances.FirstOrDefault(i => i.Name == selectedInstanceName);
            if (instanceToRemove == null) return;

            if (instanceToRemove.Programs.Any())
            {
                var moveChoice = MessageBox.Show(
                    "Ez a számítógép futtat programokat. Át szeretnéd helyezni ezeket egy másik számítógépre?",
                    "Programok áthelyezése",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (moveChoice == MessageBoxResult.Yes)
                {
                    var availableInstances = _cluster.Instances.Where(i => i != instanceToRemove).ToList();

                    if (!availableInstances.Any())
                    {
                        MessageBox.Show("Nincs elérhető számítógép a programok áthelyezésére!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    foreach (var program in instanceToRemove.Programs.ToList())
                    {
                        var targetInstance = availableInstances.FirstOrDefault(i =>
                            i.CalculateProcessorUsage() + program.ProcessorUsage <= i.ProcessorCapacity &&
                            i.CalculateMemoryUsage() + program.MemoryUsage <= i.MemoryCapacity);

                        if (targetInstance != null)
                        {
                            targetInstance.Programs.Add(program);
                            instanceToRemove.Programs.Remove(program);

                            // Programfájl áthelyezése
                            string oldPath = Path.Combine(_clusterPath, instanceToRemove.Name, program.ProgramName);
                            string newPath = Path.Combine(_clusterPath, targetInstance.Name, program.ProgramName);

                            if (File.Exists(oldPath))
                            {
                                try
                                {
                                    File.Move(oldPath, newPath);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Nem sikerült áthelyezni a(z) {program.ProgramName} programot!\nHiba: {ex.Message}",
                                        "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Nincs elég erőforrás a(z) {program.ProgramName} áthelyezésére!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else if (moveChoice == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            // Deleting the computer
            _cluster.Instances.Remove(instanceToRemove);

            string instanceFolderPath = Path.Combine(_clusterPath, instanceToRemove.Name);
            if (Directory.Exists(instanceFolderPath))
            {
                try
                {
                    Directory.Delete(instanceFolderPath, true);
                    MessageBox.Show($"Törölve: {instanceFolderPath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Nem sikerült törölni a(z) {instanceFolderPath} mappát!\nHiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Removing the cluster folder if it's empty
            if (Directory.Exists(_clusterPath) && !Directory.EnumerateFileSystemEntries(_clusterPath).Any())
            {
                try
                {
                    Directory.Delete(_clusterPath);
                    MessageBox.Show($"Törölve: {_clusterPath} (üres klaszter mappa)");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Nem sikerült törölni a klaszter mappát {_clusterPath}!\nHiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Refreshing the list
            _allInstances = _cluster.Instances.ToList();
            cbComputers.ItemsSource = _allInstances.Select(i => i.Name).ToList();
        }
    }
}
