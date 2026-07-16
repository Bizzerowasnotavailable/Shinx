using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

public static partial class CpuIdNative
{
    /// <summary>
    /// Executes CPUID with the specified leaf and sub-leaf.
    /// </summary>
    /// <param name="cpuInfo">Output array of 4 int32 [eax, ebx, ecx, edx] (modified in-place)</param>
    /// <param name="functionId">CPUID leaf (EAX)</param>
    /// <param name="subFunctionId">CPUID sub-leaf (ECX)</param>
    [LibraryImport("*", EntryPoint = "RhCpuIdEx")]
    [SuppressGCTransition]
    public static partial void GetCpuId(int[] cpuInfo, int functionId, int subFunctionId);
}