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
        private MainWindow mainWindow;

        public ComputerRemove()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            cbComputers.ItemsSource = mainWindow.cluster.Instances.Select(i => i.Name).ToList();
            btnRemoveComputer.Click += BtnRemoveComputer_Click;
        }

        private void BtnRemoveComputer_Click(object sender, RoutedEventArgs e)
        {
            if (cbComputers.SelectedItem is not string selectedInstanceName)
            {
                MessageBox.Show("Válassz egy számítógépet az eltávolításhoz!", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var instanceToRemove = mainWindow.cluster.Instances.FirstOrDefault(i => i.Name == selectedInstanceName);
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
                    var availableInstances = mainWindow.cluster.Instances.Where(i => i != instanceToRemove).ToList();

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
                            string oldPath = Path.Combine(mainWindow.cluster.Path, instanceToRemove.Name, program.ProgramName);
                            string newPath = Path.Combine(mainWindow.cluster.Path, targetInstance.Name, program.ProgramName);

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

            // Számítógép eltávolítása
            mainWindow.cluster.Instances.Remove(instanceToRemove);

            string instanceFolderPath = Path.Combine(mainWindow.cluster.Path, instanceToRemove.Name);
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

            // Üres cluster mappa törlése, ha szükséges
            if (Directory.Exists(mainWindow.cluster.Path) && !Directory.EnumerateFileSystemEntries(mainWindow.cluster.Path).Any())
            {
                try
                {
                    Directory.Delete(mainWindow.cluster.Path);
                    MessageBox.Show($"Törölve: {mainWindow.cluster.Path} (üres klaszter mappa)");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Nem sikerült törölni a klaszter mappát {mainWindow.cluster.Path}!\nHiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Lista frissítése
            mainWindow.cluster.Instances = mainWindow.cluster.Instances.ToList();
            cbComputers.ItemsSource = mainWindow.cluster.Instances.Select(i => i.Name).ToList();
        }
    }
}
