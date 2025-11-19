using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;

public static class PythonRunner
{
    public static async Task RunPythonAsync(string scriptName)
    {
        string pythonPath = @"C:\Users\Carl Ferrariz\AppData\Local\Programs\Python\Python313\python.exe";
        string workingDir = Path.Combine(Application.dataPath, "../BotAI");
        string scriptPath = Path.Combine(workingDir, scriptName);

        if (!File.Exists(scriptPath))
        {
            UnityEngine.Debug.LogError("❌ Python script not found: " + scriptPath);
            return;
        }

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{scriptPath}\"",
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        UnityEngine.Debug.Log("▶ Running Python: " + psi.Arguments);

        Process proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

        proc.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                UnityEngine.Debug.Log("🐍 PYTHON OUT: " + e.Data);
        };

        proc.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                UnityEngine.Debug.LogError("🐍 PYTHON ERROR: " + e.Data);
        };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        // Async wait for process to exit without blocking Unity
        await Task.Run(() => proc.WaitForExit());
        proc.Close();
    }
}
