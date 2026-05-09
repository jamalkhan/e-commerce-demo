using System.ComponentModel;
using EcommerceMaui.ViewModels;
using Xunit;

namespace EcommerceMaui.Core.Tests;

public class ObservableObjectTests
{
    private sealed class Sample : ObservableObject
    {
        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool TrySet(string newValue) => SetProperty(ref _name, newValue);
    }

    [Fact]
    public void SetProperty_raises_PropertyChanged_when_value_changes()
    {
        var sample = new Sample();
        var fired = new List<string?>();
        sample.PropertyChanged += (_, e) => fired.Add(e.PropertyName);

        sample.Name = "Jamal";

        Assert.Single(fired);
        Assert.Equal(nameof(Sample.Name), fired[0]);
    }

    [Fact]
    public void SetProperty_does_not_raise_when_value_is_same()
    {
        var sample = new Sample { Name = "Jamal" };
        var fired = 0;
        sample.PropertyChanged += (_, _) => fired++;

        sample.Name = "Jamal";

        Assert.Equal(0, fired);
    }

    [Fact]
    public void SetProperty_returns_true_when_value_changed()
    {
        var sample = new Sample();
        Assert.True(sample.TrySet("changed"));
    }

    [Fact]
    public void SetProperty_returns_false_when_value_did_not_change()
    {
        var sample = new Sample { Name = "x" };
        Assert.False(sample.TrySet("x"));
    }
}
