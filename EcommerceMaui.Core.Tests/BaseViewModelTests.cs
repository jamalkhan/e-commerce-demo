using EcommerceMaui.ViewModels;
using Xunit;

namespace EcommerceMaui.Core.Tests;

public class BaseViewModelTests
{
    private sealed class Sample : BaseViewModel { }

    [Fact]
    public void IsBusy_setter_flips_IsNotBusy()
    {
        var vm = new Sample { IsBusy = false };
        Assert.True(vm.IsNotBusy);

        vm.IsBusy = true;
        Assert.False(vm.IsNotBusy);
    }

    [Fact]
    public void IsBusy_setter_raises_PropertyChanged_for_both_flags()
    {
        var vm = new Sample();
        var fired = new List<string?>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName);

        vm.IsBusy = true;

        Assert.Contains(nameof(BaseViewModel.IsBusy), fired);
        Assert.Contains(nameof(BaseViewModel.IsNotBusy), fired);
    }

    [Fact]
    public void HasError_is_false_when_ErrorMessage_null_or_empty()
    {
        var vm = new Sample();
        Assert.False(vm.HasError);

        vm.ErrorMessage = string.Empty;
        Assert.False(vm.HasError);
    }

    [Fact]
    public void HasError_is_true_when_ErrorMessage_set()
    {
        var vm = new Sample { ErrorMessage = "boom" };
        Assert.True(vm.HasError);
    }

    [Fact]
    public void Setting_ErrorMessage_raises_HasError_PropertyChanged()
    {
        var vm = new Sample();
        var fired = new List<string?>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName);

        vm.ErrorMessage = "boom";

        Assert.Contains(nameof(BaseViewModel.HasError), fired);
    }
}
