using esper.plugins;
using esper.setup;
using System;

namespace esper.elements {
    public class RootElement : Container {
        public readonly Session session;

        public override DefinitionManager manager => session.definitionManager;
        public override string sortKey => null;
        public override string name => "Root";
        public override string displayName => name;
        public override string signature => null;
        public override string path => null;
        public override string fullPath => null;
        public override UInt32 size => 0;
        public override int index => 0;

        public override PluginFile file => null;
        public override GroupRecord group => null;
        public override MainRecord record => null;
        public override Element subrecord => null;

        public RootElement(Session session) : base(null, null) {
            this.session = session;
        }

        internal override Element CreateElementByName(string name) {
            return session.pluginManager.NewFile(name);
        }

        internal MainRecord GetRecordByGlobalFormId(UInt32 globalFormId) {
            foreach (PluginFile file in internalElements) {
                if (file == null) continue;
                var ordinal = file.pluginSlot?.GetOrdinal() ?? 0;
                if ((ordinal & globalFormId) == 0) continue;
                var localFormId = globalFormId ^ ordinal;
                return file?.GetRecordByLocalFormId(localFormId);
            }
            return null;
        }

        internal override Element CreateElementBySignature(string name) {
            throw new NotImplementedException();
        }
    }
}
