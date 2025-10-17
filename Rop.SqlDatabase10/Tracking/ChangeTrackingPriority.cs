using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rop.Database10.Tracking;

/// <summary>
/// Defines the priority levels for Change Tracking table monitoring.
/// Lower priority values mean more frequent checks (higher priority).
/// Interval = 2^Priority seconds.
/// </summary>
public enum ChangeTrackingPriority
{
    /// <summary>
    /// Maximum priority - Check every second
    /// Use for critical real-time data that must be immediately synchronized.
    /// </summary>
    Realtime = 0,

    /// <summary>
    /// Very high priority - Check every 2 seconds
    /// Use for highly critical transactional data.
    /// </summary>
    VeryHigh = 1,

    /// <summary>
    /// High priority - Check every 4 seconds
    /// Use for important data that changes frequently.
    /// </summary>
    High = 2,

    /// <summary>
    /// Medium-High priority - Check every 8 seconds
    /// Default priority. Good balance between performance and responsiveness.
    /// Use for standard transactional data.
    /// </summary>
    MediumHigh = 3,
    /// <summary>
    /// Default priority - Check every 16 seconds
    /// Good balance between performance and responsiveness.
    /// Use for standard transactional data.
    /// </summary>
    Default = Medium,
    /// <summary>
    /// Medium priority - Check every 16 seconds
    /// Use for data that changes moderately.
    /// </summary>
    Medium = 4,

    /// <summary>
    /// Medium-Low priority - Check every 32 seconds
    /// Use for reference data that changes infrequently.
    /// </summary>
    MediumLow = 5,

    /// <summary>
    /// Low priority - Check every 64 seconds
    /// Use for configuration or lookup data.
    /// </summary>
    Low = 6,

    /// <summary>
    /// Very low priority - Check every 128 seconds / ~2 minutes.
    /// Use for data that rarely changes.
    /// </summary>
    VeryLow = 7,

    /// <summary>
    /// Minimal priority - Check every 256 seconds / ~4 minutes.
    /// Use for static or historical data.
    /// </summary>
    Minimal = 8,

    /// <summary>
    /// Ultra-low priority - Check every 512 seconds / ~8.5 minutes.
    /// Use for archival or rarely accessed data.
    /// </summary>
    UltraLow = 9,

    /// <summary>
    /// Lowest priority - Check every 1024 seconds / ~17 minutes.
    /// Use for data that almost never changes or is updated on schedule.
    /// </summary>
    Lowest = 10
}

/// <summary>
/// Extension methods for ChangeTrackingPriority enum.
/// </summary>
public static class ChangeTrackingPriorityExtensions
{
    /// <summary>
    /// Converts priority to interval in seconds using exponential calculation (2^priority).
    /// </summary>
    public static TimeSpan ToInterval(this ChangeTrackingPriority priority)
    {
        return TimeSpan.FromSeconds(1 << (int)priority);
    }

    /// <summary>
    /// Gets the interval in seconds for the given priority.
    /// </summary>
    public static int ToSeconds(this ChangeTrackingPriority priority)
    {
        return 1 << (int)priority;
    }

    /// <summary>
    /// Gets a human-readable description of the priority and its interval.
    /// </summary>
    public static string GetDescription(this ChangeTrackingPriority priority)
    {
        var seconds = priority.ToSeconds();
        return priority switch
        {
            ChangeTrackingPriority.Realtime => $"Realtime ({seconds}s)",
            ChangeTrackingPriority.VeryHigh => $"Very High ({seconds}s)",
            ChangeTrackingPriority.High => $"High ({seconds}s)",
            ChangeTrackingPriority.MediumHigh => $"Medium-High ({seconds}s - Default)",
            ChangeTrackingPriority.Medium => $"Default / Medium ({seconds}s)",
            ChangeTrackingPriority.MediumLow => $"Medium-Low ({seconds}s)",
            ChangeTrackingPriority.Low => $"Low (~{seconds / 60}m)",
            ChangeTrackingPriority.VeryLow => $"Very Low (~{seconds / 60}m)",
            ChangeTrackingPriority.Minimal => $"Minimal (~{seconds / 60}m)",
            ChangeTrackingPriority.UltraLow => $"Ultra-Low (~{seconds / 60}m)",
            ChangeTrackingPriority.Lowest => $"Lowest (~{seconds / 60}m)",
            _ => $"Unknown ({seconds}s)"
        };
    }
}