namespace ScoutHub.Data.Tests;

[TestFixture]
public class PlaceholderDataTests
{
    [Test]
    public void TrueIsTrue()
    {
        const bool actual = true;
        Assert.That(actual, Is.True);
    }
}
