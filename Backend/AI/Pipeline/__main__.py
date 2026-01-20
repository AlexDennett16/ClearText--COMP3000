import json
from AI.Pipeline.pipeline import grammar_pipeline

if __name__ == "__main__":
    result = grammar_pipeline(
        "Some errors in this sentence. let us see if we can find them Then we can stop complaining!"
    )
    print(json.dumps(result, indent=2))
