using System;
using System.Collections.Generic;
using System.Linq;

public class Bencode {
    public static object Decode(string input) {
        if (Char.IsDigit(input[0])) {
            return DecodeString(ref input);
        } else if (input[0] == 'i') {
            return DecodeInt(ref input);
        } else if (input[0] == 'l') {
            return DecodeList(ref input);
        } else {
            throw new InvalidOperationException("Unhandled encoded value: " + input);
        }
    }

    public static string Encode(object input) {
        return Type.GetTypeCode(input.GetType()) switch {
            TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 => $"i{input}e",
            TypeCode.String => $"{((string)input).Length}:{input}",
            TypeCode.Object =>
                            input is object[] inputArray
                            ? $"l{string.Join("", inputArray.Select(x => Encode(x)))}e"
                            : throw new InvalidOperationException($"Unknown type: {input.GetType()}"),
            _ => throw new InvalidOperationException($"Unknown type: {input.GetType()}")
        };
    }

    private static string DecodeString(ref string input) {
        int colonIndex = input.IndexOf(':');
        if (colonIndex != -1) {
            int strLength = int.Parse(input[..colonIndex]);
            string result = input.Substring(colonIndex + 1, strLength);
            input = input.Substring(colonIndex + 1 + strLength);
            return result;
        } else {
            throw new InvalidOperationException("Invalid encoded value: " + input);
        }
    }

    private static long DecodeInt(ref string input) {
        int endIndex = input.IndexOf('e');
        if (endIndex != -1) {
            long result = long.Parse(input[1..endIndex]);
            input = input.Substring(endIndex + 1);
            return result;
        } else {
            throw new InvalidOperationException("Invalid encoded value: " + input);
        }
    }

    private static object[] DecodeList(ref string input) {
        input = input.Substring(1);
        var res = new List<object>();
        while (input.Length > 0 && input[0] != 'e') {
            var decoded = Decode(input);
            res.Add(decoded);
            input = input.Substring(Encode(decoded).Length);
        }
        input = input.Substring(1);
        return res.ToArray();
    }
}
