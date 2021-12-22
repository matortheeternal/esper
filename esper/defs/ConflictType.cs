namespace esper.defs {
    public enum ConflictType : byte {
        ctIgnore,
        ctBenignIfAdded,
        ctBenign,
        ctOverride,
        ctTranslate,
        ctNormal,
        ctNormalIgnoreEmpty,
        ctCritical,
        ctFormID
    }
}
