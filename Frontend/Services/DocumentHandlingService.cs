using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearText.Interfaces;
using DocumentFormat.OpenXml.Packaging;

// Explicit OpenXML aliases to avoid collisions with avalonia controls
using WordRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WordParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WordText = DocumentFormat.OpenXml.Wordprocessing.Text;
using System.Linq;
using ClearText.BaseTypes;

namespace ClearText.Services;

// Service responsible for handling loading and saving of .docx documents 
public sealed class DocumentHandlingService(IPathService pathService) : BaseService, IDocumentHandlingService
{
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private readonly List<WordRun> _originalRuns = [];

    public string LoadText(string path)
    {
        using var doc = WordprocessingDocument.Open(path, false);
        var body = doc.MainDocumentPart?.Document?.Body ??
                   throw new InvalidOperationException("The document body is null.");

        _originalRuns.Clear();
        var sb = new StringBuilder();
        var paragraphs = body.Elements<WordParagraph>().ToList();

        foreach (var paragraph in paragraphs)
        {
            foreach (var run in paragraph.Elements<WordRun>())
            {
                _originalRuns.Add((WordRun)run.CloneNode(true));
                sb.Append(run.InnerText);
            }
        }

        return sb.ToString();
    }

    public async Task SaveTextAsync(string filePath, string documentText)
    {
        await _saveLock.WaitAsync();
        try
        {
            using var doc = WordprocessingDocument.Open(filePath, true);
            var body = doc.MainDocumentPart?.Document?.Body
                       ?? throw new InvalidOperationException("The document body is null.");

            body.RemoveAllChildren();

            var textIndex = 0;

            foreach (var originalRun in _originalRuns)
            {
                var newRun = (WordRun)originalRun.CloneNode(true);

                var length = originalRun.InnerText.Length;
                if (textIndex + length > documentText.Length)
                    length = documentText.Length - textIndex;

                if (length <= 0)
                    break;

                var runText = documentText.Substring(textIndex, length);
                textIndex += length;

                newRun.RemoveAllChildren<WordText>();
                newRun.AppendChild(new WordText(runText));

                var paragraph = new WordParagraph();
                paragraph.Append(newRun);
                body.Append(paragraph);
            }

            if (textIndex < documentText.Length)
            {
                var remaining = documentText[textIndex..];
                body.Append(new WordParagraph(new WordRun(new WordText(remaining))));
            }

            doc.MainDocumentPart.Document.Save();
            pathService.TouchPage(filePath);
        }
        catch (IOException ex)
        {
            // Quietly log external IO error
            Debug.WriteLine($"Save skipped due to file lock: {ex.Message}");
        }
        finally
        {
            _saveLock.Release();
        }
    }
}