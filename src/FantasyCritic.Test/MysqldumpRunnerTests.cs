using System;
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

    [TestCase(MySqlSslMode.None, "DISABLED")]
    [TestCase(MySqlSslMode.Preferred, "PREFERRED")]
    [TestCase(MySqlSslMode.Required, "REQUIRED")]
    [TestCase(MySqlSslMode.VerifyCA, "VERIFY_CA")]
    [TestCase(MySqlSslMode.VerifyFull, "VERIFY_IDENTITY")]
    public void ToClientSslMode_MatchesTheConnectionString(MySqlSslMode sslMode, string expected)
    {
        Assert.That(MysqldumpRunner.ToClientSslMode(sslMode), Is.EqualTo(expected));
    }
}
