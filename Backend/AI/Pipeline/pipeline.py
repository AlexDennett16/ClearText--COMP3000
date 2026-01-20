from AI.nlp.tokenizer import tokenize
from ..Rules.spellcheck import detect_spelling_errors


def grammar_pipeline(text: str):
    tokens = tokenize(text)

    errors = []
    errors.extend(detect_spelling_errors(tokens))

    return {"text": text, "tokens": tokens, "errors": errors}
