using System;
using Avalonia.Controls;
using Avalonia.Input;
using ClearText.ViewModels;

namespace ClearText.Views;

public partial class PageView : UserControl
{
    public PageView()
    {
        InitializeComponent();
    }

    private void OnPageClicked(object sender, PointerPressedEventArgs e)
    {
        //Don't openeditor if user clicks on meatball menu button
        if (e.Source is Button)
            return;

        if (DataContext is PageViewModel vm)
            vm.OpenEditorCommand.Execute().Subscribe();
    }
}