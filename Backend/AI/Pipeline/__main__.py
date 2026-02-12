import json
import sys
from AI.Pipeline.pipeline import grammar_pipeline

if __name__ == "__main__":

    text = sys.stdin.read()

    result = grammar_pipeline(text)
    print(json.dumps(result, indent=2))
