using esper.defs;
using esper.elements;
using esper.queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace esper.setup {
    [JSExport]
    public class Session {
        public Game game { get; }
        public Logger logger { get; }
        public SessionOptions options { get; }
        public DefinitionManager definitionManager { get; }
        public PluginManager pluginManager { get; }
        public ResourceManager resourceManager { get; }
        public string dataPath { get; set; }
        public GameIni gameIni;
        internal Assembly assembly;
        private readonly Dictionary<ulong, QuerySelection> selections = [];
        private ulong nextSelectionId = 1;

        public RootElement root => pluginManager.root;
        public ElementDef pluginFileDef {
            get => (ElementDef)definitionManager.ResolveDef("PluginFile");
        }

        public Session(Game game, SessionOptions options) {
            this.game = game;
            this.options = options;
            dataPath = string.IsNullOrWhiteSpace(options.gamePath)
                ? null
                : Path.Combine(options.gamePath, "Data");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            logger = new Logger("esper");
            assembly = Assembly.GetExecutingAssembly();
            logger.Info($"Esper v{assembly.GetName().Version}");
            logger.Info($"Starting a new session for {game.name}");
            gameIni = new GameIni(this);
            definitionManager = new DefinitionManager(this);
            pluginManager = new PluginManager(this);
            resourceManager = new ResourceManager(this);
            if (options.loadResources) resourceManager.Init();
        }

        public QuerySelection CaptureSelection(params object[] sources) {
            ArgumentNullException.ThrowIfNull(sources);
            Element[] elements = sources.SelectMany(CaptureElements).ToArray();
            ValidateSelection(elements);
            var selection = new QuerySelection(nextSelectionId, elements);
            selections.Add(nextSelectionId, selection);
            nextSelectionId++;
            return selection;
        }

        internal QuerySelection GetSelection(ulong id) {
            if (selections.TryGetValue(id, out QuerySelection selection))
                return selection;
            throw new InvalidOperationException($"Selection '{id}' does not exist.");
        }

        private static IEnumerable<Element> CaptureElements(object source) {
            if (source is TreeNode node) return node.elements;
            if (source is Element element) return [element];
            throw new ArgumentException(
                "Selections can only capture tree nodes and elements.",
                nameof(source)
            );
        }

        private void ValidateSelection(IEnumerable<Element> elements) {
            foreach (Element element in elements)
                if (element.manager.session != this)
                    throw new ArgumentException(
                        $"Element '{element.fullPath}' belongs to another session."
                    );
        }
    }
}
