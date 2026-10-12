# Agent-driven PR workflow

How AI agents land code changes on this repo so a human can review them
with the normal GitHub PR workflow.

## One-time setup (human)

1. Install the GitHub CLI (already done on this machine: `~/.local/bin/gh`).
2. Authenticate once — this step is interactive and must be done by a human:
   `gh auth login`
3. Make sure the long-lived dev branch exists on the remote (agents create
   topic branches off it, and PRs target it):
   `git push -u origin update`

## Branch conventions

- Long-lived dev branch: `update`. All work branches off it; all PRs target it.
- Agent topic branches: `agent/<short-topic>`, e.g. `agent/decider-null-convention`.
  One insight/fix per branch — never bundle unrelated changes.
- Agents never push directly to `update` or `master`, and never merge their
  own PRs. A human merges after review.

## Agent loop: implement → PR → review → land

All commands run from the repo root (`/home/colin/git/esper`).

### 1. Start a topic branch (agent)

```bash
git checkout update
git pull --ff-only
git checkout -b agent/<short-topic>
```

### 2. Implement with proof (agent)

- Follow the repo's standing rules: smallest complete change, real-fixture
  tests preferred, full suite green before pushing.
- Commit in small, reviewable steps on the topic branch.

### 3. Open the PR (agent)

```bash
git push -u origin agent/<short-topic>
gh pr create --base update \
  --title "<concise summary>" \
  --body "$(cat <<'EOF'
## Summary
...
## Verification
- dotnet build esper.sln — 6/6, 0 warnings
- dotnet test Tests/Tests.csproj — passed: ___, skipped: ___, failed: 0
EOF
)"
```

The repo PR template (`.github/pull_request_template.md`) fills in
automatically when `--body` is omitted; passing an explicit body overrides
it, so match its sections.

### 4. Human review

The reviewer reads code on github.com or locally:

```bash
gh pr view <number> --comments   # PR description + review comments
gh pr diff <number>              # full diff without switching branches
gh pr checkout <number>          # check out the PR branch to test locally
```

### 5. Address feedback (agent)

Read review comments and reply in place:

```bash
gh pr view <number> --comments
gh api repos/{owner}/{repo}/pulls/<number>/comments   # inline code comments
gh pr comment <number> --body "Done in <sha>: <what changed>"
```

Push follow-up commits to the same branch (always new commits on shared
branches — no force-push). Then:

```bash
gh pr ready <number>   # if the PR was drafted while iterating
```

### 6. Land (human)

```bash
gh pr checks <number>   # confirm green (if CI is configured)
gh pr merge <number> --squash --delete-branch
```

## Cheat sheet

| Need                         | Command                                          |
|------------------------------|--------------------------------------------------|
| PR status overview           | `gh pr status`                                   |
| Watch your PR's comments     | `gh pr view <n> --comments`                      |
| Diff without checkout        | `gh pr diff <n>`                                 |
| Test a PR locally            | `gh pr checkout <n>`                             |
| Reply on a PR                | `gh pr comment <n> --body "..."`                 |
| Mark ready / draft           | `gh pr ready <n>` / `gh pr create --draft ...`   |
| List review threads          | `gh api repos/{owner}/{repo}/pulls/<n>/comments` |

## Guardrails for agents

- One topic per branch/PR. If review uncovers a second issue, spin up a new
  `agent/<topic>` branch instead of piling onto the open PR.
- Never `gh pr merge` your own PR, never `--force` push, never commit
  directly to `update`/`master`.
- Keep the fixture policy from the PR template: real game data is
  authoritative; synthetic test binaries are throwaway oracles, never
  committed, never treated as ground truth.
