using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rop.Database10.Tracking;
public readonly record struct TrackingVersion
{
    public required long Version { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public long Ticks => Timestamp.Ticks;
    public static TrackingVersion MinValue => new TrackingVersion { Version = 0, Timestamp = DateTimeOffset.MinValue };
}
