using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Vendora.Infrastructure.Persistence;

/// <summary>
/// SQL Server's datetime2 has no timezone concept, so every DateTime read back from the DB comes
/// out as Kind=Unspecified - which System.Text.Json then serializes without a trailing "Z",
/// causing the browser to misread the value as local time instead of UTC. Every DateTime column
/// in this app is UTC by convention (the "...Utc" naming), so this stamps Kind=Utc back on read.
/// </summary>
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v,
        v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
