using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

public class Tracker
{
    private const string PeerId = "00112233445566778899";
    private const int Port = 6881;
    private const int Uploaded = 0;
    private const int Downloaded = 0;

    public static void RequestPeers(string torrentFilePath)
    {
        try
        {
            byte[] fileBytes = File.ReadAllBytes(torrentFilePath);
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
                    string infoHash = ComputeSha1Hash(Bencode.Encode(infoDict));

                    string requestUrl = BuildRequestUrl(announce, infoHash, Port, Uploaded, Downloaded, LengthFromTorrentFile(torrentFilePath));

                    string response = SendRequest(requestUrl);
                    ProcessTrackerResponse(response);
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

    private static string BuildRequestUrl(string baseUrl, string infoHash, int port, int uploaded, int downloaded, long left)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["info_hash"] = infoHash;
        query["peer_id"] = PeerId;
        query["port"] = port.ToString();
        query["uploaded"] = uploaded.ToString();
        query["downloaded"] = downloaded.ToString();
        query["left"] = left.ToString();
        query["compact"] = "1";

        return $"{baseUrl}?{query}";
    }

    private static string SendRequest(string url)
    {
        using (var webClient = new WebClient())
        {
            return webClient.DownloadString(url);
        }
    }

    private static void ProcessTrackerResponse(string response)
    {
        var input = Encoding.ASCII.GetBytes(response);
        var decoded = Bencode.Decode(ref input) as Dictionary<string, object>;

        if (decoded != null && decoded.TryGetValue("peers", out var peersObj))
        {
            if (peersObj is byte[] peersBytes)
            {
                for (int i = 0; i < peersBytes.Length; i += 6)
                {
                    var ip = new IPAddress(peersBytes, i, 4).ToString();
                    var port = BitConverter.ToUInt16(peersBytes, i + 4);
                    Console.WriteLine($"{ip}:{port}");
                }
            }
            else
            {
                Console.WriteLine("Error: 'peers' key is not of the expected type.");
            }
        }
        else
        {
            Console.WriteLine("Error: 'peers' key not found in response.");
        }
    }

    private static string ComputeSha1Hash(byte[] data)
    {
        using (var sha1 = System.Security.Cryptography.SHA1.Create())
        {
            byte[] hash = sha1.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    private static long LengthFromTorrentFile(string torrentFilePath)
    {
        // Extract the length from the torrent file for 'left' parameter
        byte[] fileBytes = File.ReadAllBytes(torrentFilePath);
        var input = fileBytes;
        var decoded = Bencode.Decode(ref input) as Dictionary<string, object>;

        if (decoded != null && decoded.TryGetValue("info", out var infoObj))
        {
            var infoDict = infoObj as Dictionary<string, object>;
            if (infoDict != null && infoDict.TryGetValue("length", out var lengthObj) && lengthObj is long length)
            {
                return length;
            }
        }

        throw new InvalidOperationException("Could not extract file length from torrent.");
    }
}
