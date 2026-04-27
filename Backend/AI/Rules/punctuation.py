from typing import List
import re


def check_punctuation_errors(tokens: List[str]) -> List[dict]:
    errors = []

    for i, token in enumerate(tokens):
        # Match 2 or more punctuation marks at the end of a token
        if re.search(r"[,.!?]{2,}$", token):
            correctPunctuation = token[-1]
            word = token.strip(".,!?")

            errors.append(
                {
                    "type": "duplicate punctuation",
                    "token": token,
                    "index": i,
                    "suggestions": [word + correctPunctuation],
                }
            )

    return errors
