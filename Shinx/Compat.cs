using System;
using Sys = Cosmos.Kernel.System;
using Cosmos.Kernel.System.Timer;

namespace Shinx
{
    /// <summary>
    /// Reads total physical RAM from the Limine memory map.
    /// </summary>
    public static class LimineMemory
    {
        public static unsafe ulong GetTotalPhysicalRamMB()
        {
            var response = Cosmos.Kernel.Boot.Limine.Limine.MemoryMap.Response;
            if (response == null) return 0;

            // Dereference the pointer to get the actual response struct
            var responseVal = response[0];
            if (responseVal.Entries == null) return 0;

            ulong totalBytes = 0;

            for (ulong i = 0; i < responseVal.EntryCount; i++)
            {
                var entryPtr = responseVal.Entries[i];
                if (entryPtr == null) continue;

                var entry = entryPtr[0];

                // Only count usable memory (type 0 = Usable)
                if (entry.Type == Cosmos.Kernel.Boot.Limine.LimineMemmapType.Usable)
                {
                    totalBytes += entry.Length;
                }
            }

            return totalBytes / (1024 * 1024);
        }
    }
}