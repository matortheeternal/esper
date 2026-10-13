namespace esper.defs.FO3 {
    internal static class PerkTables {
        internal static readonly string[] FunctionNames = {
            "",
            "Set Value",
            "Add Value",
            "Multiply Value",
            "Add Range To Value",
            "Add Actor Value Mult",
            "",
            "",
            "Add Leveled List",
            "Add Activate Choice"
        };

        internal static readonly int[] FunctionTypes = {
            3, 0, 0, 0, 0, 0, 3, 3, 1, 2
        };

        internal static readonly int[] FunctionParamTypes = {
            0, 1, 1, 1, 2, 2, 0, 0, 3, 4
        };

        internal static readonly string[] FunctionParamNames = {
            "None",
            "Float",
            "Float, Float",
            "Leveled Item",
            "Script"
        };

        internal static readonly int[] EntryPointConditions = {
            3, 3, 3, 2, 1, 2, 7, 2, 3, 0, 0, 0, 0, 0, 4, 5, 6, 1, 0, 0,
            0, 4, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 2, 3, 3
        };

        internal static readonly int[] EntryPointFunctionTypes = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 1, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        internal static readonly int[] ConditionCounts = {
            1, 2, 2, 3, 2, 2, 3, 3
        };

        internal static readonly string[][] ConditionCaptions = {
            new string[] { "Perk Owner" },
            new string[] { "Perk Owner", "Item" },
            new string[] { "Perk Owner", "Weapon" },
            new string[] { "Perk Owner", "Weapon", "Target" },
            new string[] { "Perk Owner", "Target" },
            new string[] { "Perk Owner", "Attacker" },
            new string[] { "Perk Owner", "Attacker", "Attackee" },
            new string[] { "Perk Owner", "Attacker", "Attacker Weapon" }
        };
    }
}
