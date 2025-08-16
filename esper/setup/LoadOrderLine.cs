namespace esper.setup {
    [JSExport]
    public class LoadOrderLine {
        public string text;
        public int index;
        public LoadOrderFile file;

        public bool isActive {
            get => !file.usesAsterisks || text.StartsWith("*");
        }

        public bool isComment {
            get => text.StartsWith("#");
        }

        public string moduleName {
            get => file.usesAsterisks && text.StartsWith("*") ? text[1..] : text;
        }
        
        public LoadOrderLine(LoadOrderFile file, string text, int index) {
            this.file = file;
            this.text = text;
            this.index = index;
        }
    }
}
