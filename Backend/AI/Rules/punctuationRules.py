def check_punctuation(sentence: str):
    issues = []
    if not sentence.endswith(('.', '!', '?')):
        issues.append("Sentence should end with punctuation.")
    return issues