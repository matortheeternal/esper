namespace esper.conflicts {
    public enum CellConflictStatus {
        Unknown,
        Ignored,
        NotDefined,
        IdenticalToMaster,
        OnlyOne,
        HiddenByModGroup,
        Master,
        ConflictBenign,
        Override,
        IdenticalToMasterWinsConflict,
        ConflictWins,
        ConflictLoses
    }
}
