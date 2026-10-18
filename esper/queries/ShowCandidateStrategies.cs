#nullable enable

using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using System;
using System.Collections.Generic;
using System.Linq;

namespace esper.queries {
    internal static class ShowCandidateStrategies {
        private static readonly ShowCandidateStrategy[] strategies = [
            new RecordPredicateCandidateStrategy(),
            new RefrNameSignatureCandidateStrategy()
        ];

        internal static IEnumerable<Element> Apply(
            ShowQuery query,
            Session session,
            IEnumerable<Element> candidates
        ) {
            foreach (ShowCandidateStrategy strategy in strategies)
                if (strategy.Matches(query))
                    candidates = strategy.Apply(query, session, candidates);
            return candidates;
        }
    }

    internal abstract class ShowCandidateStrategy {
        internal abstract bool Matches(ShowQuery query);

        internal abstract IEnumerable<Element> Apply(
            ShowQuery query,
            Session session,
            IEnumerable<Element> candidates
        );

        protected static bool UsesSessionRecords(ShowQuery query) {
            return query.source is SessionSource && query.target is RecordsTarget;
        }
    }

    internal sealed class RecordPredicateCandidateStrategy : ShowCandidateStrategy {
        internal override bool Matches(ShowQuery query) {
            if (query.source is not SessionSource) return false;
            if (query.whereClause == null) return false;
            if (query.target is RecordsTarget)
                return query.whereClause.TryGetRecordFilter(out _);
            return query.whereClause.TryGetRecordScopedFilter(out _);
        }

        internal override IEnumerable<Element> Apply(
            ShowQuery query,
            Session session,
            IEnumerable<Element> candidates
        ) {
            bool scoped = query.target is not RecordsTarget;
            Func<MainRecord, bool> filter = null!;
            if (scoped) query.whereClause!.TryGetRecordScopedFilter(out filter);
            if (!scoped) query.whereClause!.TryGetRecordFilter(out filter);
            return ApplyFilter(candidates, filter);
        }

        private static IEnumerable<Element> ApplyFilter(
            IEnumerable<Element> candidates,
            Func<MainRecord, bool> filter
        ) {
            return candidates.OfType<MainRecord>().Where(filter).Cast<Element>();
        }
    }

    internal sealed class RefrNameSignatureCandidateStrategy : ShowCandidateStrategy {
        private const string nameSignature = "[@NAME].signature";

        internal override bool Matches(ShowQuery query) {
            if (!UsesSessionRecords(query)) return false;
            if (query.whereClause == null) return false;
            return query.whereClause.TryGetComparison(
                nameSignature,
                out _,
                out _,
                out _
            );
        }

        internal override IEnumerable<Element> Apply(
            ShowQuery query,
            Session session,
            IEnumerable<Element> candidates
        ) {
            query.whereClause!.TryGetComparison(
                nameSignature,
                out QueryComparison comparison,
                out object? value,
                out bool propertyFirst
            );
            Dictionary<PluginFile, HashSet<uint>> matches = BuildMatches(
                session,
                comparison,
                value,
                propertyFirst
            );
            return candidates.OfType<MainRecord>().Where(record =>
                matches.TryGetValue(record.file, out HashSet<uint>? formIds)
                && formIds.Contains(record.fileFormId)
            ).Cast<Element>();
        }

        private static Dictionary<PluginFile, HashSet<uint>> BuildMatches(
            Session session,
            QueryComparison comparison,
            object? value,
            bool propertyFirst
        ) {
            var matches = new Dictionary<PluginFile, HashSet<uint>>();
            foreach (PluginFile file in session.pluginManager.plugins) {
                Dictionary<uint, string> index = BuildIndex(file);
                HashSet<uint> formIds = index.Where(pair => Matches(
                    comparison,
                    pair.Value,
                    value,
                    propertyFirst
                )).Select(pair => pair.Key).ToHashSet();
                matches.Add(file, formIds);
            }
            return matches;
        }

        private static Dictionary<uint, string> BuildIndex(PluginFile file) {
            if (file.modified)
                return QueryIndex.Build(file, "REFR", ResolveNameSignature);
            return QueryIndex.LoadOrBuild(
                file,
                "REFR",
                nameSignature,
                ResolveNameSignature
            );
        }

        private static bool Matches(
            QueryComparison comparison,
            string indexedValue,
            object? value,
            bool propertyFirst
        ) {
            if (propertyFirst) return comparison.Matches(indexedValue, value);
            return comparison.Matches(value, indexedValue);
        }

        private static string ResolveNameSignature(MainRecord record) {
            Element? referenced = record.ResolveElement("@NAME");
            return referenced is MainRecord target ? target.signature.ToString() : "";
        }
    }
}
