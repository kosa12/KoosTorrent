using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Info
{
    private const string Red = "\x1b[31m";
    private const string Green = "\x1b[32m";
    private const string Reset = "\x1b[0m";

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

        Console.WriteLine("Decoded dictionary:");
        foreach (var kvp in decoded)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }

        if (decoded.TryGetValue("announce", out var announceObj) &&
            decoded.TryGetValue("info", out var infoObj))
        {
            string? announce = announceObj as string;
            var infoDict = infoObj as Dictionary<string, object>;

            if (infoDict != null)
            {
                if (infoDict.TryGetValue("length", out var lengthObj) &&
                    infoDict.TryGetValue("piece length", out var pieceLengthObj) &&
                    infoDict.TryGetValue("pieces", out var piecesObj))
                {
                    if (lengthObj is long length && pieceLengthObj is long pieceLength)
                    {
                        byte[] piecesBytes;

                        if (piecesObj is byte[] piecesArray)
                        {
                            piecesBytes = piecesArray;
                        }
                        else if (piecesObj is string piecesString)
                        {
                            piecesBytes = Encoding.ASCII.GetBytes(piecesString); // Handle if it's base64 encoded
                        }
                        else
                        {
                            Console.WriteLine("Error: 'pieces' key is not of the expected type.");
                            return;
                        }

                        // Create TorrentInfo instance and assign Pieces field
                        var torrentInfo = new TorrentInfo { Pieces = piecesBytes };

                        Console.WriteLine($"{Red}Tracker URL:{Reset} {Green}{announce}{Reset}");
                        Console.WriteLine($"{Red}Length:{Reset} {Green}{length}{Reset}");
                        Console.WriteLine($"{Red}Info Hash:{Reset} {Green}{ComputeSha1Hash(Bencode.Encode(infoDict))}{Reset}");
                        Console.WriteLine($"{Red}Piece Length:{Reset} {Green}{pieceLength}{Reset}");
                        Console.WriteLine($"{Red}Piece Hashes:{Reset}");

                        // Print each piece hash
                        foreach (var hash in torrentInfo.PieceHashes)
                        {
                            Console.WriteLine($"{Green}{hash}{Reset}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: One or more keys are not of the expected type.");
                    }
                }
                else
                {
                    Console.WriteLine("Error: One or more required keys not found in 'info' dictionary.");
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




    private static void PrintPieceHashes(byte[] piecesBytes)
    {
        int hashLength = 20;

        for (int i = 0; i < piecesBytes.Length; i += hashLength)
        {
            if (i + hashLength <= piecesBytes.Length)
            {
                byte[] hashBytes = piecesBytes.Skip(i).Take(hashLength).ToArray();
                string hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                Console.WriteLine($"{Green}{hashHex}{Reset}");
            }
            else
            {
                Console.WriteLine("Error: Incomplete piece hash detected.");
                break;
            }
        }
    }


    private static string ConvertBytesToHexString(byte[] bytes)
    {
        var hexString = BitConverter.ToString(bytes).Replace("-", "").ToLower();
        return hexString;
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


public class TorrentInfo
{
    public byte[] Pieces { get; set; }

    public string[] PieceHashes
    {
        get
        {
            string[] pieceHashes = new string[Pieces.Length / 20];
            for (int i = 0; i < Pieces.Length; i += 20)
            {
                byte[] pieceHash = new byte[20];
                int length = Math.Min(20, Pieces.Length - i);
                Array.Copy(Pieces, i, pieceHash, 0, length);
                pieceHashes[i / 20] = BitConverter.ToString(pieceHash)
                                      .Replace("-", string.Empty)
                                      .ToLower();
            }
            return pieceHashes;
        }
    }
}
