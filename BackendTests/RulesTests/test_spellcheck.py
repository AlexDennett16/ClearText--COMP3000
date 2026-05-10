import pytest
from Backend.AI.Rules.spellcheck import detect_spelling_errors


@pytest.mark.parametrize(
    "misspelled,correct",
    [
        ("teh", "the"),
        ("recieve", "receive"),
        ("acommodate", "accommodate"),
        ("definately", "definitely"),
        ("tthe", "the"),
        ("recievee", "receive"),
        ("acomcmodate", "accommodate"),
        ("definateely", "definitely"),
    ],
)
def test_common_misspellings(misspelled, correct):
    errors = detect_spelling_errors([misspelled])

    assert len(errors) == 1  # sanity check

    suggestions = errors[0]["suggestions"]
    assert any(correct.lower() == s.lower() for s in suggestions)
