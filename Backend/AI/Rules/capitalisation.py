from typing import List


def check_capitalization_errors(tokens: List[str]) -> List[str]:
    errors = []
    errors.extend(
        {
            "type": "capitalization",
            "token": token,
            "index": i,
            "suggestions": [token.capitalize()],
        }
        for i, token in enumerate(tokens)
        # Check if the token is the first word of a sentence or the pronoun "I"
        if (i > 0 and tokens[i - 1] in {".", "!", "?"} and not token[0].isupper())
        or (token == "i" and token.islower())
    )
    ##Cover first word of the text
    if tokens and tokens[0] and not tokens[0][0].isupper():
        errors.append(
            {
                "type": "capitalization",
                "token": tokens[0],
                "index": 0,
                "suggestions": [tokens[0].capitalize()],
            }
        )
    return errors
