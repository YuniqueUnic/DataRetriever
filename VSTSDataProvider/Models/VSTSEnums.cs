using VSTSDataProvider.Common;

namespace VSTSDataProvider.Models;

public enum OutcomeState : int
{
    [StringValue("")]
    Null,

    [StringValue("failed")]
    Failed,//failed


    [StringValue("unspecified")]
    Active,//unspecified

    [StringValue("active")]
    Active_,

    [StringValue("passed")]
    Passed,//passed
}

public enum TestTools : int
{
    [StringValue("")]
    Null,

    [StringValue]
    SilkTest,

    [StringValue]
    Silk4Net,

    [StringValue("UFT Developer")]
    UFT,
}

//Obsolete
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