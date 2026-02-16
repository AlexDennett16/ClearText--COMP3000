using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit.Rendering;
using ClearText.DataObjects;
using ClearText.Services;
using ClearText.ViewModels;
using static ClearText.Services.TextMarkerService;

namespace ClearText.Views;

public partial class TextEditorView : UserControl
{
    readonly TextMarkerService _markerService;
    private ClearTextError? _activeError;
    private TextMarker? _activeMarker;
    public TextEditorView()
    {
        InitializeComponent();

        _markerService = new TextMarkerService(Editor.Document);
        Editor.TextArea.TextView.BackgroundRenderers.Add(_markerService);
        Editor.TextArea.PointerPressed += OnPointerPressed;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is not TextEditorViewModel vm)
            return;

        Editor.Text = vm.DocumentText;
        LoadSquigglies(vm);

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.DocumentText) &&
                Editor.Text != vm.DocumentText)
            {
                Editor.Text = vm.DocumentText;
            }

            if (e.PropertyName == nameof(vm.Errors))
            {
                LoadSquigglies(vm);
            }
        };

        Editor.TextChanged += (_, _) =>
        {
            if (vm.DocumentText != Editor.Text)
                vm.DocumentText = Editor.Text;
        };

        SuggestionList.PointerPressed += (_, e) =>
        {
            if (e.Source is Button btn && btn.Content is string suggestion)
            {
                ApplySuggestion(suggestion);
            }
        };

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

            var rect = GetMarkerRect(Editor.TextArea.TextView, marker);
            if (rect != null)
            {
                ShowErrorPopup(marker.Error, pos);
            }
        }
        else
        {
            // Clicked outside any marker, close the flyout
            var flyout = (Flyout)Editor.GetValue(FlyoutBase.AttachedFlyoutProperty)!;
            flyout.Hide();
        }
    }

    private void ShowErrorPopup(ClearTextError error, Point clickPos)
    {
        _activeError = error;
        ErrorMessage.Text = $"{error.Type}: \"{error.Token}\"";

        SuggestionList.ItemsSource =
            error.Suggestions is { Count: > 0 }
                ? error.Suggestions
                : new[] { "(No suggestions available)" };


        var flyout = (Flyout)Editor.GetValue(FlyoutBase.AttachedFlyoutProperty)!;


        flyout.ShowAt(Editor, true);
    }

    internal static Rect? GetMarkerRect(TextView textView, TextMarker marker)
    {
        var rects = BackgroundGeometryBuilder.GetRectsForSegment(textView, marker);
        return rects.FirstOrDefault();
    }

    //Replace text in editor with suggestion and remove squiggly
    private void ApplySuggestion(string suggestion)
    {
        if (DataContext is not TextEditorViewModel vm)
            return;

        if (_activeMarker == null)
            return;



        Editor.Document.Replace(
        _activeMarker.StartOffset,
        _activeMarker.Length,
        suggestion
        );

        _markerService.Remove(_activeMarker);
        Editor.TextArea.TextView.Redraw();


        vm.DocumentText = Editor.Text;
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
        _markerService.LoadSquigglies(vm.DocumentText, vm.Errors ?? []);
        Editor.TextArea.TextView.Redraw();
    }
}