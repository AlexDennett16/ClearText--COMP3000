using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using ClearText.Constants;

namespace ClearText.Utilities;

public static class PythonCleanUp
{
    internal static void KillExistingPythonServers()
    {
        Console.WriteLine("[PythonCleanUp] Checking for old Python processes...");

        foreach (var p in Process.GetProcessesByName("python"))
        {
            try
            {
                if (p.HasExited) continue;
                Console.WriteLine($"[PythonCleanUp] Killing stale python.exe (PID {p.Id})");
                p.Kill(true);
            }
            catch { /* ignore */ }
        }
    }

    internal static void EnsurePortFree()
    {
        var props = IPGlobalProperties.GetIPGlobalProperties();
        var listeners = props.GetActiveTcpListeners();

        if (listeners.Any(l => l.Port == PythonConstants.PythonPort))
        {
            throw new Exception($"Port {PythonConstants.PythonPort} is already in use. A zombie Python process may still be running.");
        }
    }
}