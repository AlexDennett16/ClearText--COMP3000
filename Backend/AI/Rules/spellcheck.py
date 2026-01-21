from typing import List, Dict
from ..nlp.corporaLoader import load_corpora
from nltk.metrics import edit_distance
from wordfreq import zipf_frequency

WORD_LIST = load_corpora()


def suggest_corrections(token: str, max_suggestions=3):
    candidates = []
    for word in WORD_LIST:
        dist = edit_distance(token.lower(), word.lower())
        if dist <= 2:
            freq = zipf_frequency(word, "en")
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
