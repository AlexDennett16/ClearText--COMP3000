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
    return token[0].lower() == word[0].lower()


def suggest_corrections(token: str, max_suggestions: int = 3):
    candidates = []
    token_lowerLen = len(token)
    token_lower = token.lower()

    for length in range(token_lowerLen - 2, token_lowerLen + 3):
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
        if not token.isalpha() or len(token) < 2:
            continue

        token_lower = token.lower()

        if token_lower in WORD_SET:
            continue

        # Optional plural tolerance
        if token_lower.endswith("s") and token_lower[:-1] in WORD_SET:
            continue

        if suggestions := suggest_corrections(token):
            errors.append(
                {
                    "type": "spelling",
                    "token": token,
                    "index": i,
                    "suggestions": suggestions,
                }
            )

    return errors
