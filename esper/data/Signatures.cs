﻿namespace esper.data {
    public static class Signatures {
        public static readonly Signature None = new Signature(0);
        public static readonly Signature GRUP = Signature.FromString("GRUP");
        public static readonly Signature TES4 = Signature.FromString("TES4");
        public static readonly Signature CELL = Signature.FromString("CELL");
        public static readonly Signature QUST = Signature.FromString("QUST");
        public static readonly Signature NAVM = Signature.FromString("NAVM");
        public static readonly Signature XXXX = Signature.FromString("XXXX");
        public static readonly Signature NPC_ = Signature.FromString("NPC_");
        public static readonly Signature FACT = Signature.FromString("FACT");
    }
}
