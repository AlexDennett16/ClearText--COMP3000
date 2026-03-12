import json
import sys
from AI.Pipeline.pipeline import grammar_pipeline


def read_csharp_json():
    for line in sys.stdin:
        raw = line.strip()
        if not raw:
            continue

        data = json.loads(raw)
        return data

    return None


if __name__ == "__main__":
    while True:
        data = read_csharp_json()
        if data is None:
            continue

        # Extract the actual text field
        text = data.get("text", "")

        # Run grammar pipeline on the REAL text
        result = grammar_pipeline(text)

        # Send JSON back to C#
        print(json.dumps(result))
        sys.stdout.flush()
