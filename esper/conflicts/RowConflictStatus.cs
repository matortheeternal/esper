namespace esper.conflicts {
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
