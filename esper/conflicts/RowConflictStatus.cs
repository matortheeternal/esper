namespace esper.conflicts {
    [JSExport]
    public enum RowConflictStatus {
        Unknown,
        OnlyOne,
        NoConflict,
        ConflictBenign,
        Override,
        Conflict,
        ConflictCritical
    }
}
