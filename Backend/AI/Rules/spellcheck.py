from functools import lru_cache
from typing import List, Dict
from ..nlp.corporaLoader import load_corpora
from nltk.metrics import edit_distance
from wordfreq import zipf_frequency

WORD_LIST = load_corpora()


# Cache to help with any repeated token checks
@lru_cache(maxsize=50000)
def dist_cached(a: str, b: str) -> int:
    return edit_distance(a, b)


@lru_cache(maxsize=50000)
def freq_cached(word: str) -> float:
    return zipf_frequency(word, "en")


def suggest_corrections(token: str, max_suggestions=3):
    candidates = []
    for word in WORD_LIST:
        if not passes_prefilters(token, word):
            continue

        dist = dist_cached(token.lower(), word.lower())
        if dist <= 2:
            freq = freq_cached(word)
            candidates.append((word, dist, freq))

    candidates.sort(key=lambda x: (x[1], -x[2]))
    return [w for w, d, f in candidates[:max_suggestions]]


def detect_spelling_errors(tokens: List[str]) -> List[Dict]:
    errors = []
    for i, token in enumerate(tokens):
        if token.isalpha() and token.lower() not in WORD_LIST:
            errors.append(
                {
                    "type": "spelling",
                    "token": token,
                    "index": i,
                    "suggestions": suggest_corrections(token),
                }
            )
    return errors


def passes_prefilters(token: str, word: str) -> bool:
    # Length difference filter
    if abs(len(token) - len(word)) > 2:
        return False

    # First letter filter
    if token[0].lower() != word[0].lower():
        return False

    return True
