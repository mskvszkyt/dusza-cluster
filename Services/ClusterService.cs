using System.IO;
using DuszaArpadWPF.Models;

namespace DuszaArpadWPF.Services
{
    public class ClusterService
    {
        private readonly FileService _fileService = new FileService();
        private string _rootPath;

        public void SetRootPath(string path)
        {
            _rootPath = path;
        }

        public List<Computer> LoadComputers()
        {
            var computers = new List<Computer>();
            foreach (var dir in _fileService.GetDirectories(_rootPath))
            {
                var configFile = Path.Combine(dir, ".szamitogep_config");
                if (File.Exists(configFile))
                {
                    var lines = _fileService.ReadAllLines(configFile);
                    computers.Add(new Computer
                    {
                        Name = Path.GetFileName(dir),
                        TotalCPU = double.Parse(lines[0]),
                        TotalMemory = int.Parse(lines[1])
                    });
                }
            }
            return computers;
        }

        public void AddComputer(string name, double cpu, int memory)
        {
            var path = Path.Combine(_rootPath, name);
            _fileService.CreateDirectory(path);
            _fileService.WriteAllText(
                Path.Combine(path, ".szamitogep_config"),
                $"{cpu}\n{memory}"
            );
        }
    }
}