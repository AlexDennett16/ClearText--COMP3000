from typing import Set
from .keyboardNeighbours import KEYBOARD_NEIGHBORS


def edits1(word: str) -> Set[str]:
    letters = "abcdefghijklmnopqrstuvwxyz"
    splits = [(word[:i], word[i:]) for i in range(len(word) + 1)]

    deletes = [L + R[1:] for L, R in splits if R]
    transposes = [L + R[1] + R[0] + R[2:] for L, R in splits if len(R) > 1]
    replaces = [L + c + R[1:] for L, R in splits if R for c in letters]
    inserts = [L + c + R for L, R in splits for c in letters]

    return set(deletes + transposes + replaces + inserts)


def keyboard_edits(word: str) -> Set[str]:
    results = set()
    for i, c in enumerate(word):
        for n in KEYBOARD_NEIGHBORS.get(c, ""):
            results.add(word[:i] + n + word[i + 1 :])
    return results
