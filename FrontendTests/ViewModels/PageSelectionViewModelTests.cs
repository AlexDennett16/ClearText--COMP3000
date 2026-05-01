namespace FrontendTests.ViewModels;

public class PageSelectionViewModelTests
{
    // ReSharper disable once InconsistentNaming
    private static (
        PageSelectionViewModel vm,
        Mock<IPathService> path,
        Mock<IDialogService> dialog,
        Mock<IToastService> toast,
        Mock<ICreateNewDocumentDialogFactory> createFactory,
        Mock<IConfirmCancelDialogFactory> confirmFactory,
        Mock<IStringDialogFactory> stringFactory
    ) CreateVM(
        Action<Mock<IPathService>>? pathSetup = null,
        Action<Mock<IDialogService>>? dialogSetup = null,
        Action<Mock<IToastService>>? toastSetup = null)
    {
        var path = new Mock<IPathService>();
        var dialog = new Mock<IDialogService>();
        var toast = new Mock<IToastService>();
        var confirmFactory = new Mock<IConfirmCancelDialogFactory>();
        var createFactory = new Mock<ICreateNewDocumentDialogFactory>();
        var stringFactory = new Mock<IStringDialogFactory>();

        pathSetup?.Invoke(path);
        dialogSetup?.Invoke(dialog);
        toastSetup?.Invoke(toast);

        if (pathSetup == null)
        {
            path.Setup(x => x.PageFilePaths).Returns([]);
        }

        var vm = new PageSelectionViewModel(
            _ => { },
            toast.Object,
            path.Object,
            dialog.Object,
            createFactory.Object,
            stringFactory.Object,
            confirmFactory.Object
        );

        return (vm, path, dialog, toast, createFactory, confirmFactory, stringFactory);
    }

    [Fact]
    public void FilterText_ShouldFilterPagesCorrectly()
    {
        var (vm, _, _, _, _, _, _) = CreateVM(
            pathSetup: p => p.Setup(x => x.PageFilePaths)
                             .Returns(
                             [
                             "document.docx",
                             "bigDocument.docx",
                             "veryBigDocument.docx"
                             ])
        );

        vm.FilterText = "big";

        vm.FilteredPages.Should().HaveCount(2);
        vm.FilteredPages.Select(p => p.Title)
            .Should().Contain(["bigDocument", "veryBigDocument"]);
    }


    [Fact]
    public void FilterText_Empty_ShouldResetFilteredPages()
    {
        var (vm, _, _, _, _, _, _) = CreateVM(
            pathSetup: p => p.Setup(x => x.PageFilePaths)
                             .Returns(["One.docx", "Two.docx"])
        );

        vm.FilterText = "Two";
        vm.FilteredPages.Should().HaveCount(1);

        vm.FilterText = "";
        vm.FilteredPages.Should().HaveCount(2);
    }

    [Fact]
    public void RefreshPages_ShouldReloadPages_WhenStorageRaisesEvent()
    {

        var (vm, path, _, _, _, _, _) = CreateVM();

        path.SetupSequence(x => x.PageFilePaths)
            .Returns(["One.docx"])
            .Returns(["One.docx", "Two.docx"]);

        // Trigger the event twice
        path.Raise(x => x.PagePathsChanged += null);
        path.Raise(x => x.PagePathsChanged += null);

        vm.AllPages.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateNewDocument_ShouldAddPage_WhenDialogReturnsName()
    {
        var (vm, path, _, toast, createFactory, _, _) =
            CreateVM(
                dialogSetup: d =>
                    d.Setup(x => x.ShowAsync(It.IsAny<DialogViewModelBase<string?>>()))
                     .ReturnsAsync("C:/Docs/NewDoc.docx")
            );

        createFactory
            .Setup(f => f.Create(It.IsAny<string>()))
            .Returns(It.IsAny<CreateNewDocumentDialogViewModel>());

        await vm.CreateNewDocumentCommand.Execute();

        path.Verify(x => x.AddPage("C:/Docs/NewDoc.docx"), Times.Once);
        toast.Verify(x => x.CreateAndShowInfoToast(It.IsAny<string>(), null), Times.Once);
    }

    [Fact]
    public void CreateNewDocument_ShouldNotAddPage_WhenDialogReturnsNull()
    {
        var (vm, path, dialog, _, _, _, _) = CreateVM();

        dialog.Setup(x => x.ShowAsync(It.IsAny<DialogViewModelBase<string?>>()))
              .ReturnsAsync((string?)null);



        vm.CreateNewDocumentCommand.Execute().Subscribe();

        path.Verify(x => x.AddPage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void RenamePage_ShouldRenameFile_WhenDialogReturnsNewName()
    {
        var (vm, path, dialog, toast, _, _, _) = CreateVM();

        dialog.Setup(x => x.ShowAsync(It.IsAny<DialogViewModelBase<string?>>()))
              .ReturnsAsync("Renamed");

        var rename = typeof(PageSelectionViewModel)
            .GetMethod("RenamePage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        rename.Invoke(vm, ["C:/Docs/Old.docx"]);

        path.Verify(x => x.RenamePage(
            "C:/Docs/Old.docx",
            It.Is<string>(s => s.EndsWith("Renamed.docx"))
        ), Times.Once);

        toast.Verify(x => x.CreateAndShowInfoToast(It.IsAny<string>(), null), Times.Once);
    }

    [Fact]
    public void DeletePage_ShouldCallStorageAndToast()
    {

        var (vm, path, dialog, toast, _, _, _) = CreateVM();

        var delete = typeof(PageSelectionViewModel)
            .GetMethod("DeletePage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;


        dialog
            .Setup(x => x.ShowAsync(It.IsAny<DialogViewModelBase<bool?>>()))
            .ReturnsAsync(true);


        delete.Invoke(vm, ["C:/Docs/ToDelete.docx"]);

        path.Verify(x => x.DeletePage("C:/Docs/ToDelete.docx"), Times.Once);
        toast.Verify(x => x.CreateAndShowInfoToast(It.IsAny<string>(), null), Times.Once);
    }

    [Fact]
    public void WrapWidth_ShouldRaisePropertyChanged()
    {
        var (vm, _, _, _, _, _, _) = CreateVM();

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