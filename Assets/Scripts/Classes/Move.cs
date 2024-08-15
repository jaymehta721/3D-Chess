public class Move
{
    public int X;
    public int Y;
    public MoveType Type;

    public Move(int x, int y, MoveType type)
    {
        this.X = x;
        this.Y = y;
        this.Type = type;
    }
}