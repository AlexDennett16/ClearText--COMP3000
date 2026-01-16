from spellchecker import SpellChecker

spell = SpellChecker()


def check_spelling(text: str):
    words = text.split()
    misspelled = spell.unknown(words)
    return {word: list(spell.candidates(word)) for word in misspelled}
