using DuszaArpadWPF.Services;
using DuszaArpadWPF.Views;
using System.Windows;

namespace DuszaArpadWPF
{
    public partial class App : Application
    {
        //private void Application_Startup(object sender, StartupEventArgs e)
        //{
        //    // Path bekérése
        //    var pathDialog = new PathDialog();
        //    if (pathDialog.ShowDialog() == true)
        //    {
        //        try
        //        {
        //            // Főablak indítása és a path átadása
        //            var clusterService = new ClusterService();
        //            clusterService.SetRootPath(pathDialog.Path);

        //            MainWindow mainWindow = new MainWindow(clusterService);

        //            // Az alkalmazás főablakának beállítása
        //            mainWindow.Show();
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Hiba történt: {ex.Message}");
        //            Shutdown();
        //        }
        //    }
        //    else
        //    {
        //        Shutdown();
        //    }
        //}

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            new MainWindow().Show();
        }
    }
}