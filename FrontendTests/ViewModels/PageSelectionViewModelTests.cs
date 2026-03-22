namespace FrontendTests.Unit.ViewModels;

public class PageSelectionViewModelTests
{
    private PageSelectionViewModel CreateVM(
        Mock<IPathService>? path = null,
        Mock<IDialogService>? dialog = null,
        Mock<IToastService>? toast = null)
    {
        path ??= new Mock<IPathService>();
        dialog ??= new Mock<IDialogService>();
        toast ??= new Mock<IToastService>();

        // Only set a default if the test didn't configure PageFilePaths
        if (!path.Setups.Any(s => s.Expression.ToString().Contains("PageFilePaths")))
        {
            path.Setup(x => x.PageFilePaths).Returns(Array.Empty<string>());
        }


        var app = new Mock<IAppServices>();
        app.Setup(x => x.PathService).Returns(path.Object);
        app.Setup(x => x.DialogService).Returns(dialog.Object);
        app.Setup(x => x.ToastService).Returns(toast.Object);

        return new PageSelectionViewModel(_ => { }, app.Object);
    }

    [Fact]
    public void FilterText_ShouldFilterPagesCorrectly()
    {
        var path = new Mock<IPathService>();
        path.Setup(x => x.PageFilePaths)
            .Returns(new[] { "document.docx", "bigDocument.docx", "veryBigDocument.docx" });

        var vm = CreateVM(path);

        vm.FilterText = "big";

        // only bigDocument, veryBigDocument contain "big"
        vm.FilteredPages.Should().HaveCount(2);
        vm.FilteredPages.Select(p => p.Title)
            .Should().Contain(new[] { "bigDocument", "veryBigDocument" });
    }

    [Fact]
    public void FilterText_Empty_ShouldResetFilteredPages()
    {
        var path = new Mock<IPathService>();
        path.Setup(x => x.PageFilePaths)
            .Returns(new[] { "One.docx", "Two.docx" });

        var vm = CreateVM(path);

        vm.FilterText = "o";

        // "One" and "Two" both contain "o"
        vm.FilteredPages.Should().HaveCount(2);

        vm.FilterText = "";
        vm.FilteredPages.Should().HaveCount(2);
    }

    [Fact]
    public void RefreshPages_ShouldReloadPages_WhenStorageRaisesEvent()
    {
        var path = new Mock<IPathService>();
        path.SetupSequence(x => x.PageFilePaths)
            .Returns(new[] { "One.docx" })
            .Returns(new[] { "One.docx", "Two.docx" });

        var vm = CreateVM(path);

        // Trigger the event
        path.Raise(x => x.PagePathsChanged += null);

        vm.AllPages.Should().HaveCount(2);
    }

    [Fact]
    public void CreateNewDocument_ShouldAddPage_WhenDialogReturnsName()
    {
        var path = new Mock<IPathService>();
        var dialog = new Mock<IDialogService>();
        var toast = new Mock<IToastService>();

        dialog.Setup(x => x.ShowAsync<string?>(It.IsAny<DialogViewModelBase<string?>>()))
              .ReturnsAsync("NewDoc");

        path.Setup(x => x.CreatePageFilePath("NewDoc"))
            .Returns("C:/Docs/NewDoc.docx");

        var vm = CreateVM(path, dialog, toast);

        vm.CreateNewDocumentCommand.Execute().Subscribe();

        path.Verify(x => x.AddPage("C:/Docs/NewDoc.docx"), Times.Once);
        toast.Verify(x => x.CreateAndShowInfoToast(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void CreateNewDocument_ShouldNotAddPage_WhenDialogReturnsNull()
    {
        var path = new Mock<IPathService>();
        var dialog = new Mock<IDialogService>();
        var toast = new Mock<IToastService>();

        dialog.Setup(x => x.ShowAsync<string?>(It.IsAny<DialogViewModelBase<string?>>()))
              .ReturnsAsync((string?)null);

        var vm = CreateVM(path, dialog, toast);

        vm.CreateNewDocumentCommand.Execute().Subscribe();

        path.Verify(x => x.AddPage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void RenamePage_ShouldRenameFile_WhenDialogReturnsNewName()
    {
        var path = new Mock<IPathService>();
        var dialog = new Mock<IDialogService>();
        var toast = new Mock<IToastService>();

        dialog.Setup(x => x.ShowAsync<string?>(It.IsAny<DialogViewModelBase<string?>>()))
              .ReturnsAsync("Renamed");

        var vm = CreateVM(path, dialog, toast);

        var rename = typeof(PageSelectionViewModel)
            .GetMethod("RenamePage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        rename.Invoke(vm, new object[] { "C:/Docs/Old.docx" });

        path.Verify(x => x.RenamePage(
            "C:/Docs/Old.docx",
            It.Is<string>(s => s.EndsWith("Renamed.docx"))
        ), Times.Once);

        toast.Verify(x => x.CreateAndShowInfoToast(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void DeletePage_ShouldCallStorageAndToast()
    {
        var path = new Mock<IPathService>();
        var toast = new Mock<IToastService>();

        var vm = CreateVM(path, null, toast);

        var delete = typeof(PageSelectionViewModel)
            .GetMethod("DeletePage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        delete.Invoke(vm, new object[] { "C:/Docs/ToDelete.docx" });

        path.Verify(x => x.DeletePage("C:/Docs/ToDelete.docx"), Times.Once);
        toast.Verify(x => x.CreateAndShowInfoToast(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void WrapWidth_ShouldRaisePropertyChanged()
    {
        var vm = CreateVM();

        double observed = -1;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PageSelectionViewModel.WrapWidth))
                observed = vm.WrapWidth;
        };

        vm.WrapWidth = 123;

        observed.Should().Be(123);
    }
}