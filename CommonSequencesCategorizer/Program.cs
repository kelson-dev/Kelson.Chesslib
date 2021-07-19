using var file = File.OpenRead(args[0]);
using var reader = new StreamReader(file);
nint i = 0;
while (reader.ReadLine() is string line)
    if (line.StartsWith("1."))
        Print(line);

void Print(string text)
{
    i++;
    if (i == 80)
    {
        i = 0;
        _ = Console.ReadLine();
        Console.Clear();
    }
    if (text.Length > 80)
        text = text[..80];
    Console.WriteLine(text);
}