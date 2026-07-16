using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Bridge;

namespace Cosmos.Kernel.Core.CPU;

/// <summary>
/// CPUID wrapper for getting CPU information.
/// Uses native RhCpuIdEx from Cosmos.Kernel.Core native code.
/// </summary>
public static class CpuId
{
    /// <summary>
    /// Gets the CPU brand string (e.g., "Intel(R) Core(TM) i7-XXXX CPU @ X.XXGHz").
    /// Reads CPUID leaves 0x80000002, 0x80000003, 0x80000004.
    /// </summary>
    public static string GetBrandString()
    {
        var buffer = new byte[48];
        
        // Leaf 0x80000002 - bytes 0-15
        var cpuInfo = new int[4];
        CpuIdNative.GetCpuId(cpuInfo, unchecked((int)0x80000002), 0);
        Buffer.BlockCopy(BitConverter.GetBytes(cpuInfo[0]), 0, buffer, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(cpuInfo[1]), 0, buffer, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(cpuInfo[2]), 0, buffer, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(cpuInfo[3]), 0, buffer, 12, 4);

        // Leaf 0x80000003
        CpuIdNative.GetCpuId(cpuInfo, unchecked((int)0x80000003), 0);
        Buffer.BlockCopy(BitConverter.GetBytes(cpuInfo[0]), 0, buffer, 16, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(cpuInfo[1]), 0, buffer, 20, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(cpuInfo[2]), 0, buffer, 24, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(cpuInfo[3]), 0, buffer, 28, 4);

        // Leaf 0x80000004
        CpuIdNative.GetCpuId(cpuInfo, unchecked((int)0x80000004), 0);
        Buffer.BlockCopy(BitConverter.GetBytes(cpuInfo[0]), 0, buffer, 32, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(cpuInfo[1]), 0, buffer, 36, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(cpuInfo[2]), 0, buffer, 40, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(cpuInfo[3]), 0, buffer, 44, 4);

        // Find null terminator or trim trailing spaces
        int len = 0;
        while (len < buffer.Length && buffer[len] != 0) len++;
        
        // Manual ASCII conversion to avoid System.Text.Encoding dependency
        char[] chars = new char[len];
        for (int i = 0; i < len; i++) chars[i] = (char)buffer[i];
        return new string(chars).TrimEnd(' ');
    }

    /// <summary>
    /// Executes CPUID with the specified leaf and sub-leaf.
    /// </summary>
    public static void GetCpuId(int[] cpuInfo, int functionId, int subFunctionId)
    {
        if (cpuInfo == null || cpuInfo.Length < 4)
            throw new ArgumentException("cpuInfo must have at least 4 elements", nameof(cpuInfo));
        
        CpuIdNative.GetCpuId(cpuInfo, functionId, subFunctionId);
    }
}