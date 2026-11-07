using System.Data.Common;
using System.Globalization;

namespace Omni.Admin.Persistence.Helpers;

/// <summary>
/// Caps how many database connections the portal opens at once (QA t3-inbox-02). The portal shares Supabase's session pooler
/// (pool_size clients per database user, 15 on the current plan) with the main app, so it keeps a small cap (Database:MaxPoolSize,
/// 3: it only writes its audit log) and the main app's cap plus this one stay below the pooler's limit. A full pool waits for a
/// free connection instead of failing. Same rule as the main app's Persistence/Helpers/ConnectionPool.
/// </summary>
public static class ConnectionPool
{
    private static readonly string[] MaxPoolSizeKeys = ["Maximum Pool Size", "Max Pool Size", "MaxPoolSize"];

    /// <summary>The connection string with <c>Maximum Pool Size</c> set to <paramref name="maxPoolSize"/>, unless it already
    /// sets one (an explicit value in the connection string wins) or no cap is configured.</summary>
    public static string WithPoolLimit(string connectionString, int? maxPoolSize)
    {
        if (maxPoolSize is not int max) return connectionString;
        if (max < 1) throw new InvalidOperationException("Database:MaxPoolSize must be at least 1.");

        // Parsed only to look for an existing cap; the string itself is kept as written.
        var parsed = new DbConnectionStringBuilder { ConnectionString = connectionString };
        if (MaxPoolSizeKeys.Any(parsed.ContainsKey)) return connectionString;
        return connectionString.TrimEnd().TrimEnd(';') + ";Maximum Pool Size=" + max.ToString(CultureInfo.InvariantCulture);
    }
}
