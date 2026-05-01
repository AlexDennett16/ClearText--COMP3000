using System;
using System.IO;
using System.Reactive;
using ClearText.BaseTypes.BaseViewModels;
using ReactiveUI;
using DocumentFormat.OpenXml.Packaging;
using System.Threading.Tasks;
using System.Diagnostics;
using ClearText.Interfaces;

namespace ClearText.ViewModels;

public class PageViewModel : ViewModelBase
{
    private string _filePath;

    private string _previewText = string.Empty;

    public string PreviewText
    {
        get => _previewText;
        set => this.RaiseAndSetIfChanged(ref _previewText, value);
    }

    public string FilePath
    {
        get => _filePath;
        set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }

    public string Title => Path.GetFileNameWithoutExtension(FilePath);
    public ReactiveCommand<Unit, Unit> OpenEditorCommand { get; }
    public ReactiveCommand<Unit, Unit> RenameCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }


    public PageViewModel(
        string filePath,
        INavigationService navigationService,
        Action renameCallback,
        Action deleteCallback)

    {
        _filePath = filePath;

        PreviewText = "Loading Preview...";
        _ = ExtractDocxPreview(FilePath);


        OpenEditorCommand = ReactiveCommand.Create(() => navigationService.ShowEditor(FilePath));
        RenameCommand = ReactiveCommand.Create(renameCallback);
        DeleteCommand = ReactiveCommand.Create(deleteCallback);
    }

    public async Task ExtractDocxPreview(string filePath)
    {

        try
        {
            // Delay to allow for Editor to free up to allow for reads
            await Task.Delay(150);
            const int maxChars = 1000;

            using var doc = WordprocessingDocument.Open(filePath, false);

            var mainPart = doc.MainDocumentPart;
            if (mainPart?.Document?.Body == null)
            {
                DefaultPreviewName();
                return;
            }

            var text = mainPart.Document.Body.InnerText;

            if (string.IsNullOrWhiteSpace(text))
            {
                DefaultPreviewName();
                return;
            }

            PreviewText = text.Length > maxChars
                ? text[..maxChars] + "…"
                : text;

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading preview for {filePath}: {ex.Message}");
            DefaultPreviewName();
        }

    }

    private void DefaultPreviewName()
    {
        PreviewText = "No preview available";
    }
}