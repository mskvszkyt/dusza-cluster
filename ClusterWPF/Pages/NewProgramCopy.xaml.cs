﻿using ConsoleApp1;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for NewProgramCopy.xaml
    /// </summary>
    public partial class NewProgramCopy : Page
    {
        private Cluster _cluster;
        private string _clusterPath;
        private bool _isUpdating = false;
        public NewProgramCopy(Cluster cluster, string path)
        {
            InitializeComponent();
            _cluster = cluster;
            _clusterPath = path;
            PopulateComboBoxes();
        }
       
        /// <summary>
        /// Populates the ComboBoxes with available scheduled programs and inactive program instances.  
        /// Scheduled programs are those that can be run, while inactive programs are already on the machine  
        /// but not currently running.
        /// </summary>
        private void PopulateComboBoxes()
        {
            // Get scheduled programs (Programs that can be run)
            var scheduledPrograms = _cluster.ScheduledPrograms
                .Select(sp => sp.ProgramName)
                .Distinct()
                .ToList();

            // Get programs that are already running but inactive
            var inactivePrograms = _cluster.Instances
                .SelectMany(i => i.Programs)
                .Where(p => !p.IsRunning)  // Select only inactive ones
                .Select(p => p.ProgramName)
                .Distinct()
                .ToList();

            cbPrograms.ItemsSource = scheduledPrograms;
            cbComputers.ItemsSource = inactivePrograms;
        }

        private void cbPrograms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating) return; // Prevent recursive calls

            _isUpdating = true;
            cbComputers.SelectedItem = null; // Clear the other ComboBox
            _isUpdating = false;
        }

        // Event handler for ComboBox selection changed (for cbComputers)
        private void cbComputers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating) return; // Prevent recursive calls

            _isUpdating = true;
            cbPrograms.SelectedItem = null; // Clear the other ComboBox
            _isUpdating = false;
        }


        private void btnRunProgram_Click(object sender, RoutedEventArgs e)
        {
            string? selectedProgram = cbComputers.SelectedItem as string ?? cbPrograms.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedProgram))
            {
                MessageBox.Show("Kérlek, válassz egy programot!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- Handling already running programs from cbComputers ---
            if (cbComputers.SelectedItem != null)
            {
                Instance? currentInstance = _cluster.Instances
                    .FirstOrDefault(i => i.Programs.Any(p => p.ProgramName == selectedProgram));

                if (currentInstance == null)
                {
                    MessageBox.Show("Nem található olyan számítógép, amely tartalmazza ezt a programot.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ProgInstance? existingInstance = currentInstance.Programs
                    .FirstOrDefault(p => p.ProgramName == selectedProgram);

                if (existingInstance == null)
                {
                    MessageBox.Show("Nem található a program példánya.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (currentInstance.CalculateProcessorUsage() + existingInstance.ProcessorUsage <= currentInstance.ProcessorCapacity &&
                    currentInstance.CalculateMemoryUsage() + existingInstance.MemoryUsage <= currentInstance.MemoryCapacity)
                {
                    existingInstance.IsRunning = true;
                    MessageBox.Show($"A(z) {existingInstance.ProgramName} program elindult ezen: {currentInstance.Name}.", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

                    // File writing for already running program instance
                    string instancePath = Path.Combine(_clusterPath, currentInstance.Name);
                    Directory.CreateDirectory(instancePath);

                    File.WriteAllText(
                        Path.Combine(instancePath, existingInstance.ProgramName),
                        $"{existingInstance.StartDate}\nAKTÍV\n{existingInstance.ProcessorUsage}\n{existingInstance.MemoryUsage}"
                    );
                }
                else
                {
                    var result = MessageBox.Show($"A(z) {existingInstance.ProgramName} nem futtatható ezen a számítógépen: {currentInstance.Name}.\nÁthelyezed egy másik számítógépre?",
                        "Megerősítés", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        Instance? newInstance = _cluster.Instances
                            .Where(i => i != currentInstance &&
                                        i.CalculateProcessorUsage() + existingInstance.ProcessorUsage <= i.ProcessorCapacity &&
                                        i.CalculateMemoryUsage() + existingInstance.MemoryUsage <= i.MemoryCapacity)
                            .OrderBy(i => i.CalculateProcessorUsage() + i.CalculateMemoryUsage())
                            .FirstOrDefault();

                        if (newInstance == null)
                        {
                            MessageBox.Show("Nincs elérhető számítógép az áthelyezéshez!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        currentInstance.Programs.Remove(existingInstance);
                        newInstance.Programs.Add(existingInstance);
                        existingInstance.IsRunning = true;

                        MessageBox.Show($"A(z) {existingInstance.ProgramName} program elindult a {newInstance.Name} számítógépen.", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

                        // File writing for program moved to new instance
                        string newInstancePath = Path.Combine(_clusterPath, newInstance.Name);
                        Directory.CreateDirectory(newInstancePath);

                        File.WriteAllText(
                            Path.Combine(newInstancePath, existingInstance.ProgramName),
                            $"{existingInstance.StartDate}\nAKTÍV\n{existingInstance.ProcessorUsage}\n{existingInstance.MemoryUsage}"
                        );
                    }
                }
            }


            else if (cbPrograms.SelectedItem != null)
            {
                ScheduledProgram? scheduledProgram = _cluster.ScheduledPrograms
                    .FirstOrDefault(sp => sp.ProgramName == selectedProgram);

                if (scheduledProgram == null)
                {
                    MessageBox.Show("A kiválasztott program nem található az ütemezett programok között.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int instancesToRun = scheduledProgram.InstanceCount;
                int instancesStarted = 0;
                string baseProgramName = selectedProgram.Split('-')[0];

                int currentlyRunningInstances = _cluster.Instances
                    .SelectMany(i => i.Programs)
                    .Count(p => p.ProgramName.StartsWith(baseProgramName + "-"));

                int instancesNeeded = instancesToRun - currentlyRunningInstances;

                if (instancesNeeded <= 0)
                {
                    var result = MessageBox.Show($"Az összes ütemezett példány már fut. Növeljük az ütemezett példányszámot?",
                        "Megerősítés", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        scheduledProgram.InstanceCount++;
                        instancesNeeded = 1;
                    }
                    else
                    {
                        return;
                    }
                }

                List<Instance> availableInstances = _cluster.Instances
                    .Where(i => i.CalculateProcessorUsage() < i.ProcessorCapacity &&
                                i.CalculateMemoryUsage() < i.MemoryCapacity)
                    .OrderBy(i => i.CalculateProcessorUsage() + i.CalculateMemoryUsage())
                    .ToList();

                foreach (var instance in availableInstances)
                {
                    if (instancesStarted >= instancesNeeded) break;

                    string key = ProgramManager.GenerateUniqueKey(FileManager.GetExistingKeys(_clusterPath));
                    string uniqueProgramName = $"{baseProgramName}-{key}";

                    ProgInstance newProgramInstance = new ProgInstance(
                        uniqueProgramName,
                        true,
                        scheduledProgram.ProcessorRequirement,
                        scheduledProgram.MemoryRequirement,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    );

                    if (instance.CalculateProcessorUsage() + newProgramInstance.ProcessorUsage <= instance.ProcessorCapacity &&
                        instance.CalculateMemoryUsage() + newProgramInstance.MemoryUsage <= instance.MemoryCapacity)
                    {
                        instance.Programs.Add(newProgramInstance);
                        instancesStarted++;

                        // Create physical file
                        string instancePath = Path.Combine(_clusterPath, instance.Name);
                        Directory.CreateDirectory(instancePath);
                        File.WriteAllText(
                            Path.Combine(instancePath, newProgramInstance.ProgramName),
                            $"{newProgramInstance.StartDate}\nAKTÍV\n{newProgramInstance.ProcessorUsage}\n{newProgramInstance.MemoryUsage}"
                        );
                    }
                }

                if (instancesStarted < instancesNeeded)
                {
                    MessageBox.Show($"Nem minden példányt sikerült elindítani. Próbálja meg később.",
                        "Információ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Összesen {instancesStarted} példány indult el.", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }


        private void BtnCreateProgram_Click(object sender, RoutedEventArgs e)
        {
            string programName = tbNewProgramName.Text.Trim();
            string memoryUsageText = tbNewProgramsMemory.Text.Trim();
            string processorUsageText = tbNewProgramsProcessor.Text.Trim();
            string instanceCountText = tbInstanceCount.Text.Trim();

            if (string.IsNullOrEmpty(programName) || string.IsNullOrEmpty(memoryUsageText) ||
                string.IsNullOrEmpty(processorUsageText) || string.IsNullOrEmpty(instanceCountText))
            {
                MessageBox.Show("Minden mezőt ki kell tölteni!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(memoryUsageText, out int memoryUsage) || memoryUsage <= 0)
            {
                MessageBox.Show("Érvénytelen memóriaérték!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(processorUsageText, out int processorUsage) || processorUsage <= 0)
            {
                MessageBox.Show("Érvénytelen processzorhasználat!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(instanceCountText, out int instanceCount) || instanceCount <= 0)
            {
                MessageBox.Show("Érvénytelen példányszám!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if the program name already exists in ScheduledPrograms
            if (_cluster.ScheduledPrograms.Any(sp => sp.ProgramName == programName))
            {
                MessageBox.Show("Ez a program már létezik az ütemezett programok között!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create the scheduled program
            ScheduledProgram newScheduledProgram = new ScheduledProgram
            {
                ProgramName = programName,
                InstanceCount = instanceCount,
                ProcessorRequirement = processorUsage,
                MemoryRequirement = memoryUsage
            };

            _cluster.ScheduledPrograms.Add(newScheduledProgram);

            // Write the updated cluster data to the file
            FileManager.WriteCluster(_clusterPath, _cluster);

            MessageBox.Show($"A(z) {programName} program sikeresen hozzáadva az ütemezett programokhoz!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear input fields
            tbNewProgramName.Clear();
            tbNewProgramsMemory.Clear();
            tbNewProgramsProcessor.Clear();
            tbInstanceCount.Clear();

            // Refresh UI elements (e.g., update ComboBoxes if needed)
            PopulateComboBoxes();
        }


    }
}
