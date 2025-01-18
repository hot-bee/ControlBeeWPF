using JetBrains.Annotations;
using Xunit;

namespace ControlBeeWPF.Tests;

[TestSubject(typeof(Class1))]
public class Class1Test
{
    [Fact]
    public void FooTest()
    {
        Assert.Equal("foo", new Class1().Foo());
    }
}
