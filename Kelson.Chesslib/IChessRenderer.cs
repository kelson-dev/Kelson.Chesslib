using Kelson.Chesslib.Sim;

namespace Kelson.Chesslib
{
    public interface IChessRenderer<TResult>
    {
        TResult Render(in Chessboard board);
    }
}
