using System.IO;
using System.Windows;

namespace DuszaArpadWPF.Views
{
    public partial class PathDialog : Window
    {
        public string Path { get; private set; }

        public PathDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(PathTextBox.Text))
            {
                Path = PathTextBox.Text;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Az elérési út nem létezik!");
            }
        }
    }
}
