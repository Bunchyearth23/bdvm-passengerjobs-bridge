using System;
using PassengerJobs.API;

namespace BDVM.PassengerJobsBridge;

public sealed class PassengerJobsGenerationControl
{
    private readonly IPassengerJobsApiV2 api;

    private PassengerJobsGenerationControl(IPassengerJobsApiV2 api) => this.api = api;

    public bool IsAvailable => api.CanControlAutomaticGeneration;
    public bool IsSuspended => api.IsAutomaticGenerationSuspended;

    public bool TrySet(string operationId, bool suspended) =>
        IsAvailable && !string.IsNullOrWhiteSpace(operationId) && api.SetAutomaticGenerationSuspended(operationId, suspended) && api.IsAutomaticGenerationSuspended == suspended;

    public static bool TryCreate(out PassengerJobsGenerationControl? control, out string resultCode)
    {
        control = null;
        if (!(PassengerJobsApi.Current is IPassengerJobsApiV2 api)) { resultCode = "passengerjobs-generation-api-unavailable"; return false; }
        var candidate = new PassengerJobsGenerationControl(api);
        if (!candidate.IsAvailable) { resultCode = "passengerjobs-generation-control-not-authoritative"; return false; }
        control = candidate; resultCode = "passengerjobs-generation-control-ready"; return true;
    }
}
