using Maui.RevenueCat.iOS;

namespace Uno.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class LogLevelExtensions
{
    internal static RCLogLevel ToRCLogLevel(this LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => RCLogLevel.Verbose,
            LogLevel.Debug => RCLogLevel.Debug,
            LogLevel.Information => RCLogLevel.Info,
            LogLevel.Warning => RCLogLevel.Warn,
            LogLevel.Error => RCLogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
    }
}
