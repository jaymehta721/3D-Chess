using UnityEngine;

public class Coordinate
{
    public int X;
    public int Y;
    public Vector3 Position;

    public Coordinate(int x, int y)
    {
        this.X = x;
        this.Y = y;
        Position = Vector3.zero;
    }
}