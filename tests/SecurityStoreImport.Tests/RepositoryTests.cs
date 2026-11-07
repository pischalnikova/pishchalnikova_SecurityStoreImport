namespace SecurityStoreImport.Tests;

public class RepositoryTests
{
    [Fact]
    public void Add_NewSku_ProductIsStored()
    {
        var repository = new InMemoryProductRepository();

        repository.Add(TestHelpers.MakeProduct("A-1"));

        Assert.Equal(1, repository.Count);
        Assert.NotNull(repository.GetBySku("A-1"));
    }

    [Fact]
    public void Exists_IgnoresCase()
    {
        var repository = new InMemoryProductRepository();
        repository.Add(TestHelpers.MakeProduct("grd-1"));

        Assert.True(repository.Exists("GRD-1"));
    }

    [Fact]
    public void Add_Duplicate_Throws()
    {
        var repository = new InMemoryProductRepository();
        repository.Add(TestHelpers.MakeProduct("A-1"));

        Assert.Throws<InvalidOperationException>(() => repository.Add(TestHelpers.MakeProduct("A-1")));
    }

    [Fact]
    public void GetAll_KeepsInsertionOrder()
    {
        var repository = new InMemoryProductRepository();
        foreach (var sku in new[] { "C", "A", "B" })
            repository.Add(TestHelpers.MakeProduct(sku));

        Assert.Equal(new[] { "C", "A", "B" }, repository.GetAll().Select(p => p.Sku));
    }

    [Fact]
    public void GetBySku_Unknown_ReturnsNull()
    {
        var repository = new InMemoryProductRepository();

        Assert.Null(repository.GetBySku("нет"));
        Assert.Null(repository.GetBySku(""));
    }

    [Fact]
    public void Clear_RemovesEverything()
    {
        var repository = new InMemoryProductRepository();
        repository.Add(TestHelpers.MakeProduct("A-1"));

        repository.Clear();

        Assert.Equal(0, repository.Count);
        Assert.False(repository.Exists("A-1"));
    }
}
