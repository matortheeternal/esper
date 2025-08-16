namespace esper.data {
    [JSExport]
    public static class Signatures {
        public static readonly Signature None = new Signature(0);
        public static readonly Signature GRUP = Signature.FromString("GRUP");
        public static readonly Signature TES4 = Signature.FromString("TES4");
        public static readonly Signature CELL = Signature.FromString("CELL");
        public static readonly Signature QUST = Signature.FromString("QUST");
        public static readonly Signature NAVM = Signature.FromString("NAVM");
        public static readonly Signature XXXX = Signature.FromString("XXXX");
        public static readonly Signature BOOK = Signature.FromString("BOOK");
        public static readonly Signature LSCR = Signature.FromString("LSCR");
        public static readonly Signature DESC = Signature.FromString("DESC");
        public static readonly Signature CNAM = Signature.FromString("CNAM");
        public static readonly Signature INFO = Signature.FromString("INFO");
        public static readonly Signature RNAM = Signature.FromString("RNAM");
    }
}
