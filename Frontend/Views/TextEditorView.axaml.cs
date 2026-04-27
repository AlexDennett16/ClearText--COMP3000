using System;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ClearText.Constants;
using ClearText.DataObjects;
using ClearText.Services;
using ClearText.Utilities;
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
            this.WhenAnyValue(v => v.ViewModel!.DocumentText)
                .Subscribe(text =>
                {
                    if (text != Editor.Document.Text)
                        Editor.Document.Text = text;
                })
                .DisposeWith(disposables);

            Editor.Document.TextChanged += (_, _) =>
            {
                if (ViewModel != null)
                    ViewModel.DocumentText = Editor.Document.Text;
            };

            this.WhenAnyValue(v => v.ViewModel!.Errors)
                .Subscribe(_ =>
                {
                    if (ViewModel != null)
                        LoadSquigglies(ViewModel);
                })
                .DisposeWith(disposables);
        });
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pos = e.GetPosition(Editor.TextArea.TextView);
        var logical = Editor.TextArea.TextView.GetPositionFloor(pos);

        if (logical == null)
            return;

        var offset = Editor.Document.GetOffset(logical.Value.Line, logical.Value.Column);

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

    private void ApplySuggestion(string suggestion)
    {
        if (ViewModel == null || _activeMarker == null)
            return;

        var suggestions = _activeMarker.Error.Suggestions;

        if (suggestions is null || suggestions.Count == 0 ||
            suggestions[0] == ClearTextErrorConstants.NoSuggestions)
            return;

        Editor.Document.Replace(
            _activeMarker.StartOffset,
            _activeMarker.Length,
            suggestion
        );

        _markerService.Remove(_activeMarker);
        Editor.TextArea.TextView.Redraw();

        var flyout = (Flyout)Editor.GetValue(FlyoutBase.AttachedFlyoutProperty)!;
        flyout.Hide();
    }

    private void SuggestionButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Content: string suggestion })
        {
            ApplySuggestion(suggestion);
        }
    }

    private void LoadSquigglies(TextEditorViewModel vm)
    {

        var text = Editor.Document.Text;

        var tokens = TextTokeniser.TokeniseOnWhitespace(text)
            .Select(t => t.Text)
            .ToList();


        _markerService.ClearMarkers();
        _markerService.LoadSquigglies(
        text,
        tokens,
        vm.Errors ?? []
        );

        Editor.TextArea.TextView.Redraw();
    }
}