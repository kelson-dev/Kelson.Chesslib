using System;
using Kelson.Chesslib;
using Kelson.Chesslib.Encoding;
using Kelson.Chesslib.Renderers;
using Kelson.Chesslib.Sim;

class PlainTextConsoleRenderer : IChessRenderer<Unit>
{
    private ChessTextRenderer _textRenderer = new(ChessTextRenderer.EmojiGlyphset);
    private readonly bool printCifText;

    public PlainTextConsoleRenderer(bool asciiEncode = false, bool printCifText = false)
    {
        _textRenderer = new(asciiEncode ? ChessTextRenderer.AsciiGlyphset : ChessTextRenderer.EmojiGlyphset);
        this.printCifText = printCifText;
    }

    public Unit Render(in Chessboard board)
    {
        if (printCifText)
        {
            string text = ChessInterchangeString.Encode(board);
            Console.WriteLine(text);
        }
        Console.WriteLine(_textRenderer.Render(board).ToString());
        return default;
    }
}
