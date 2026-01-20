import json
from AI.Pipeline.pipeline import grammar_pipeline

if __name__ == "__main__":
    result = grammar_pipeline("Smoe errors in ths sentnce")
    print(json.dumps(result, indent=2))
