namespace AgentFramework.Kernel.Diagnostics;

/// <summary>
/// Provides a read-only view of the kernel’s current runtime state
/// for introspection, metrics collection, or observability dashboards.
/// </summary>
public interface IKernelInspector
{
    /// <summary>Creates a consistent snapshot of the current kernel state.</summary>
    KernelSnapshot GetSnapshot();
}
