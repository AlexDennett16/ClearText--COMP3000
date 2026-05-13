from wordfreq import top_n_list, zipf_frequency


def load_corpora(
    max_words: int = 50000,
    min_zipf: float = 3.0,
):
    words = []

    for w in top_n_list("en", max_words):
        w = w.lower()

        if not w.isalpha():
            continue

        # Discard very rare words that we are unlikely to need
        if zipf_frequency(w, "en") < min_zipf:
            continue

        words.append(w)

    return set(words)
