using Bounteous.Data.Extensions.Utilities;
using Xunit;

namespace Bounteous.Data.Extensions.Tests;

public class ProductionDetectorTests
{
    [Fact]
    public void IsProductionEnvironment_ShouldReturnFalseInDebug()
    {
        // This test runs in Debug configuration, so should return false
        Assert.False(ProductionDetector.IsProductionEnvironment());
    }

    [Fact]
    public void ProductionWarningMarker_ShouldNotThrowInTestContext()
    {
        // Should not throw in test context even if Release configuration
        ProductionWarningMarker.ValidateContext();
        
        // If we get here, the validation passed
        Assert.True(true);
    }
}
