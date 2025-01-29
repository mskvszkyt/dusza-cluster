using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuszaArpadWPF.Models
{
    public class Process
    {
        public string Id { get; set; } // Pl. "chrome-asdfgh"
        public string Status { get; set; } // "AKTÍV" vagy "INAKTÍV"
        public string ProgramName { get; set; }
        public string ComputerName { get; set; }
    }
}
