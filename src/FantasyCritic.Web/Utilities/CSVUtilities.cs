using System.Globalization;
using System.IO;
using CsvHelper;

namespace FantasyCritic.Web.Utilities;

public class CSVUtilities
{
    public static MemoryStream GetCSVStream<T>(IEnumerable<T> records)
    {
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream);
        var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
        csvWriter.WriteRecords(records);
        streamWriter.Flush();
        memoryStream.Position = 0;
        return memoryStream;
    }
}
