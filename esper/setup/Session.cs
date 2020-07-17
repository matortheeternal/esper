using System.Text;

namespace esper.setup {
    public class Session {
        public Game game;
        public SessionOptions options;
        public DefinitionManager definitionManager;
        public PluginManager pluginManager;
        public string dataPath;

        public Session(Game game, SessionOptions options) {
            this.game = game;
            this.options = options;
            definitionManager = new DefinitionManager(game, this);
            pluginManager = new PluginManager(game, this);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
