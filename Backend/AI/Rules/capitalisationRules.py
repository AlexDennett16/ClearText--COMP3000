def check_capitalization(sentence: str):
    issues = []
    if sentence and not sentence[0].isupper():
        issues.append("Start the sentence with a capital letter.")
    return issues
