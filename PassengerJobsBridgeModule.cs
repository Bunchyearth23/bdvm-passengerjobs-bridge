using BDVM.Common;

namespace BDVM.PassengerJobsBridge;

public sealed class PassengerJobsBridgeModule : BdvmModuleBase
{
    public PassengerJobsBridgeModule()
        : base("BDVM.PassengerJobsBridge", "BDVM.Common", "BDVM.Passengers")
    {
    }
}
