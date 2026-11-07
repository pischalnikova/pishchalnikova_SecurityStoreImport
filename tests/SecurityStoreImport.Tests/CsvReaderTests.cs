using SecurityStoreImport.Services.Csv;

namespace SecurityStoreImport.Tests;

public class CsvReaderTests
{
    [Fact]
    public void ReadAll_SimpleLines_SplitsFieldsAndNumbersLines()
    {
        var records = CsvReader.ReadAll("a,b,c\r\n1,2,3\n", ',');

        Assert.Equal(2, records.Count);
        Assert.Equal(new[] { "1", "2", "3" }, records[1].Fields.ToArray());
        Assert.Equal(2, records[1].LineNumber);
    }

    [Fact]
    public void ReadAll_QuotedFieldWithDelimiterAndQuotes_IsOneValue()
    {
        var records = CsvReader.ReadAll("1,\"Контроллер \"\"Gate\"\", 2 двери\",x", ',');

        Assert.Equal(new[] { "1", "Контроллер \"Gate\", 2 двери", "x" }, records[0].Fields.ToArray());
    }

    [Fact]
    public void ReadAll_LineBreakInsideQuotes_KeepsRecordAndCountsLines()
    {
        var records = CsvReader.ReadAll("h1,h2\n1,\"две\nстроки\"\n2,x", ',');

        Assert.Equal(3, records.Count);
        Assert.Equal("две\nстроки", records[1].Fields[1]);
        Assert.Equal(4, records[2].LineNumber);
    }

    [Fact]
    public void ReadAll_QuoteInsideUnquotedValue_IsLiteral()
    {
        var records = CsvReader.ReadAll("1,Монитор 22\",3", ',');

        Assert.Equal("Монитор 22\"", records[0].Fields[1]);
    }

    [Theory]
    [InlineData("sku,name,price", ',')]
    [InlineData("sku;name;price", ';')]
    [InlineData("sku\tname\tprice", '\t')]
    [InlineData("\"a,b\";c;d", ';')]
    public void DetectDelimiter_PicksMostFrequent(string header, char expected)
    {
        Assert.Equal(expected, CsvReader.DetectDelimiter(header));
    }
}
