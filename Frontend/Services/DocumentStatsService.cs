using System;
using System.Linq;
using ClearText.BaseTypes;
using ClearText.DataObjects;
using ClearText.Interfaces;

namespace ClearText.Services;

public class DocumentStatsService : BaseService, IDocumentStatsService
{
    public DocumentStats GetDocumentStats(string documentText)
    {
        if (string.IsNullOrWhiteSpace(documentText))
        {
            return new DocumentStats
            {
                WordCount = "0",
                CharacterCount = "0",
                FleschKincaidGradeLevel = "0",
                FleschKincaidBreakdown = "N/A"
            };
        }

        var words = documentText
            .Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries);

        var sentences = documentText
            .Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries);

        var wordCount = words.Length;
        var sentenceCount = Math.Max(1, sentences.Length); // avoid divide-by-zero
        var syllableCount = words.Sum(CountSyllables);

        var fkGrade = CalculateFleschKincaid(wordCount, sentenceCount, syllableCount);

        return new DocumentStats
        {
            WordCount = wordCount.ToString(),
            CharacterCount = documentText.Length.ToString(),
            FleschKincaidGradeLevel = fkGrade.ToString("0.#"),
            FleschKincaidBreakdown = GetReadabilityDescription(fkGrade)
        };
    }

    private static int CountSyllables(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return 0;

        word = word.ToLower().Trim();

        var syllables = 0;
        var lastWasVowel = false;

        foreach (var c in word)
        {
            if (IsVowel(c))
            {
                if (!lastWasVowel)
                    syllables++;

                lastWasVowel = true;
            }
            else
            {
                lastWasVowel = false;
            }
        }

        // Remove silent "e"
        if (word.EndsWith('e') && syllables > 1)
            syllables--;

        // Ensure at least 1 syllable
        return Math.Max(1, syllables);

        //y is classified as a vowel for FKGL
        static bool IsVowel(char c)
        {
            return "aeiouy".Contains(c);
        }
    }

    private static double CalculateFleschKincaid(double words, double sentences, double syllables)
    {
        var wordsPerSentence = words / sentences;
        var syllablesPerWord = syllables / words;

        var result = 0.39 * wordsPerSentence +
                     11.8 * syllablesPerWord -
                     15.59;
        result = Math.Round(result, 3);

        return result;
    }

    public static string GetReadabilityDescription(double fkgl)
    {
        return fkgl switch
        {
            <= 3.0 => "Primary School (KS1-KS2) — Years 2-4 — Ages 6-9",
            <= 6.0 => "Primary KS2 / Lower Secondary (KS3) — Years 5-7 — Ages 9-12",
            <= 9.0 => "Secondary School (KS3) — Years 8-9 — Ages 12-14",
            <= 12.0 => "Secondary School (KS4-KS5) — Years 10-13 — Ages 14-18",
            _ => "University / Higher Education — Ages 18+"
        };
    }
}