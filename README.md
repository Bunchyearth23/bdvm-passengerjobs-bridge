# BDVM - PassengerJobs Bridge

`BDVM.PassengerJobsBridge` is the optional, fail-closed adapter between the independent `BDVM.Passengers` economy and Passenger Jobs for Derail Valley.

## Status

| Property | Value |
| --- | --- |
| Module kind | Optional runtime bridge |
| Target framework | .NET Framework 4.8 (`net48`) |
| Required BDVM modules | `BDVM.Common`, `BDVM.Passengers` |
| Direct runtime dependency | `PassengerJobs` 5.2.x or 5.3.x |
| Transitive runtime dependency | `DVLangHelper`, required by Passenger Jobs |
| Standalone | No |

`BDVM.Passengers` remains independent and testable without Passenger Jobs. The current in-game passenger workflow requires this bridge and a compatible Passenger Jobs installation because BDVM observes existing passenger missions instead of creating a second job system.

## Responsibilities

- Detect whether Passenger Jobs is loaded and exposes the expected integration surface.
- Accept Passenger Jobs versions from 5.2.0 inclusive to 6.0.0 exclusive.
- Refuse missing, malformed or unsupported versions with an explicit status code.
- Provide the boundary for observing Passenger Jobs identity, lifecycle and vanilla settlement.
- Keep passenger demand, capacity, punctuality and company accounting inside `BDVM.Passengers`.
- Prevent BDVM from presenting manual placeholder data as a verified Passenger Jobs mission.

## Current implementation

`PassengerJobsBridgeProbe` validates the Unity Mod Manager version and the required runtime types without linking against or copying Passenger Jobs code. `PassengerJobsBridgeModule` declares the BDVM dependency graph. Concrete job-event observation remains a runtime adapter task and must stay fail-closed until its host/client behavior is validated.

## Boundaries

The bridge does not generate passenger jobs, spawn passenger cars, award money, buy or refund licenses, or become an economic authority. Passenger Jobs continues to own its job lifecycle and vanilla payout. BDVM observes that result and applies its own idempotent accounting exactly once.

## Build

Place Common and Passengers beside this repository under `src/`, then run:

```powershell
dotnet build .\BDVM.PassengerJobsBridge.csproj -c Release
```

Passenger Jobs is a runtime dependency and is therefore not needed to build the probe contract.

## Testing and installation

Unit validation covers missing, compatible and incompatible runtime surfaces. This module is not an independent Unity Mod Manager mod. Install it only through a compatible BDVM composition together with Passenger Jobs and its required DVLangHelper dependency.

The installed development baseline used for the dependency audit was Passenger Jobs 5.3.0. Runtime host/client completion, cancellation, reload and payout reconciliation remain required before declaring the adapter production-ready.

## Upstream and provenance

- Original repository: [katycat5e/DVPassengerJobs](https://github.com/katycat5e/DVPassengerJobs).
- Recorded audit revision: `9bb668cbc2f3d270d282b2b3297667f01bec3e18`.
- Author: Katy Fox / `Katycat`.
- Upstream license: MIT.
- Code copied into this repository: none.

Passenger Jobs is not bundled. If upstream code is incorporated later, its copyright and MIT notice must be preserved.

## Compatibility

The probe currently accepts the Passenger Jobs 5.x integration line from 5.2.0 onward. Version acceptance alone is insufficient: required runtime types must also exist. Unknown major versions are refused until audited. The assembly version is not used because the distributed DLL reports `1.0.0.0`; the Unity Mod Manager manifest version is authoritative.

## License

BDVM bridge code is licensed under the Apache License, Version 2.0. See [LICENSE](LICENSE). Passenger Jobs remains under its upstream MIT license.
