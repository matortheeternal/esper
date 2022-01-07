## Conflict Priority

- Stored in definitions, sometimes dynamic based on a callback
- When dynamic, iterate through all elements and find the highest priority

## Row Conflict Status

### Unknown

- If there are no elements

### OnlyOne

- If there is only one cell

### NoConflict

- If there is only one unique value

### ConflictBenign

- If the overall conflict priority is benign and there is any conflict (2 or more unique values)

### Override

- If there are 2 unique values and one of the following is true:
  - either the first or the last element is not assigned (exclusive)
  - the first element is assigned and its value does not match the last element's value
  - the first element has an empty value and the last element is assigned and has a non-empty value

### Conflict

- If there are two or more unique values and a conflict status of `Override` is not applied
- OR if there is any `CellConflictStatus` greater than or equal to `IdenticalToMasterWinsConflict`

### ConflictCritical

- If conflict priority is critical and there is more than 1 non-empty unique value.

## Cell Conflict Status

### Unknown

- Element is not assigned

### Ignored

- In translation mode and conflict priority is Translate
- Conflict priority of element is Ignore
- Conflict priority is NormalIgnoreEmpty and element is not assigned

### NotDefined

- All elements are not assigned

### IdenticalToMaster

- When value is same as the first cell
  - cannot be set if first element's conflict priority is ignore but overall conflict priority is greater than ignore 
- when a cell wins a conflict but row conflict status is NoConflict

### OnlyOne

- When there's only one cell

### HiddenByModGroup

- Unknown conflict level for records are set to this

### Master

- If the cell is the first cell

### ConflictBenign

- If the conflict priority is benign and the conflict this is greater than ConflictBenign

### Override

- When a cell wins a conflict and the row conflict status is override

### IdenticalToMasterWinsConflict

- On last cell only:
  - when priority is not benign, there is some kind of conflict, and cell conflict status would otherwise be IdenticalToMaster


### ConflictWins

- When not first element, ignored, or same value as first element.  If value is same as last element's value.
- On last cell only:
  - When priority is not benign, there is some kind of conflict, and cell conflict status wouldn't otherwise be IdenticalToMaster


### ConflictLoses

- When not first element, ignored, or same value as first element.  If value is not the same as last element's value.



