using System;

namespace esper.setup {
    [Flags]
    public enum ModuleFlags {
        Invalid = 1,
        Valid = 2,
        Ghost = 4,
        MastersMissing = 8,
        HasESMFlag = 16,
        HasESLFlag = 32,
        HasLocalizedFlag = 64,
        HasESMExtension = 128,
        IsESM = 256,
        ActiveInPluginsTxt = 512,
        Active = 1024,
        HasIndex = 2048,
        HasFile = 4096,
        IsHardcoded = 8192,
        IsGameMaster = 16384,
        MastersMissingMasters = 32768
    }
}
