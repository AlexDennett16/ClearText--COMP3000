using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ClearText.Constants;
using ClearText.DataObjects;
using ClearText.Services;
using ClearText.ViewModels;
using ReactiveUI;
using static ClearText.Services.TextMarkerService;

namespace ClearText.Views;

public partial class TextEditorView : ReactiveUserControl<TextEditorViewModel>
{
    private readonly TextMarkerService _markerService;
    private TextMarker? _activeMarker;

    public TextEditorView()
    {
        InitializeComponent();

        _markerService = new TextMarkerService(Editor.Document);
        Editor.TextArea.TextView.BackgroundRenderers.Add(_markerService);
        Editor.TextArea.PointerPressed += OnPointerPressed;

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.DocumentText, v => v.Editor.Text)
                .DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel!.Errors)
                .Subscribe(_ => LoadSquigglies(ViewModel))
                .DisposeWith(disposables);
        });
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pos = e.GetPosition(Editor.TextArea.TextView);
        var logical = Editor.TextArea.TextView.GetPositionFloor(pos);

        if (logical == null)
            return;

        int offset = Editor.Document.GetOffset(logical.Value.Line, logical.Value.Column);

        var marker = _markerService.GetMarkerAtOffset(offset);
        if (marker != null)
        {
            _activeMarker = marker;
            ShowErrorPopup(marker.Error);
        }
        else
        {
            var flyout = (Flyout)Editor.GetValue(FlyoutBase.AttachedFlyoutProperty)!;
            flyout.Hide();
        }
    }

    private void ShowErrorPopup(ClearTextError error)
    {
        ErrorMessage.Text = $"{error.Type}: \"{error.Token}\"";

        SuggestionList.ItemsSource =
            error.Suggestions is { Count: > 0 }
                ? error.Suggestions
                : new[] { ClearTextErrorConstants.NoSuggestions };

        var flyout = (Flyout)Editor.GetValue(FlyoutBase.AttachedFlyoutProperty)!;
        flyout.ShowAt(Editor, true);
    }

    private void HandleDuplicatePunctuation(string suggestion)
    {
        // Only do the expansion for duplicate punctuation errors


        int start = _activeMarker.StartOffset;
        int end = start + _activeMarker.Length;


        // Expand left
        while (start > 0)
        {
            char c = Editor.Document.GetCharAt(start - 1);
            if (!char.IsPunctuation(c))
                break;
            start--;
        }

        // Expand right
        while (end < Editor.Document.TextLength)
        {
            char c = Editor.Document.GetCharAt(end);
            if (!char.IsPunctuation(c))
                break;
            end++;
        }

        int length = end - start;

        Editor.Document.Replace(start, length, suggestion);
        RemoveMarkersInRange(start, end);

    }

    private void RemoveMarkersInRange(int start, int end)
    {
        var markers = _markerService.GetMarkers();

        foreach (var marker in markers)
        {
            int mStart = marker.StartOffset;
            int mEnd = marker.StartOffset + marker.Length;

            bool overlaps =
                !(mEnd <= start || mStart >= end); // intervals overlap

            if (overlaps)
                _markerService.Remove(marker);
        }
    }

    private void ApplySuggestion(string suggestion)
    {
        if (ViewModel == null || _activeMarker == null)
            return;

        if (_activeMarker.Error.Suggestions is not List<string> suggestions || suggestions[0] == ClearTextErrorConstants.NoSuggestions)
            return;


        if (_activeMarker.Error.Type == ClearTextErrorConstants.DuplicatePunctuation)
        {
            HandleDuplicatePunctuation(suggestion);
        }
        else
        {
            Editor.Document.Replace(
                _activeMarker.StartOffset,
                _activeMarker.Length,
                suggestion
            );
        }

        _markerService.Remove(_activeMarker);
        Editor.TextArea.TextView.Redraw();

        var flyout = (Flyout)Editor.GetValue(FlyoutBase.AttachedFlyoutProperty)!;
        flyout.Hide();
    }

    private void SuggestionButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content is string suggestion)
        {
            ApplySuggestion(suggestion);
        }
    }

    private void LoadSquigglies(TextEditorViewModel vm)
    {
        _markerService.ClearMarkers();
        _markerService.LoadSquigglies(Editor.Text, vm.Errors ?? []);
        Editor.TextArea.TextView.Redraw();
    }
}