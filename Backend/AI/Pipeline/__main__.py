import json
import sys
from AI.Pipeline.pipeline import grammar_pipeline

if __name__ == "__main__":

    for line in sys.stdin:
        text = line.strip()
        if not text:
            continue

        result = grammar_pipeline(text)

        print(json.dumps(result))
        sys.stdout.flush()
