// Stub for NativeAOT finalizer support.
// In a bare-metal OS environment, finalizers are not fully supported.

void RhWaitForPendingFinalizers(void)
{
    // No-op: finalizers are not run in this environment.
    // In a full NativeAOT runtime, this would wait for all pending finalizers to complete.
}