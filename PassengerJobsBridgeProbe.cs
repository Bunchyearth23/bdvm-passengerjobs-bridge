using System;
using System.Reflection;

namespace BDVM.PassengerJobsBridge;

public enum PassengerJobsBridgeState
{
    Available,
    Missing,
    Incompatible
}

public sealed class PassengerJobsBridgeStatus
{
    public PassengerJobsBridgeState State { get; set; }
    public string ModVersion { get; set; } = "";
    public string Code { get; set; } = "";
    public bool IsAvailable => State == PassengerJobsBridgeState.Available;
}

public static class PassengerJobsBridgeProbe
{
    public static readonly Version MinimumSupportedVersion = new Version(5, 2, 0);
    public static readonly Version MaximumExclusiveVersion = new Version(6, 0, 0);

    public static PassengerJobsBridgeStatus Inspect(string? modVersion, Assembly? assembly)
    {
        if (assembly == null)
            return Status(PassengerJobsBridgeState.Missing, modVersion, "passengerjobs-not-loaded");

        return InspectSurface(modVersion, name => assembly.GetType(name, false) != null);
    }

    public static PassengerJobsBridgeStatus InspectSurface(string? modVersion, Func<string, bool> hasType)
    {
        if (hasType == null) throw new ArgumentNullException(nameof(hasType));

        if (!Version.TryParse(modVersion, out var version) || version < MinimumSupportedVersion || version >= MaximumExclusiveVersion)
            return Status(PassengerJobsBridgeState.Incompatible, modVersion, "passengerjobs-version-unsupported");

        if (!hasType("PassengerJobs.PJMain") ||
            !hasType("PassengerJobs.Generation.PassJobType") ||
            !hasType("PassengerJobs.Generation.PassengerHaulJobDefinition"))
            return Status(PassengerJobsBridgeState.Incompatible, modVersion, "passengerjobs-contract-missing");

        return Status(PassengerJobsBridgeState.Available, modVersion, "passengerjobs-compatible");
    }

    private static PassengerJobsBridgeStatus Status(PassengerJobsBridgeState state, string? version, string code)
    {
        return new PassengerJobsBridgeStatus { State = state, ModVersion = version ?? "", Code = code };
    }
}

public sealed class PassengerJobsRuntimeBridge
{
    private readonly Assembly? assembly;
    private readonly MethodInfo? isPassengerJobType;

    public PassengerJobsRuntimeBridge(string? modVersion, Assembly? assembly)
    {
        this.assembly = assembly;
        Status = PassengerJobsBridgeProbe.Inspect(modVersion, assembly);
        isPassengerJobType = Status.IsAvailable
            ? assembly!.GetType("PassengerJobs.Generation.PassJobType", false)?.GetMethod("IsPJType", BindingFlags.Public | BindingFlags.Static)
            : null;
        if (Status.IsAvailable && isPassengerJobType == null)
            Status = new PassengerJobsBridgeStatus { State = PassengerJobsBridgeState.Incompatible, ModVersion = modVersion ?? "", Code = "passengerjobs-classifier-missing" };
    }

    public PassengerJobsBridgeStatus Status { get; private set; }

    public bool IsPassengerJob(object? job)
    {
        if (!Status.IsAvailable || assembly == null || isPassengerJobType == null || job == null) return false;
        var type = job.GetType();
        var jobType = type.GetProperty("jobType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(job, null)
            ?? type.GetField("jobType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(job);
        return jobType != null && isPassengerJobType.Invoke(null, new[] { jobType }) is bool result && result;
    }
}
