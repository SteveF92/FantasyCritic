using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.MySQL;
using MySqlConnector;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class MysqldumpRunnerTests
{
    [TestCase("plain", "\"plain\"")]
    [TestCase(@"back\slash", "\"back\\\\slash\"")]
    [TestCase("has#hash", "\"has#hash\"")]
    [TestCase("has'apostrophe#", "\"has'apostrophe#\"")]
    [TestCase("has\"quote#", "'has\"quote#'")]
    public void QuoteOptionValue_EscapesBackslashesAndUsesTheQuoteTheValueLacks(string password, string expected)
    {
        Assert.That(MysqldumpRunner.QuoteOptionValue(password), Is.EqualTo(expected));
    }

    [Test]
    public void QuoteOptionValue_RefusesBothKindsOfQuote()
    {
        Assert.Throws<ArgumentException>(() => MysqldumpRunner.QuoteOptionValue("both'\""));
    }

    [TestCase("/*!50013 DEFINER=`root`@`%` SQL SECURITY DEFINER */", "/*!50013 DEFINER=CURRENT_USER SQL SECURITY DEFINER */")]
    [TestCase("CREATE DEFINER=`root`@`%` PROCEDURE `sp_getleagueyear`(", "CREATE DEFINER=CURRENT_USER PROCEDURE `sp_getleagueyear`(")]
    [TestCase("/*!50003 CREATE*/ /*!50017 DEFINER=`fantasycritic-admin`@`%`*/ /*!50003 TRIGGER `t` BEFORE INSERT ON `x` FOR EACH ROW",
        "/*!50003 CREATE*/ /*!50017 DEFINER=CURRENT_USER*/ /*!50003 TRIGGER `t` BEFORE INSERT ON `x` FOR EACH ROW")]
    [TestCase("INSERT INTO `tbl_note` VALUES ('DEFINER=`root`@`%`');", "INSERT INTO `tbl_note` VALUES ('DEFINER=`root`@`%`');")]
    [TestCase("CREATE TABLE `tbl_user` (", "CREATE TABLE `tbl_user` (")]
    public void ReplaceDefiner_UsesTheImportingUserOutsideData(string line, string expected)
    {
        Assert.That(MysqldumpRunner.ReplaceDefiner(line), Is.EqualTo(expected));
    }

    [TestCase(MySqlSslMode.None, "DISABLED")]
    [TestCase(MySqlSslMode.Preferred, "PREFERRED")]
    [TestCase(MySqlSslMode.Required, "REQUIRED")]
    [TestCase(MySqlSslMode.VerifyCA, "VERIFY_CA")]
    [TestCase(MySqlSslMode.VerifyFull, "VERIFY_IDENTITY")]
    public void ToClientSslMode_MatchesTheConnectionString(MySqlSslMode sslMode, string expected)
    {
        Assert.That(MysqldumpRunner.ToClientSslMode(sslMode), Is.EqualTo(expected));
    }

    [TestCase("CREATE TABLE t (id int);\n-- Dump completed on 2026-10-03 12:00:00\n\n", true)]
    [TestCase("CREATE TABLE t (id int);\nINSERT INTO t VALUES (1", false)]
    [TestCase("", false)]
    public async Task GzipDumpIsComplete_RequiresTheCompletionLineLast(string contents, bool expected)
    {
        var path = Path.GetTempFileName();
        try
        {
            await using (var file = File.Create(path))
            await using (var gzip = new GZipStream(file, CompressionLevel.Fastest))
            {
                await gzip.WriteAsync(Encoding.UTF8.GetBytes(contents));
            }

            Assert.That(await MysqldumpRunner.GzipDumpIsComplete(path, CancellationToken.None), Is.EqualTo(expected));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
