# BDVM - PassengerJobs Bridge

`BDVM.PassengerJobsBridge` is the optional, fail-closed adapter between the independent `BDVM.Passengers` economy and Passenger Jobs for Derail Valley.

## Status

| Property | Value |
| --- | --- |
| Module kind | Optional runtime bridge |
| Target framework | .NET Framework 4.8 (`net48`) |
| Required BDVM modules | `BDVM.Common`, `BDVM.Passengers` |
| Direct runtime dependency | BDVM fork of `PassengerJobs` 5.2.x/5.3.x plus `PassengerJobs.API` 1.x |
| Transitive runtime dependency | `DVLangHelper`, required by Passenger Jobs |
| Standalone | No |

`BDVM.Passengers` remains independent and testable without Passenger Jobs. The current in-game passenger workflow requires this bridge and a compatible Passenger Jobs installation because BDVM observes existing passenger missions instead of creating a second job system.

## Responsibilities

- Detect whether Passenger Jobs is loaded and has registered the versioned `PassengerJobs.API` surface.
- Use API 1.1 to suspend automatic passenger-job generation when strict BDVM population control owns rolling-stock supply.
- Accept Passenger Jobs versions from 5.2.0 inclusive to 6.0.0 exclusive.
- Refuse missing, malformed or unsupported versions with an explicit status code.
- Provide the boundary for observing Passenger Jobs identity, lifecycle and vanilla settlement.
- Keep passenger demand, capacity, punctuality and company accounting inside `BDVM.Passengers`.
- Prevent BDVM from presenting manual placeholder data as a verified Passenger Jobs mission.

## Current implementation

`PassengerJobsBridgeProbe` validates the Unity Mod Manager version and `PassengerJobs.API` major version. `PassengerJobsRuntimeBridge` resolves passenger identity through `IPassengerJobsApiV1` instead of reflecting over PassengerJobs implementation types. `PassengerJobsGenerationControl` consumes the optional 1.1 control surface and fails closed when it is absent or non-authoritative. The API exposes lookup and lifecycle observations for available, taken, completed and abandoned jobs; runtime host/client settlement still remains fail-closed until validated.

## Boundaries

The bridge does not generate passenger jobs, spawn passenger cars, award money, buy or refund licenses, or become an economic authority. Passenger Jobs continues to own its job lifecycle and vanilla payout. BDVM observes that result and applies its own idempotent accounting exactly once.

## Build

Place Common and Passengers beside this repository under `src/`, then run:

```powershell
dotnet build .\BDVM.PassengerJobsBridge.csproj -c Release
```

Build `PassengerJobs.API.dll` from the BDVM integration fork first, or set the `PassengerJobsApiPath` MSBuild property to an equivalent API 1.x contract assembly.

## Testing and installation

Unit validation covers missing, compatible and incompatible runtime surfaces. This module is not an independent Unity Mod Manager mod. Install it only through a compatible BDVM composition together with Passenger Jobs and its required DVLangHelper dependency.

The installed development baseline used for the dependency audit was Passenger Jobs 5.3.0. The fork build is staged but not installed automatically. Runtime host/client completion, cancellation, reload and payout reconciliation remain required before declaring the adapter production-ready.

## Upstream and provenance

- Original repository: [katycat5e/DVPassengerJobs](https://github.com/katycat5e/DVPassengerJobs).
- BDVM integration fork: [Bunchyearth23/DVPassengerJobs](https://github.com/Bunchyearth23/DVPassengerJobs), branch `bdvm-integration`.
- Recorded audit revision: `9bb668cbc2f3d270d282b2b3297667f01bec3e18`.
- Author: Katy Fox / `Katycat`.
- Upstream license: MIT.
- Code copied into this repository: none; the bridge references only the separately built MIT-licensed API contract assembly.

Passenger Jobs and `PassengerJobs.API.dll` are supplied by the separate fork package, not by this Apache-2.0 bridge. The fork preserves Katy Fox's copyright and MIT notice.

## Compatibility

The probe currently accepts the Passenger Jobs 5.x integration line from 5.2.0 onward and API major version 1. Version acceptance alone is insufficient: PassengerJobs must register the API implementation at load. Missing API registrations and unknown major versions are refused. The Unity Mod Manager manifest version remains authoritative for the mod version.

## License

BDVM bridge code is licensed under the Apache License, Version 2.0. See [LICENSE](LICENSE). Passenger Jobs remains under its upstream MIT license.
