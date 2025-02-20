﻿using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using ConsoleApp1;

namespace ClusterWPF.Pages
{
    public partial class RemoveProgram : Page
    {
        private readonly Cluster _cluster;
        private readonly string _path;

        // Use ObservableCollection for dynamic updates
        private ObservableCollection<dynamic> _programs;

        public RemoveProgram(Cluster cluster, string path)
        {
            InitializeComponent();
            _cluster = cluster;
            _path = path;
            LoadPrograms();

            btnRemoveProgram.Click += BtnRemoveProgram_Click;
        }

        private void LoadPrograms()
        {
            // Fetch all scheduled programs and display them in the ListBox
            _programs = new ObservableCollection<dynamic>(_cluster.ScheduledPrograms
                .Select(p => p.ProgramName)
                .ToList());

            cbPrograms.ItemsSource = _programs;
        }



        private void BtnRemoveProgram_Click(object sender, RoutedEventArgs e)
        {
            if (cbPrograms.SelectedItem == null)
            {
                MessageBox.Show("Please select a program to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get the selected program details
            var selectedProgram = cbPrograms.SelectedItem as dynamic;
            string programName = selectedProgram;

            // Call the method to shut down the program
            ShutDownProgram(programName, _path, _cluster);

            // Reload the list of programs after removal
            LoadPrograms();
        }
        /// <summary>
        /// Shuts down a program by removing it from the scheduled programs, deleting all related instances 
        /// and files, and updating the cluster configuration.
        /// </summary>
        /// <param name="programName">The name of the program to shut down.</param>
        /// <param name="path">The base path where program files are stored.</param>
        /// <param name="cluster">The cluster containing the program instances.</param>

        public static void ShutDownProgram(string programName, string path, Cluster cluster)
        {
            // 1. Remove from ScheduledPrograms
            var scheduledProgram = cluster.ScheduledPrograms.FirstOrDefault(sp => sp.ProgramName == programName);
            if (scheduledProgram == null)
            {
                ShowError("Error: Program not found");
                return;
            }
            cluster.ScheduledPrograms.Remove(scheduledProgram);

            // 2. Delete all related program instances and files
            bool anyDeleted = false;
            foreach (var instance in cluster.Instances)
            {
                var programsToRemove = instance.Programs
                    .Where(p => p.ProgramName.StartsWith($"{programName}-", StringComparison.Ordinal))
                    .ToList();

                foreach (var program in programsToRemove)
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

            MessageBox.Show(anyDeleted
                ? "Program and all its instances have been successfully removed."
                : "Warning: The program was not running on any machine.",
                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
