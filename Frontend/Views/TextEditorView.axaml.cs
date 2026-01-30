using System;
using Avalonia;
using Avalonia.Controls;
using ClearText.ViewModels;

namespace ClearText.Views;

public partial class TextEditorView : UserControl
{
    public TextEditorView()
    {
        InitializeComponent();

        // When DataContext changes, sync VM → Editor
        _ = this.GetObservable(DataContextProperty)
    .Subscribe(dc =>
    {
        if (dc is not TextEditorViewModel vm) return;
        Editor.Text = vm.DocumentText;

        Editor.TextChanged += (_, _) =>
        {
            if (Editor.Text != vm.DocumentText)
                vm.DocumentText = Editor.Text;
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
}