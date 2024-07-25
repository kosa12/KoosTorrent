using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Bencode {
    public static object Decode(ref byte[] input) {
        if (Char.IsDigit((char)input[0])) {
            return DecodeString(ref input);
        } else if ((char)input[0] == 'i') {
            return DecodeInt(ref input);
        } else if ((char)input[0] == 'l') {
            return DecodeList(ref input);
        } else if ((char)input[0] == 'd') {
            return DecodeDictionary(ref input);
        } else {
            throw new InvalidOperationException("Unhandled encoded value: " + Encoding.ASCII.GetString(input));
        }
    }
    private static string DecodeString(ref byte[] input) {
        int colonIndex = Array.IndexOf(input, (byte)':');
        if (colonIndex == -1) {
            throw new InvalidOperationException("Invalid encoded value: " + Encoding.ASCII.GetString(input));
        }
        
        int strLength = int.Parse(Encoding.ASCII.GetString(input, 0, colonIndex));
        if (strLength < 0 || strLength > input.Length - (colonIndex + 1)) {
            throw new InvalidOperationException("Invalid string length.");
        }
        
        string result = Encoding.ASCII.GetString(input, colonIndex + 1, strLength);
        input = input[(colonIndex + 1 + strLength)..]; 
        return result;
    }


    private static long DecodeInt(ref byte[] input) {
        int endIndex = Array.IndexOf(input, (byte)'e');
        if (endIndex != -1) {
            long result = long.Parse(Encoding.ASCII.GetString(input, 1, endIndex - 1));
            input = input[(endIndex + 1)..]; 
            return result;
        } else {
            throw new InvalidOperationException("Invalid encoded value: " + Encoding.ASCII.GetString(input));
        }
    }

    private static object[] DecodeList(ref byte[] input) {
        input = input[1..]; 
        var res = new List<object>();
        while (input.Length > 0 && (char)input[0] != 'e') {
            var decoded = Decode(ref input);
            res.Add(decoded);
        }
        input = input[1..]; 
        return res.ToArray();
    }

    private static Dictionary<string, object> DecodeDictionary(ref byte[] input) {
        input = input[1..]; 
        var res = new Dictionary<string, object>();
        while (input.Length > 0 && (char)input[0] != 'e') {
           
            string key = DecodeString(ref input);
            
            var value = Decode(ref input);
            res.Add(key, value);
        }
        input = input[1..]; 
        return res;
    }
}
