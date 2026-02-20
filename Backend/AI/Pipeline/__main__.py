import json
import sys
from AI.Pipeline.pipeline import grammar_pipeline


def read_csharp_json():
    for line in sys.stdin:
        text = line.strip()
        if not text:
            continue

        result = grammar_pipeline(text)

        print(json.dumps(result))
        sys.stdout.flush()


if __name__ == "__main__":
    while True:
        data = read_csharp_json()
        if data is None:
            continue
        text = data.get("text", "")

        result = grammar_pipeline(text)

        # Send JSON back to C#
        print(json.dumps(result))
        sys.stdout.flush()
