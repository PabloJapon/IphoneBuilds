using System.Collections.Generic;

public static class Cp858
{
    private static readonly Dictionary<char, byte> map = new Dictionary<char, byte>
    {
        {'á', 0xA0}, {'é', 0x82}, {'í', 0xA1}, {'ó', 0xA2}, {'ú', 0xA3},
        {'ñ', 0xA4}, {'Ñ', 0xA5},
        {'Á', 0xB5}, {'É', 0x90}, {'Í', 0xD6}, {'Ó', 0xE0}, {'Ú', 0xE9},
        {'ü', 0x81}, {'Ü', 0x9A},
        {'¿', 0xA8}, {'¡', 0xAD},
        {'€', 0xD5},
    };

    public static byte[] GetBytes(string s)
    {
        if (string.IsNullOrEmpty(s)) return new byte[0];
        var bytes = new byte[s.Length];
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            bytes[i] = c < 128 ? (byte)c
                     : map.TryGetValue(c, out byte b) ? b
                     : (byte)'?';
        }
        return bytes;
    }
}