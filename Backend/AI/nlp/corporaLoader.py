import nltk
from nltk.corpus import words
from wordfreq import top_n_list


def load_corpora():
    nltk.download("punkt", quiet=True)

    return set(top_n_list("en", 100000))
