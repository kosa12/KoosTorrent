using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: tor.sh <command> <param>");
            return;
        }

        var (command, param) = (args[0], args[1]);

        if (command == "decode")
        {
            var encodedValue = param;
            byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(encodedValue);
            var decodedValue = Bencode.Decode(ref byteArray);
            Console.WriteLine(JsonSerializer.Serialize(decodedValue));
        }
        else if (command == "info")
        {
            Info.InfoCommand(param);
        }
        else if (command == "peers")
        {
            Tracker.RequestPeers(param);
        }
        else
        {
            Console.WriteLine($"Invalid command: {command}");
        }
    }
}

