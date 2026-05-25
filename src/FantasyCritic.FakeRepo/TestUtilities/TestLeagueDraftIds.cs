using System;
using System.Security.Cryptography;
using System.Text;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.FakeRepo.TestUtilities;

public static class TestLeagueDraftIds
{
    private static readonly Guid NamespaceId = new("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

    public static Guid For(LeagueYearKey key) => For(key.LeagueID, key.Year);

    public static Guid For(Guid leagueID, int year)
    {
        var name = $"{leagueID:N}|{year}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(name));
        var bytes = new byte[16];
        Array.Copy(hash, bytes, 16);
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x30);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        return new Guid(bytes);
    }
}
