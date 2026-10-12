using esper.defs;
using esper.elements;
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
    }
}
