using System;
using Avalonia;
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

        //Attach TextEditor for underline Squigglies
        _markerService = new TextMarkerService(Editor.Document);

        Editor.TextArea.TextView.BackgroundRenderers.Add(_markerService);



        // When DataContext changes, sync VM → Editor
        _ = this.GetObservable(DataContextProperty)
    .Subscribe(dc =>
    {
        if (dc is not TextEditorViewModel vm) return;

        //Text Loaded, can now load squigglies
        Editor.Text = vm.DocumentText;

        LoadSquigglies();


        Editor.TextChanged += (_, _) =>
        {
            if (Editor.Text != vm.DocumentText)
                vm.DocumentText = Editor.Text;
            LoadSquigglies();
        };

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.DocumentText) &&
                Editor.Text != vm.DocumentText)
            {
                Editor.Text = vm.DocumentText;
            }
        };
    });
    }

    private void LoadSquigglies()
    {
        _markerService.ClearMarkers();

        _markerService.LoadSquigglies(Editor.Text);

        Editor.TextArea.TextView.InvalidateVisual();
    }
}