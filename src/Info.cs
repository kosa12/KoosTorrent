using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Info {
    public static void InfoCommand(string param) {
        var filePath = param;
        try {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            var input = fileBytes;
            var decoded = Bencode.Decode(ref input) as Dictionary<string, object>;

            if (decoded == null) {
                Console.WriteLine("Error: Decoding failed.");
                return;
            }

            if (decoded.TryGetValue("announce", out var announceObj) &&
                decoded.TryGetValue("info", out var infoObj)) {

                string? announce = announceObj as string;
                var infoDict = infoObj as Dictionary<string, object>;

                if (infoDict != null && infoDict.TryGetValue("length", out var lengthObj)) {
                    if (lengthObj is long length) {
                        Console.WriteLine($"Tracker URL: {announce}");
                        Console.WriteLine($"Length: {length}");
                    } else {
                        Console.WriteLine("Error: 'length' key is not of type long.");
                    }
                } else {
                    Console.WriteLine("Error: 'length' key not found in 'info' dictionary.");
                }
            } else {
                Console.WriteLine("Error: 'announce' or 'info' key not found.");
            }
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
