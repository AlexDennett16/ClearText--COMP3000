import re


# Strips internal double letters
def collapse_duplicates(word: str) -> str:
    return re.sub(r"(.)\1+", r"\1", word)


# Naive case match
def match_case(original: str, suggestion: str) -> str:
    if original.isupper():
        return suggestion.upper()
    if original[:1].isupper():
        return suggestion.capitalize()
    return suggestion.lower()


def replace_core_preserve_punctuation(token: str, corrected_core: str) -> str:
    match = re.match(r"(^[^a-zA-Z]*)([a-zA-Z]+)([^a-zA-Z]*$)", token)
    if not match:
        return corrected_core
    prefix, _, suffix = match.groups()
    return f"{prefix}{corrected_core}{suffix}"
