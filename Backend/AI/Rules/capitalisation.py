import re
from typing import List


def check_capitalization_errors(tokens: List[str]) -> List[dict]:
    errors = []
    lowercase_i_pattern = re.compile(r"^i(?:['’](?:m|ve|ll|d))?[,.!?]*$")

    for i, token in enumerate(tokens):

        # Pronoun "i"
        if lowercase_i_pattern.match(token):
            errors.append(
                {
                    "type": "capitalization",
                    "token": token,
                    "index": i,
                    "suggestions": [token.capitalize()],
                }
            )

        # First word of document
        elif i == 0 and token and not token[0].isupper():
            errors.append(
                {
                    "type": "capitalization",
                    "token": token,
                    "index": i,
                    "suggestions": [token.capitalize()],
                }
            )
            continue

        # Sentence start
        elif (
            i > 0
            and tokens[i - 1].endswith((".", "!", "?"))
            and token
            and not token[0].isupper()
        ):
            errors.append(
                {
                    "type": "capitalization",
                    "token": token,
                    "index": i,
                    "suggestions": [token.capitalize()],
                }
            )
            continue

    return errors
