using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Info
{
    public static void InfoCommand(string param)
    {
        var filePath = param;
        try
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            var input = fileBytes;
            var decoded = Bencode.Decode(ref input) as Dictionary<string, object>;

            if (decoded == null)
            {
                Console.WriteLine("Error: Decoding failed.");
                return;
            }

            if (decoded.TryGetValue("announce", out var announceObj) &&
                decoded.TryGetValue("info", out var infoObj))
            {
                string? announce = announceObj as string;
                var infoDict = infoObj as Dictionary<string, object>;

                if (infoDict != null)
                {
                    if (infoDict.TryGetValue("length", out var lengthObj))
                    {
                        if (lengthObj is long length)
                        {
                            Console.WriteLine($"Tracker URL: {announce}");
                            Console.WriteLine($"Length: {length}");

                            byte[] bencodedInfo = Bencode.Encode(infoDict);

                            string infoHash = ComputeSha1Hash(bencodedInfo);

                            Console.WriteLine($"Info Hash: {infoHash}");
                        }
                        else
                        {
                            Console.WriteLine("Error: 'length' key is not of type long.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: 'length' key not found in 'info' dictionary.");
                    }
                }
                else
                {
                    Console.WriteLine("Error: 'info' key is not a dictionary.");
                }
            }
            else
            {
                Console.WriteLine("Error: 'announce' or 'info' key not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static byte[] BencodeEncode(Dictionary<string, object> dict)
    {
        return Bencode.Encode(dict);
    }

    private static string ComputeSha1Hash(byte[] data)
    {
        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] hash = sha1.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
