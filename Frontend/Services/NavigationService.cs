using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ClearText.BaseTypes;
using ClearText.Interfaces;
using ClearText.ViewModels;
using ClearText.ViewModels.Toolbar;
using ReactiveUI;

namespace ClearText.Services;

// Service responsible for handling navigation between main and toolbar ViewModels
// Responsible for disposing old ViewModels and wiring up values between the Toolbar and Main content
public sealed class NavigationService(
    IUiHost host,
    Func<string, TextEditorViewModel> editorFactory,
    Func<PageSelectionViewModel> selectionFactory,
    Func<DashboardToolbarViewModel> dashboardToolbarFactory,
    Func<string, EditorToolbarViewModel> editorToolbarFactory) : BaseService, INavigationService
{
    private readonly CompositeDisposable _navigationScope = [];

    public void ShowDashboard()
    {
        ResetScope();

        var dashboardToolbar = dashboardToolbarFactory();
        var pageSelection = selectionFactory();

        WireDashboardSearch(
            dashboardToolbar,
            pageSelection);

        host.Toolbar = dashboardToolbar;
        host.MainContent = pageSelection;
    }

    public void ShowEditor(string path)
    {
        ResetScope();

        host.Toolbar = editorToolbarFactory(path);
        host.MainContent = editorFactory(path);
    }


    private void ResetScope()
    {
        _navigationScope.Clear();

        if (host.MainContent is IDisposable content)
            content.Dispose();

        if (host.Toolbar is IDisposable toolbar)
            toolbar.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        ResetScope();
        _navigationScope.Dispose();
        base.Dispose(disposing);
    }

    private void WireDashboardSearch(
    DashboardToolbarViewModel toolbar,
    PageSelectionViewModel pageSelection)
    {
        toolbar
            .WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(text => pageSelection.FilterText = text)
            .DisposeWith(_navigationScope);
    }

}