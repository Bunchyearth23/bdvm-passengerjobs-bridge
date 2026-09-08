using System;
using System.Reflection;
using PassengerJobs.API;

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

        return InspectApi(modVersion, PassengerJobsApi.Current);
    }

    public static PassengerJobsBridgeStatus InspectApi(string? modVersion, IPassengerJobsApiV1? api)
    {
        if (!Version.TryParse(modVersion, out var version) || version < MinimumSupportedVersion || version >= MaximumExclusiveVersion)
            return Status(PassengerJobsBridgeState.Incompatible, modVersion, "passengerjobs-version-unsupported");
        if (api == null) return Status(PassengerJobsBridgeState.Incompatible, modVersion, "passengerjobs-api-missing");
        if (!Version.TryParse(api.ApiVersion, out var apiVersion) || apiVersion.Major != 1)
            return Status(PassengerJobsBridgeState.Incompatible, modVersion, "passengerjobs-api-version-unsupported");
        return Status(PassengerJobsBridgeState.Available, modVersion, "passengerjobs-api-compatible");
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
    private readonly IPassengerJobsApiV1? api;

    public PassengerJobsRuntimeBridge(string? modVersion, Assembly? assembly)
    {
        api = PassengerJobsApi.Current;
        Status = PassengerJobsBridgeProbe.Inspect(modVersion, assembly);
    }

    public PassengerJobsBridgeStatus Status { get; private set; }

    public bool IsPassengerJob(object? job)
    {
        if (!Status.IsAvailable || api == null || job == null) return false;
        var type = job.GetType();
        var jobId = type.GetProperty("ID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(job, null) as string
            ?? type.GetField("ID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(job) as string;
        return !string.IsNullOrWhiteSpace(jobId) && api.TryGetJob(jobId!, out _);
    }


    public bool TryGetJob(string jobId, out PassengerJobSnapshot snapshot)
    {
        snapshot = new PassengerJobSnapshot();
        return Status.IsAvailable && api != null && api.TryGetJob(jobId, out snapshot);
    }
}
