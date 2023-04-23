using VSTSDataProvider.Common;

namespace VSTSDataProvider.Models;

public enum OutcomeState
{
    [StringValue("failed")]
    Failed,//failed

    [StringValue("unspecified")]
    Active,//unspecified

    [StringValue("passed")]
    Passed,//passed
}

public enum TestTools
{

    [StringValue]
    SilkTest,

    [StringValue]
    Silk4Net,

    [StringValue]
    UFT,
}

//还待完善,暂定
public enum ProductAreas
{
    AAM,
    AcidGas,
    AI,
    AOT,
    Aprop,
    BLOWDOWN,
    Dyn,
    EA,
    EmbeddedAI,
    EO,
    FlareNet,
    GUI,
    HYSYS,
    OLI,
    PSV,
    RefSYS,
    Samples,
    SS,
    Sulsim,
    UnitOps,
    Upstream,
}