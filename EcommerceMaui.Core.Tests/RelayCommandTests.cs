using EcommerceMaui.ViewModels;
using Xunit;

namespace EcommerceMaui.Core.Tests;

public class RelayCommandTests
{
    [Fact]
    public void CanExecute_returns_true_when_no_predicate_supplied()
    {
        var command = new RelayCommand(() => { });

        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public void CanExecute_respects_predicate()
    {
        var allowed = false;
        var command = new RelayCommand(() => { }, () => allowed);

        Assert.False(command.CanExecute(null));
        allowed = true;
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public async Task Execute_invokes_action_when_can_execute()
    {
        var executed = false;
        var command = new RelayCommand(() => executed = true);

        command.Execute(null);
        await Task.Delay(50);

        Assert.True(executed);
    }

    [Fact]
    public async Task Execute_does_nothing_when_can_execute_false()
    {
        var executed = false;
        var command = new RelayCommand(() => executed = true, () => false);

        command.Execute(null);
        await Task.Delay(50);

        Assert.False(executed);
    }

    [Fact]
    public void RaiseCanExecuteChanged_notifies_listeners()
    {
        var command = new RelayCommand(() => { });
        var fired = 0;
        command.CanExecuteChanged += (_, _) => fired++;

        command.RaiseCanExecuteChanged();

        Assert.Equal(1, fired);
    }

    [Fact]
    public async Task Async_command_executes_async_function()
    {
        var executed = false;
        var command = new RelayCommand(async () =>
        {
            await Task.Yield();
            executed = true;
        });

        command.Execute(null);
        await Task.Delay(50);

        Assert.True(executed);
    }

    [Fact]
    public async Task Generic_command_passes_parameter()
    {
        string? received = null;
        var command = new RelayCommand<string>(s => received = s);

        command.Execute("hello");
        await Task.Delay(50);

        Assert.Equal("hello", received);
    }
}
