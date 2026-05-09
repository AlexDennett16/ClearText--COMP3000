import re
from functools import lru_cache
from typing import List, Dict, Set

from ..nlp.corporaLoader import load_corpora
from nltk.metrics import edit_distance
from wordfreq import zipf_frequency

from ..Helpers.keyboardNeighbours import KEYBOARD_NEIGHBORS
from ..Helpers.textUtils import (
    collapse_duplicates,
    match_case,
    replace_core_preserve_punctuation,
)
from ..Helpers.candidateUtils import edits1, keyboard_edits

# ---------------------------------------------------------------------------
# Load dictionary
# ---------------------------------------------------------------------------

WORD_LIST = load_corpora()
WORD_SET: Set[str] = set(map(str.lower, WORD_LIST))


# ---------------------------------------------------------------------------
# Cached helpers
# ---------------------------------------------------------------------------


@lru_cache(maxsize=100_000)
def dist_cached(a: str, b: str) -> int:
    return edit_distance(a, b)


@lru_cache(maxsize=100_000)
def freq_cached(word: str) -> float:
    return zipf_frequency(word, "en")


# ---------------------------------------------------------------------------
# Phase 1: candidate generation
# ---------------------------------------------------------------------------


def candidate_words(token: str) -> Set[str]:
    token = token.lower()
    collapsed = collapse_duplicates(token)

    candidates: Set[str] = set()

    # Edit-distance-based candidates
    candidates |= {w for w in edits1(collapsed) if w in WORD_SET}

    # Keyboard-neighbour candidates
    candidates |= {w for w in keyboard_edits(token) if w in WORD_SET}

    # Fallback: conservative distance search
    if not candidates:
        candidates = {
            w
            for w in WORD_SET
            if abs(len(w) - len(token)) <= 2 and dist_cached(token, w) <= 2
        }

    return candidates


# ---------------------------------------------------------------------------
# Phase 2: ranking
# ---------------------------------------------------------------------------


def score_candidate(token: str, word: str) -> float:
    dist = dist_cached(token, word)
    if dist > 3:
        return float("inf")

    freq = min(freq_cached(word), 5.0)

    kb = 0.0
    for ca, cb in zip(token, word):
        if ca == cb:
            kb += 0.2
        elif cb in KEYBOARD_NEIGHBORS.get(ca, ""):
            kb += 0.1

    return dist * 10.0 - kb * 2.0 - freq


# ---------------------------------------------------------------------------
# Public API
# ---------------------------------------------------------------------------


def suggest_corrections(token: str, max_suggestions: int = 3) -> List[str]:
    candidates = candidate_words(token)

    scored = [(w, score_candidate(token.lower(), w)) for w in candidates]
    scored.sort(key=lambda x: x[1])

    return [w for w, _ in scored[:max_suggestions]]


def detect_spelling_errors(tokens: List[str]) -> List[Dict]:
    errors: List[Dict] = []

    for i, token in enumerate(tokens):
        core = re.sub(r"[^a-zA-Z]", "", token)
        if len(core) < 2:
            continue

        core_lower = core.lower()

        # Skip valid words
        if core_lower in WORD_SET:
            continue

        # Simple plural tolerance
        if (
            core_lower.endswith("s")
            and not core_lower.endswith("ss")
            and core_lower[:-1] in WORD_SET
        ):
            continue

        # Always emit an error for misspellings
        suggestions_raw = suggest_corrections(core_lower)

        suggestions = [
            replace_core_preserve_punctuation(
                token,
                match_case(core, s),
            )
            for s in suggestions_raw
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
