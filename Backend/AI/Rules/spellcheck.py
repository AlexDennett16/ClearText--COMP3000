import re
from functools import lru_cache
from typing import List, Dict, Set
from ..nlp.corporaLoader import load_corpora
from nltk.metrics import edit_distance
from wordfreq import zipf_frequency
from ..Helpers.commonTypos import COMMON_TYPOS
from ..Helpers.keyboardNeighbours import KEYBOARD_NEIGHBORS
from ..Helpers.candidateUtils import edits1, keyboard_edits
from ..Helpers.textUtils import (
    collapse_duplicates,
    match_case,
    replace_core_preserve_punctuation,
)

# ---------------------------------------------------------------------------
# Load dictionary
# ---------------------------------------------------------------------------

WORD_LIST = load_corpora()
WORD_SET: Set[str] = set(map(str.lower, WORD_LIST))


# ---------------------------------------------------------------------------
# Cached helpers
# ---------------------------------------------------------------------------


# O(n^2) complexity, caching greatly improves performance as many candidates share the same words
@lru_cache(maxsize=100_000)
def dist_cached(a: str, b: str) -> int:
    return edit_distance(a, b)


# Called every word in the text, and so benefits from caching, as many candidates share the same words and frequencies
# Zipf frequency is a measure of word commonness, with high = more common, on a 0-8 scale.
# Scale is in log10 so a value of 6 means it appears 1/1000, with each additional point being a 10x increase in frequency
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

    # Edit-distance-based candidates (Edit distance 1)
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


# Scores candidates based on a combination of edit distance, word frequency, and keyboard proximity, with tunable weights
# Lower score is better, inf prevents use
def score_candidate(token: str, word: str) -> float:
    dist = dist_cached(token, word)
    # Hard cutoff to prevent overly distant candidates from being considered
    if dist > 3:
        return float("inf")

    # Cap frequency contribution to prevent it from dominating the score, as edit distance is more important for relevance
    freq = min(freq_cached(word), 5.0)

    kb = 0.0
    # Char match score: +0.2 for each exact char match
    # +0.1 for each keyboard-neighbour match, incentivising candidates that are similar to the original token
    for ca, cb in zip(token, word):
        if ca == cb:
            kb += 0.2
        elif cb in KEYBOARD_NEIGHBORS.get(ca, ""):
            kb += 0.1

    # Final score allows for weights to be placed differently to optimise results
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
        # Scrip away non-alphabetic characters to get to the core of the word for spellchecking, as well as discarding letters
        core = re.sub(r"[^a-zA-Z]", "", token)
        if len(core) < 2:
            continue
        core_lower = core.lower()

        # Check predefined dict for typos, fast pass with given solutions
        if core_lower in COMMON_TYPOS:
            corrected = COMMON_TYPOS[core_lower]

            suggestion = replace_core_preserve_punctuation(
                token,
                match_case(core, corrected),
            )

            errors.append(
                {
                    "type": "spelling",
                    "token": token,
                    "index": i,
                    "suggestions": [suggestion],
                }
            )
            continue

        # Skip valid words
        if core_lower in WORD_SET:
            continue

        # Simple plural tolerance, catches false positives that are omitted from corpora
        if (
            core_lower.endswith("s")
            and not core_lower.endswith("ss")
            and core_lower[:-1] in WORD_SET
        ):
            continue

        # Always emit an error for misspellings as we display no suggestions in this case
        suggestions_raw = suggest_corrections(core_lower)

        # Ensure we rematch original case and punctuation in suggestions
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
