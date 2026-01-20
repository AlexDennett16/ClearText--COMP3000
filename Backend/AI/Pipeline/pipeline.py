from AI.nlp.tokenizer import tokenize
from ..Rules.capitalisation import check_capitalization_errors
from ..Rules.spellcheck import detect_spelling_errors
from ..Rules.punctuation import check_punctuation_errors


def grammar_pipeline(text: str):
    tokens = tokenize(text)

    errors = []
    errors.extend(detect_spelling_errors(tokens))
    errors.extend(check_punctuation_errors(tokens))
    errors.extend(check_capitalization_errors(tokens))

    return {"text": text, "tokens": tokens, "errors": errors}
