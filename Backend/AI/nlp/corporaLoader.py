import nltk
from nltk.corpus import words


def load_corpora(): 
    nltk.download("punkt", quiet=True)
    nltk.download("words", quiet=True) #words is old corpus, worth updating to something more modern later

    return set(words.words())
