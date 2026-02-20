from typing import List


def check_capitalization_errors(tokens: List[str]) -> List[str]:
    errors = []
    for i, token in enumerate(tokens):
        if i > 0 and tokens[i - 1] in {".", "!", "?"} and not token[0].isupper():
            errors.append(
                {
                    "type": "capitalization",
                    "token": token,
                    "index": i,
                    "suggestions": [token.capitalize()],
                }
            )
    return errors
