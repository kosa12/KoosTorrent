using System.Text.Json;

var (command, param) = args.Length switch {
  0 => throw new InvalidOperationException("Usage: tor.sh <command> <param>"),
  1 => throw new InvalidOperationException("Usage: tor.sh <command> <param>"),
  _ => (args[0], args[1])
};

if (command == "decode") {

  var encodedValue = param;
  Console.WriteLine(JsonSerializer.Serialize(Bencode.Decode(ref encodedValue)));
} else {
  throw new InvalidOperationException($"Invalid command: {command}");
}