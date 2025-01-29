using System.Windows;
using DuszaArpadWPF.Models;

namespace DuszaArpadWPF.Views
{
    public partial class AddComputerDialog : Window
    {
        public string ComputerName { get; private set; }
        public double CpuCapacity { get; private set; }
        public int MemoryCapacity { get; private set; }

        public AddComputerDialog()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(CpuCapacityTextBox.Text, out var cpu) &&
                int.TryParse(MemoryCapacityTextBox.Text, out var memory))
            {
                new Computer();
                ComputerName = ComputerNameTextBox.Text;
                CpuCapacity = cpu;
                MemoryCapacity = memory;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Érvénytelen bemenet!");
            }
        }
    }
}