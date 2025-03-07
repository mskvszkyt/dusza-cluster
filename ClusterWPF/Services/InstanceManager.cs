﻿using System;
using System.IO;

namespace ConsoleApp1
{
    public static class InstanceManager
    {
        public static void RemoveInstance(string instanceName, string path)
        {
            bool validate = false;
            foreach (string instance in Directory.GetDirectories(path))
            {
                if (Path.GetFileName(instance) == instanceName)
                {
                    // Check for active programs
                    foreach (string programFile in Directory.GetFiles(instance))
                    {
                        if (Path.GetFileName(programFile) == ".szamitogep_config") continue;

                        string[] programData = File.ReadAllLines(programFile);
                        if (programData[1] == "AKTÍV")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("HIBA: PROGRAM FUT A GÉPEN");
                            Console.ForegroundColor = ConsoleColor.White;
                            return;
                        }
                    }

                    Directory.Delete(instance, true);
                    Console.WriteLine("Sikeres számítógép törlés!");
                    validate = true;
                    break;
                }
            }

            if (!validate)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("HIBA: NEM TALÁLHATÓ SZÁMÍTÓGÉP");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}