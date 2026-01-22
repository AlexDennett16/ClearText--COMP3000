from typing import List


def check_punctuation_errors(tokens: List[str]) -> List[str]:
    errors = []
    for i, token in enumerate(tokens[:-1]):
        if token in {".", ",", "!", "?"} and i + 1 < len(tokens):
            next_token = tokens[i + 1]
            if next_token in {".", ",", "!", "?"}:
                errors.append(
                    {
                        "type": "duplicate punctuation",
                        "token": token,
                        "index": i,
                    }
                )
    return errors
