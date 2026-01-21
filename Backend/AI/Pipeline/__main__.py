import json
from AI.Pipeline.pipeline import grammar_pipeline

if __name__ == "__main__":
    result = grammar_pipeline(
        "somee errors in this sentence. let us see if we can find thme Then we can stop complaning!"
    )
    print(json.dumps(result, indent=2))
