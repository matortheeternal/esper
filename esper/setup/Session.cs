using esper.defs;
using esper.elements;
using System.Reflection;
using System.Text;

namespace esper.setup {
    [JSExport]
    public class Session {
        public Game game;
        public Logger logger;
        public SessionOptions options;
        public DefinitionManager definitionManager;
        public PluginManager pluginManager;
        public ResourceManager resourceManager;
        public string dataPath;
        public GameIni gameIni;
        internal Assembly assembly;

        public RootElement root => pluginManager.root;
        public ElementDef pluginFileDef {
            get => (ElementDef)definitionManager.ResolveDef("PluginFile");
        }

        public Session(Game game, SessionOptions options) {
            this.game = game;
            this.options = options;
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
