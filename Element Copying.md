# Element Copying

| Source             | Target       | XEditLib                               | esper                                 |
| ------------------ | ------------ | -------------------------------------- | ------------------------------------- |
| RootElement        | *            | No                                     | No                                    |
| *                  | RootElement  | No                                     | No                                    |
| PluginFile         | *            | No                                     | No                                    |
| GroupRecord        | PluginFile   | Yes                                    | Yes                                   |
| GroupRecord        | GroupRecord  | No                                     | No                                    |
| GroupRecord        | MainRecord   | No                                     | No                                    |
| MainRecord         | PluginFile   | Yes                                    | Yes                                   |
| MainRecord         | GroupRecord  | <span style="color: lime">No</span>    | <span style="color: lime">Yes</span>  |
| MainRecord         | MainRecord   | No                                     | No                                    |
| Member/Field       | PluginFile   | <span style="color: salmon">Yes</span> | <span style="color: salmon">No</span> |
| Member/Field       | GroupRecord  | No                                     | No                                    |
| Member/Field       | MainRecord   | Yes                                    | Yes                                   |
| Array Member/Field | ArrayElement | Yes                                    | Yes                                   |

*Everything else is not valid.*

