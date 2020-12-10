# Element Resolution and Creation

## Resolution

| Container       | Name                                     | XEditLib.dll | esper  |
| --------------- | ---------------------------------------- | ------------ | ------ |
| Root            | File by filename                         | yes          | yes    |
| Root            | File by index                            | yes          | yes    |
| Root            | Record by global form ID                 | no           | yes    |
| File            | Top group by signature                   | yes          | yes    |
| File            | Top group by name                        | yes          | yes    |
| File            | Element by index                         | yes          | yes    |
| File            | File header by name                      | yes          | yes    |
| File            | Record by file form ID                   | yes          | yes    |
| File            | Record by global form ID                 | yes          | yes    |
| **File**        | **Record by full name**                  | **yes**      | **no** |
| File            | File header by signature                 | no           | yes    |
| **File**        | **Record by editor ID**                  | **yes**      | **no** |
| Group           | Element by index                         | yes          | yes    |
| **Group**       | **Record by local form ID**              | **yes**      | **no** |
| Group           | Record by file form ID                   | yes          | yes    |
| Group           | Record by global form ID                 | yes          | yes    |
| Group           | Record by full name                      | yes          | yes    |
| Group           | Record by editor ID                      | yes          | yes    |
| Group           | Group by name                            | yes          | yes    |
| Main Record     | Child group                              | yes          | yes    |
| Record elements | Element by name                          | yes          | yes    |
| Record elements | Element by index                         | yes          | yes    |
| Record elements | Element by signature                     | yes          | yes    |
| Record elements | Container                                | yes          | yes    |
| Record elements | Reference                                | no           | yes    |
| *               | Parent file, group, record, or subrecord | no           | yes    |
| *               | Container                                | no           | yes    |
| *               | Element by value                         | no           | yes    |

### Todo

- ResolveChildGroup
- ResolveByGlobalFormId
- ResolveByFileFormId
- ResolveByValue

## Creation

| Container       | Name                              | XEditLib.dll | esper |
| --------------- | --------------------------------- | ------------ | ----- |
| Root            | File by filename                  | yes          | yes   |
| File            | Top group by signature            | yes          | yes   |
| File            | Top group by name                 | yes          | yes   |
| File            | Override record by global form ID | yes          | yes   |
| File            | Override record by file form ID   | no           | yes   |
| Group           | Record by signature               | yes          | yes   |
| Group           | Record by name                    | yes          | yes   |
| Group           | Record by default                 | yes          | yes   |
| Group           | Record by full name               | no           | yes   |
| Group           | Group by name                     | yes          | yes   |
| Main Record     | Child group                       | yes          | yes   |
| Record elements | Element by name                   | yes          | yes   |
| Record elements | Element by signature              | yes          | yes   |
| Array           | Element by default                | yes          | yes   |

