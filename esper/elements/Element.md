# Element

`Element` is the base node of Esper's parsed plugin object graph. Every
thing in a plugin file — the file itself, groups, records, structs,
values — is an `Element`. The class owns identity, placement, and
mutation state; everything about shape, parsing, and serialization lives
in its paired `ElementDef`.

> Scope: this page describes `Element.cs` as implemented. Behaviors that
> properly belong to subclasses (array indexing, group/record resolution)
> are mentioned only where the base class touches them; their xEdit
> correspondence is established in their own class pages, still to come.
> Anything below marked pending cross-check is a known unverified claim,
> not a finding.

## Placement in the tree

An element knows its `container`, and construction auto-attaches: `new
Element(container, def)` appends itself to `container.internalElements`
(null container = detached, the explicit opt-out used when the caller
attaches manually). Navigation upward delegates through the chain:
`file`, `group`, `record` each walk `container` until the matching kind
is found; `subrecord` returns self when the def is one, else delegates.
`path` / `fullPath` compose from `pathKey` segments (`[index]` inside
arrays, signature for subrecords, name otherwise — the `[index]` shape is
pending cross-check against xEdit's container path handling, which may not
have an index-spelling counterpart); `index` is the
position in the container. There is deliberately no child list on the
base class — leaf access to `elements` throws "not a container". Only
`Container` subclasses hold children.

## Def-driven surface

`name`, `signature`, `displayName`, `size`, `sortKey`, and `manager` all
delegate to `def`. The split is strict: the element is identity plus
position, the def is meaning plus codec. `SupportsSignature`,
`HasSubrecord`, and `referencedRecord` are virtual capability gates —
the base throws "does not …", and only capable subclasses override them.
`game`, `sessionOptions`, and `manager` resolve through `file`, so any
element can reach session context without carrying it.

## Lifecycle

Construction attaches; `Initialize()` runs the post-parse hook (defs
call it after `NewElement`); `ElementsReady()` fires once a container's
children are complete. `Remove()` refuses required elements and
delegates to the parent container. `sourceDataSize` records the sized
source a struct was read from, for union deciders that branch on
subrecord data size.

## Mutation state

`ElementState` is a small flag set (`None`, `Modified`, `Protected`,
`FilterShow`) with bitwise `SetState` / `ClearState`.
`MarkModified` sets `Modified` on self, cascades down through children,
and walks up to the root — dirty tracking propagates in both
directions from the edit point. Protected-state checks (editability)
read these flags.

## Persistence

Record bodies are lazy: the read path stores header plus body offset
and skips the stream, parsing only on first `elements` access
(`readAllSubrecords` opts into eager parsing, default off). The byte
source is a read-only memory-mapped view (`PluginFileSource`), so
untouched records are shells over OS-paged bytes. Writing goes through
`def.WriteElement` via `WriteTo`. `ToJson` exists as an internal hook
for future serialization; the base throws.

## Copy machinery

`CopyTo` routes by target kind (record, array, else reject) into
`AssignCopy`, which pairs `ForceContainers` (build the target's
container chain, resolving or creating each level) with `ResolveIn` /
`CopyInto` (find the existing element or construct it). `CopyOptions`
steers the behavior (`AsNewRecord`, `CopyChildGroups`, `CopyMaster`,
override variants). The base implementations throw; each element kind
overrides the pieces it supports.

## Differences from xEdit's element

(Detail lives in the oracle doc at `tes5edit/docs/Element.md`.)

- **Lifetime.** Strong container reference plus GC instead of manual
  refcounting. No cycles to break, no pointer-stability guarantees.
- **No view layer on the class.** Conflict levels, filters, tags, and
  error reporting are not element state. Conflict data is being moved
  toward cached-on-element with generation invalidation; errors stay
  deferred by design.
- **Laziness granularity.** Same memory-mapped shape as xEdit, but
  parsing happens per record body on first touch rather than per value
  on demand. Managed cost tracks touched records.
- **No identity ownership yet.** There is no `ReferenceFile` counterpart;
  resolving one needs FormID master-slot decoding.
- **No member-switching API on elements.** Union reinterpretation goes
  through `MemberUnionElement` and resolution, not `NextMember`-style
  methods.
- **`CompareExchangeFormID` is planned**, probably renamed, probably on
  the element hierarchy. Not yet present.
