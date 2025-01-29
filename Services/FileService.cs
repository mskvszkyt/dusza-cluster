using System.IO;

namespace DuszaArpadWPF.Services
{
    public class FileService
    {
        public List<string> GetDirectories(string path)
        {
            return Directory.GetDirectories(path).ToList();
        }

        public List<string> GetFiles(string path)
        {
            return Directory.GetFiles(path).ToList();
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public string[] ReadAllLines(string path)
        {
            return File.ReadAllLines(path);
        }
    }
}