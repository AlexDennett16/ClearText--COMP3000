from pydoc import text

from AI.nlp.tokenizer import tokenize
from ..Rules.capitalisation import check_capitalization_errors
from ..Rules.spellcheck import detect_spelling_errors
from ..Rules.punctuation import check_punctuation_errors


def grammar_pipeline(token: list[str]):

    errors = []
    errors.extend(detect_spelling_errors(token))
    errors.extend(check_punctuation_errors(token))
    errors.extend(check_capitalization_errors(token))

    return errors
