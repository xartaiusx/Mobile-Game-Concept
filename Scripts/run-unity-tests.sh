#!/usr/bin/env bash
set -u

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_BIN="${UNITY_BIN:-$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity}"
RESULTS_DIR="$ROOT_DIR/TestResults"
EDIT_LOG="${EDIT_LOG:-/tmp/mobile-game-editmode-custom.log}"
PLAY_LOG="${PLAY_LOG:-/tmp/mobile-game-playmode-custom.log}"
EDIT_XML="$RESULTS_DIR/editmode-results.xml"
PLAY_XML="$RESULTS_DIR/playmode-results.xml"
UNITY_TEST_TIMEOUT="${UNITY_TEST_TIMEOUT:-600}"

mkdir -p "$RESULTS_DIR"
rm -f "$RESULTS_DIR/editmode-summary.json" "$RESULTS_DIR/playmode-summary.json" "$RESULTS_DIR/summary.txt"

run_unity_mode() {
    local mode="$1"
    local method="$2"
    local log_file="$3"
    local xml_file="$4"

    timeout "$UNITY_TEST_TIMEOUT" "$UNITY_BIN" \
        -batchmode \
        -projectPath "$ROOT_DIR" \
        -executeMethod "$method" \
        -runTests \
        -testPlatform "$mode" \
        -testResults "$xml_file" \
        -logFile "$log_file"
    return $?
}

edit_exit=0
play_exit=0

run_unity_mode "EditMode" "Game.Editor.ProjectTestRunner.RunEditMode" "$EDIT_LOG" "$EDIT_XML"
edit_exit=$?

run_unity_mode "PlayMode" "Game.Editor.ProjectTestRunner.RunPlayMode" "$PLAY_LOG" "$PLAY_XML"
play_exit=$?

failures=0

require_nonempty() {
    local file="$1"
    if [[ ! -s "$file" ]]; then
        echo "Missing or empty summary: $file" >&2
        failures=1
    fi
}

require_nonempty "$RESULTS_DIR/editmode-summary.json"
require_nonempty "$RESULTS_DIR/playmode-summary.json"
require_nonempty "$RESULTS_DIR/summary.txt"

print_totals() {
    local label="$1"
    local file="$2"
    if [[ ! -s "$file" ]]; then
        return
    fi

    python3 - "$label" "$file" <<'PY'
import json
import sys

label, path = sys.argv[1], sys.argv[2]
with open(path, "r", encoding="utf-8") as handle:
    data = json.load(handle)
print(
    f"{label}: total={data.get('total', 0)} "
    f"passed={data.get('passed', 0)} "
    f"failed={data.get('failed', 0)} "
    f"skipped={data.get('skipped', 0)} "
    f"inconclusive={data.get('inconclusive', 0)}"
)
for failure in data.get("failures", []):
    print(f"{label} failure: {failure.get('name', '')} :: {failure.get('message', '')}")
sys.exit(1 if data.get("failed", 0) else 0)
PY
    local parse_exit=$?
    if [[ $parse_exit -ne 0 ]]; then
        failures=1
    fi
}

print_totals "EditMode" "$RESULTS_DIR/editmode-summary.json"
print_totals "PlayMode" "$RESULTS_DIR/playmode-summary.json"

scan_log() {
    local label="$1"
    local file="$2"
    if [[ ! -f "$file" ]]; then
        echo "Missing log: $file" >&2
        failures=1
        return
    fi

    local matches
    matches="$(grep -En 'error CS|Compilation failed|NullReferenceException|MissingReferenceException|Missing Script|Missing script|test failures|FAIL|Assertion failed' "$file" || true)"
    if [[ -n "$matches" ]]; then
        echo "$label log contains failure markers:" >&2
        echo "$matches" >&2
        failures=1
    fi
}

scan_log "EditMode" "$EDIT_LOG"
scan_log "PlayMode" "$PLAY_LOG"

if [[ $edit_exit -ne 0 ]]; then
    echo "EditMode Unity invocation exited $edit_exit" >&2
    failures=1
fi

if [[ $play_exit -ne 0 ]]; then
    echo "PlayMode Unity invocation exited $play_exit" >&2
    failures=1
fi

if [[ $failures -ne 0 ]]; then
    exit 1
fi

cat "$RESULTS_DIR/summary.txt"
