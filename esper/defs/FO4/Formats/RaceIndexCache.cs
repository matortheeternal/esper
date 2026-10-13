using esper.elements;
using System.Collections.Generic;

namespace esper.defs.FO4 {
    internal class RaceIndexEntries {
        internal readonly string raceId;
        internal readonly bool female;
        internal readonly Dictionary<uint, string> entries =
            new Dictionary<uint, string>();

        internal RaceIndexEntries(string raceId, bool female) {
            this.raceId = raceId;
            this.female = female;
        }

        internal string GetEntryName(uint index) {
            return entries.TryGetValue(index, out var name) ? name : null;
        }
    }

    internal abstract class RaceIndexCache {
        private readonly List<RaceIndexEntries> cache =
            new List<RaceIndexEntries>();

        internal RaceIndexEntries Get(MainRecord race, bool? female) {
            if (race == null || female == null) return null;
            var raceId = race.editorId;
            var found = cache.Find(e =>
                e.raceId == raceId && e.female == female);
            if (found == null) {
                found = new RaceIndexEntries(raceId, (bool)female);
                LoadEntries(found, race);
                cache.Add(found);
            }
            return found;
        }

        protected abstract void LoadEntries(
            RaceIndexEntries entries, MainRecord race);
    }
}
