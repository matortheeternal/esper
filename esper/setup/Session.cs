using esper.defs;
using esper.elements;
using System.Reflection;
using System.Text;

namespace esper.setup {
    public class Session {
        public Game game;
        public Logger logger;
        public SessionOptions options;
        public DefinitionManager definitionManager;
        public PluginManager pluginManager;
        public string dataPath;
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
            definitionManager = new DefinitionManager(game, this);
            pluginManager = new PluginManager(game, this);
        }
    }
}
