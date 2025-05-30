using Microsoft.Extensions.DependencyInjection;

namespace UnitTest;

public static class Extensions
{
    public static T GetService<T>(this TestSetup testSetup) where T : class
    {
        return testSetup.ServiceProvider.GetRequiredService<T>();
    }
}
