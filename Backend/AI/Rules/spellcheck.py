import re
from functools import lru_cache
from typing import List, Dict
from collections import defaultdict
from ..nlp.corporaLoader import load_corpora
from nltk.metrics import edit_distance
from wordfreq import zipf_frequency

WORD_LIST = load_corpora()
WORD_SET = set(map(str.lower, WORD_LIST))

WORD_BUCKETS = defaultdict(list)
for w in WORD_LIST:
    WORD_BUCKETS[len(w)].append(w)


@lru_cache(maxsize=50_000)
def dist_cached(a: str, b: str) -> int:
    return edit_distance(a, b)


@lru_cache(maxsize=50_000)
def freq_cached(word: str) -> float:
    return zipf_frequency(word, "en")


def passes_prefilters(token: str, word: str) -> bool:
    # First-letter heuristic
    return token[0].lower() == word[0].lower()


# Matches the case pattern of the original to the suggestion - either all caps or first letter
def match_case(original: str, suggestion: str) -> str:
    if original.isupper():
        return suggestion.upper()
    if original[0].isupper():
        return suggestion.capitalize()
    return suggestion.lower()


# Replaces the alphabetic core of the token with the corrected word, preserving punctuation in suggestion
def replace_core_preserve_punctuation(token: str, corrected_core: str) -> str:
    """
    Replaces the alphabetic core of `token` with `corrected_core`,
    preserving leading and trailing punctuation.
    """
    match = re.match(r"(^[^a-zA-Z]*)([a-zA-Z]+)([^a-zA-Z]*$)", token)
    if not match:
        # Fallback: replace entire token
        return corrected_core

    prefix, _, suffix = match.groups()
    return f"{prefix}{corrected_core}{suffix}"


def suggest_corrections(token: str, max_suggestions: int = 3):
    candidates = []
    token_len = len(token)
    token_lower = token.lower()

    for length in range(token_len - 2, token_len + 3):
        for word in WORD_BUCKETS.get(length, []):
            if not passes_prefilters(token, word):
                continue

            dist = dist_cached(token_lower, word.lower())
            if dist <= 2:
                candidates.append((word, dist, freq_cached(word)))

    candidates.sort(key=lambda x: (x[1], -x[2]))
    return [w for w, _, _ in candidates[:max_suggestions]]


def detect_spelling_errors(tokens: List[str]) -> List[Dict]:
    errors = []

    for i, token in enumerate(tokens):
        # Extract alphabetic core
        core = re.sub(r"[^a-zA-Z]", "", token)
        if len(core) < 2:
            continue

        core_lower = core.lower()

        # Correct word – no error
        if core_lower in WORD_SET:
            continue

        # Plural tolerance (e.g. "things" but not "thingss")
        if (
            core_lower.endswith("s")
            and not core_lower.endswith("ss")
            and core_lower[:-1] in WORD_SET
        ):
            continue

        raw_suggestions = suggest_corrections(core_lower)
        if not raw_suggestions:
            continue

        suggestions = [
            replace_core_preserve_punctuation(token, match_case(core, suggestion))
            for suggestion in raw_suggestions
        ]

        errors.append(
            {
                "type": "spelling",
                "token": token,
                "index": i,
                "suggestions": suggestions,
            }
        )

    return errors
