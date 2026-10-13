using esper;
using esper.defs;
using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Tests {
    // Shared synthetic-element builders. Sessions are cached per game
    // abbreviation because Session construction eagerly parses all JSON
    // definitions and builds every def (~1s+); previously each NewStruct
    // call paid that cost. Cached sessions are never used to load plugins,
    // so no test state leaks through them.
    internal static class Synthetic {
        private static readonly Dictionary<string, Session> sessions =
            new Dictionary<string, Session>();

        public static Session GetSession(Game game) {
            lock (sessions) {
                if (!sessions.TryGetValue(game.abbreviation, out var session)) {
                    session = new Session(game, new SessionOptions());
                    sessions[game.abbreviation] = session;
                }
                return session;
            }
        }

        private class TestStructElement : StructElement {
            private readonly DefinitionManager definitionManager;

            public override DefinitionManager manager => definitionManager;

            public TestStructElement(
                StructDef def, DefinitionManager definitionManager
            ) : base(null, def) {
                this.definitionManager = definitionManager;
            }
        }

        public static StructElement NewStruct(Game game, string json) {
            var manager = GetSession(game).definitionManager;
            var def = new StructDef(manager, JObject.Parse(json));
            var element = new TestStructElement(def, manager);
            element.Initialize();
            return element;
        }
    }
}
