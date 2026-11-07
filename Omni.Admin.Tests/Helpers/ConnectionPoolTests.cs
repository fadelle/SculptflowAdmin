using Omni.Admin.Persistence.Helpers;

namespace Omni.Admin.Tests.Helpers;

// QA t3-inbox-02: the portal shares the Supabase pooler's client limit (15) with the main app, so it opens few connections.
public class ConnectionPoolTests
{
    [Fact]
    public void The_pool_cap_is_added_unless_the_connection_string_sets_one()
    {
        Assert.Contains("Maximum Pool Size=3", ConnectionPool.WithPoolLimit("Host=db;Username=u", 3));
        Assert.Equal("Host=db;MaxPoolSize=2", ConnectionPool.WithPoolLimit("Host=db;MaxPoolSize=2", 3));
        Assert.Equal("Host=db", ConnectionPool.WithPoolLimit("Host=db", null));
        Assert.Throws<InvalidOperationException>(() => ConnectionPool.WithPoolLimit("Host=db", -1));
    }
}
