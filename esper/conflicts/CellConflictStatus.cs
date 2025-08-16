namespace esper.conflicts {
    [JSExport]
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
