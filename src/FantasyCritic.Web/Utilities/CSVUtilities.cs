using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace FantasyCritic.Web.Utilities;

public class CSVUtilities
{
    public static MemoryStream GetCSVStream<T>(IEnumerable<T> records)
    {
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream);
        var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            ShouldQuote = _ => true
        });
        csvWriter.WriteRecords(records);
        streamWriter.Flush();
        memoryStream.Position = 0;
        return memoryStream;
    }
}
