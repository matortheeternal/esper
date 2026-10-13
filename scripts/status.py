#!/usr/bin/env python3
"""Generates .ai/status.json and .ai/status.html from curated notes plus
the latest coverage report and test results.

Usage: scripts/status.py
Reads: .ai/status-manual.json, newest coverage.cobertura.xml, newest *.trx
Writes: .ai/status.json, .ai/status.html
"""
import glob
import json
import os
import subprocess
import sys
import xml.etree.ElementTree as ET
from datetime import datetime, timezone

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
AI = os.path.join(ROOT, ".ai")
SCRIPTS = os.path.join(ROOT, "scripts")


def newest(pattern):
    files = glob.glob(os.path.join(ROOT, pattern), recursive=True)
    if not files:
        return None
    return max(files, key=os.path.getmtime)


def load_coverage():
    path = newest(os.path.join("Tests", "TestResults", "*", "coverage.cobertura.xml"))
    if path is None:
        return None
    tree = ET.parse(path)
    root = tree.getroot()
    classes = []
    for cls in root.iter("class"):
        lines = cls.findall("./lines/line")
        total = len(lines)
        if total == 0:
            continue
        covered = sum(1 for ln in lines if int(ln.get("hits", "0")) > 0)
        classes.append({
            "name": f"{cls.get('filename', '')}::{cls.get('name', '')}",
            "lineRate": round(covered / total, 4),
            "covered": covered,
            "total": total,
        })
    classes.sort(key=lambda c: c["lineRate"])
    return {
        "lineRate": float(root.get("line-rate", 0)),
        "branchRate": float(root.get("branch-rate", 0)),
        "linesCovered": int(root.get("lines-covered", 0)),
        "linesValid": int(root.get("lines-valid", 0)),
        "source": os.path.relpath(path, ROOT),
        "lowestClasses": classes[:10],
    }


def load_tests():
    path = newest(os.path.join("Tests", "TestResults", "*.trx"))
    if path is None:
        return None
    ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
    tree = ET.parse(path)
    root = tree.getroot()
    summary = root.find("t:ResultSummary/t:Counters", ns)
    if summary is None:
        return None
    return {
        "total": int(summary.get("total", 0)),
        "passed": int(summary.get("passed", 0)),
        "failed": int(summary.get("failed", 0)),
        "source": os.path.relpath(path, ROOT),
    }


def head_commit():
    try:
        out = subprocess.run(
            ["git", "rev-parse", "--short", "HEAD"],
            cwd=ROOT, capture_output=True, text=True, check=True,
        )
        return out.stdout.strip()
    except Exception:
        return None


HTML_TEMPLATE = """<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<title>Esper project status</title>
<style>
body { font-family: sans-serif; max-width: 900px; margin: 2em auto; padding: 0 1em; }
table { border-collapse: collapse; width: 100%; }
th, td { border: 1px solid #ccc; padding: 4px 8px; text-align: left; }
.pill { border-radius: 8px; padding: 1px 8px; font-size: 0.85em; }
.done { background: #cfc; } .open { background: #fec; }
.ongoing { background: #ccf; } .blocked { background: #fcc; }
.working { background: #cfc; } .merged { background: #cfc; }
</style>
</head>
<body>
<h1>Esper project status</h1>
<p id="meta"></p>
<h2>Coverage</h2>
<div id="coverage"></div>
<h2>Tests</h2>
<div id="tests"></div>
<h2>Todos</h2>
<div id="todos"></div>
<h2>Missing definitions per game</h2>
<div id="missing"></div>
<h2>JS interop</h2>
<div id="js"></div>
<h2>Pull requests</h2>
<div id="prs"></div>
<script id="status-data" type="application/json">
__STATUS_JSON__
</script>
<script>
(function () {
  function render(data) {
    document.getElementById("meta").textContent =
      "Generated " + data.generated_at + " at commit " + (data.commit || "?");
    var c = data.coverage;
    document.getElementById("coverage").innerHTML = c
      ? "<p>Line " + (c.lineRate * 100).toFixed(2) + "%, branch " +
        (c.branchRate * 100).toFixed(2) + "% (" + c.linesCovered + "/" +
        c.linesValid + " lines, from " + c.source + ")</p>" +
        "<table><tr><th>Lowest class</th><th>Rate</th><th>Covered</th></tr>" +
        c.lowestClasses.map(function (k) {
          return "<tr><td>" + k.name + "</td><td>" +
            (k.lineRate * 100).toFixed(1) + "%</td><td>" +
            k.covered + "/" + k.total + "</td></tr>";
        }).join("") + "</table>"
      : "<p>No coverage report found. Run scripts/coverage.sh.</p>";
    var t = data.tests;
    document.getElementById("tests").innerHTML = t
      ? "<p>" + t.passed + " passed, " + t.failed + " failed of " + t.total +
        " (from " + t.source + ")</p>"
      : "<p>No test results found.</p>";
    document.getElementById("todos").innerHTML =
      "<table><tr><th>Todo</th><th>Status</th><th>Detail</th></tr>" +
      data.todos.map(function (td) {
        return "<tr><td>" + td.title + "</td><td><span class='pill " +
          td.status + "'>" + td.status + "</span></td><td>" + td.detail +
          "</td></tr>";
      }).join("") + "</table>";
    document.getElementById("missing").innerHTML =
      "<table><tr><th>Game</th><th>Missing</th></tr>" +
      Object.keys(data.missingDefinitions).map(function (g) {
        var items = data.missingDefinitions[g];
        return "<tr><td>" + g + "</td><td>" +
          (items.length ? items.join("; ") : "none") + "</td></tr>";
      }).join("") + "</table>";
    document.getElementById("js").innerHTML =
      "<p><span class='pill " + data.jsInterop.status + "'>" +
      data.jsInterop.status + "</span> " + data.jsInterop.detail + "</p>";
    document.getElementById("prs").innerHTML =
      "<table><tr><th>PR</th><th>Branch</th><th>Title</th><th>Status</th></tr>" +
      data.prs.map(function (pr) {
        return "<tr><td>" + pr.id + "</td><td>" + pr.branch + "</td><td>" +
          pr.title + "</td><td><span class='pill " + pr.status + "'>" +
          pr.status + "</span></td></tr>";
      }).join("") + "</table>";
  }
  var embedded = JSON.parse(document.getElementById("status-data").textContent);
  render(embedded);
  fetch("status.json").then(function (r) {
    if (r.ok) r.json().then(render);
  }).catch(function () {});
})();
</script>
</body>
</html>
"""


def main():
    with open(os.path.join(SCRIPTS, "status-manual.json")) as f:
        data = json.load(f)
    data["generated_at"] = datetime.now(timezone.utc).isoformat(timespec="seconds")
    data["commit"] = head_commit()
    data["coverage"] = load_coverage()
    data["tests"] = load_tests()
    with open(os.path.join(AI, "status.json"), "w") as f:
        json.dump(data, f, indent=2)
        f.write("\n")
    html = HTML_TEMPLATE.replace(
        "__STATUS_JSON__", json.dumps(data, indent=2)
    )
    with open(os.path.join(AI, "status.html"), "w") as f:
        f.write(html)
    cov = data["coverage"]
    if cov:
        print(f"coverage: line {cov['lineRate']:.4f} branch {cov['branchRate']:.4f}")
    else:
        print("coverage: no report found")
    print("wrote .ai/status.json and .ai/status.html")


if __name__ == "__main__":
    sys.exit(main())
