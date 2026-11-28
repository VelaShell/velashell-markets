using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using VelaShell.Market.Api.Endpoints;
using VelaShell.Market.Domain;

namespace VelaShell.Market.Tests;

[TestClass]
public class PluginSearchFilterTests
{
    private static BsonDocument CatalogFilter(string[]? installedIds) =>
        PluginEndpoints.BuildSearchFilter("redis", "TOOLS", 2, true, installedIds)
                       .Render(new(BsonSerializer.LookupSerializer<Plugin>(), BsonSerializer.SerializerRegistry));

    [TestMethod]
    public void OmittedEmptyOrBlankIds_DoNotRestrictTheCatalog()
    {
        BsonDocument catalog = CatalogFilter(null);
        Assert.IsFalse(catalog.Contains("_id"));
        Assert.AreEqual(catalog, CatalogFilter([]));
        Assert.AreEqual(catalog, CatalogFilter(["", "  "]));
    }

    [TestMethod]
    public void InstalledIds_ExcludeMatchingPluginIdsFromTheCatalog()
    {
        BsonDocument filter = CatalogFilter(["acme.redis", "", " acme.ssh ", "acme.redis"]);
        var expected = new BsonDocument("$nin", new BsonArray { "acme.redis", "acme.ssh" });
        Assert.AreEqual(expected, filter["_id"]);
        filter.Remove("_id");
        Assert.AreEqual(CatalogFilter(null), filter);
    }
}
