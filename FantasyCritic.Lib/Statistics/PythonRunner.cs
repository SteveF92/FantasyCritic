using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Statistics
{
    public class PythonRunner
    {
        private readonly string _pythonPath;

        public PythonRunner(string pythonPath)
        {
            _pythonPath = pythonPath;
        }

        public string RunPython(string scriptFilePath, string dataFilePath)
        {
            // 1) Create Process Info
            var psi = new ProcessStartInfo();
            psi.FileName = _pythonPath;

            psi.ArgumentList.Add(scriptFilePath);
            psi.ArgumentList.Add(dataFilePath);

            // 3) Process configuration
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            // 4) Execute process and get output
            var results = "";

            using (var process = Process.Start(psi))
            {
                results = process.StandardOutput.ReadToEnd();
            }

            return results;
        }
    }
}
