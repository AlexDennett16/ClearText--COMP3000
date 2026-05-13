import math
from typing import Set
from .keyboardNeighbours import KEYBOARD_NEIGHBORS


# ----------------------------------------------------------------------------
# Generate all terms that are one edit distance away from the input word
# Words are generated at this stage, but not filtered for existence in the dictionary or frequency
# ----------------------------------------------------------------------------
def edits1(word: str) -> Set[str]:
    letters = "abcdefghijklmnopqrstuvwxyz"
    splits = [(word[:i], word[i:]) for i in range(len(word) + 1)]

    deletes = [L + R[1:] for L, R in splits if R]
    transposes = [L + R[1] + R[0] + R[2:] for L, R in splits if len(R) > 1]
    replaces = [L + c + R[1:] for L, R in splits if R for c in letters]
    inserts = [L + c + R for L, R in splits for c in letters]

    return set(deletes + transposes + replaces + inserts)


# Generate edits based on keyboard proximity
def keyboard_edits_dynamic(word: str) -> Set[str]:
    max_typos = allowed_typos(word)

    results = set()
    current_level = {word}

    for _ in range(max_typos):
        next_level = set()

        for w in current_level:
            edits = keyboard_edits(w)

            for e in edits:
                next_level.add(e)

        results.update(next_level)
        current_level = next_level

    # Remove original word if present
    if word in results:
        results.remove(word)

    return results


def allowed_typos(word: str) -> int:
    return max(1, math.ceil(len(word) * 0.2))


def keyboard_edits(word: str) -> Set[str]:
    results = set()

    for i in range(len(word)):
        char = word[i]
        neighbors = KEYBOARD_NEIGHBORS.get(char, "")

        for n in neighbors:
            new_word = word[:i] + n + word[i + 1 :]
            results.add(new_word)

    return results
