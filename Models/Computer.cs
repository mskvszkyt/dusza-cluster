using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuszaArpadWPF.Models
{
    public class Computer
    {
        public string Name { get; set; }
        public double TotalCPU { get; set; } // Millimag
        public int TotalMemory { get; set; } // MB
        public double UsedCPU { get; set; }
        public int UsedMemory { get; set; }
    }
}
