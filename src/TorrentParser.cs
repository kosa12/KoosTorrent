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
                                piecesBytes = Encoding.ASCII.GetBytes(piecesString);
                            }
                            else
                            {
                                Console.WriteLine("Error: 'pieces' key is not of the expected type.");
                                return;
                            }

                            string piecesHexString = BitConverter.ToString(piecesBytes).Replace("-", "").ToLower();

                            Console.WriteLine($"{Red}Tracker URL:{Reset} {Green}{announce}{Reset}");
                            Console.WriteLine($"{Red}Length:{Reset} {Green}{length}{Reset}");
                            Console.WriteLine($"{Red}Info Hash:{Reset} {Green}{ComputeSha1Hash(Bencode.Encode(infoDict))}{Reset}");
                            Console.WriteLine($"{Red}Piece Length:{Reset} {Green}{pieceLength}{Reset}");
                            Console.WriteLine($"{Red}Piece Hashes:{Reset}");

                            PrintPieceHashes(piecesHexString);
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

    private static void PrintPieceHashes(string piecesHexString)
    {
        int hashLength = 40;
        for (int i = 0; i < piecesHexString.Length; i += hashLength)
        {
            if (i + hashLength <= piecesHexString.Length)
            {
                string hashHex = piecesHexString.Substring(i, hashLength);
                Console.WriteLine($"{Green}{hashHex.ToUpper()}{Reset}");
            }
            else
            {
                Console.WriteLine("Error: Incomplete piece hash detected.");
                break;
            }
        }
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
