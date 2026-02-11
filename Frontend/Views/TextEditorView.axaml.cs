using System;
using Avalonia.Controls;
using ClearText.Services;
using ClearText.ViewModels;

namespace ClearText.Views;

public partial class TextEditorView : UserControl
{
    TextMarkerService _markerService;

    public TextEditorView()
    {
        InitializeComponent();

        _markerService = new TextMarkerService(Editor.Document);
        Editor.TextArea.TextView.BackgroundRenderers.Add(_markerService);
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

    }

    private void LoadSquigglies(TextEditorViewModel vm)
    {
        _markerService.ClearMarkers();
        _markerService.LoadSquigglies(vm.DocumentText, vm.Errors ?? []);
        Editor.TextArea.TextView.InvalidateVisual();
    }
}