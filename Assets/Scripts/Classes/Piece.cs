using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Piece : MonoBehaviour
{
    private List<Move> allowedMoves = new List<Move>();
    private MoveType moveType;
    private Piece castlingTower;

    public readonly List<Coordinate> BreakPoints = new List<Coordinate>();
    public bool started;
    public Square CurrentSquare;
    public Board Board;

    [SerializeField] public string PieceName;

    [SerializeField] public int team; // Whites = -1, Blacks = 1

    [SerializeField] public List<Piece> castlingTowers;

    void Start()
    {
        switch (PieceName)
        {
            case "Pawn":
                AddPawnAllowedMoves();
                break;
            case "Tower":
                AddLinealAllowedMoves();
                break;
            case "Horse":
                AddHorseAllowedMoves();
                break;
            case "Bishop":
                AddDiagonalAllowedMoves();
                break;
            case "Queen":
                AddLinealAllowedMoves();
                AddDiagonalAllowedMoves();
                break;
            case "King":
                AddKingAllowedMoves();
                break;
        }
    }


    public void MovePiece(Square square)
    {
        if (CheckValidMove(square))
        {
            switch (moveType)
            {
                case MoveType.StartOnly:
                    if (PieceName == "King" && CheckCastling(square))
                    {
                        if (castlingTower.CurrentSquare.Coordinate.X == 0)
                        {
                            castlingTower.CastleTower(castlingTower.CurrentSquare.Coordinate.X + 2);
                        }
                        else
                        {
                            castlingTower.CastleTower(castlingTower.CurrentSquare.Coordinate.X - 3);
                        }
                    }

                    break;
                case MoveType.Eat:
                case MoveType.EatMove:
                case MoveType.EatMoveJump:
                    EatPiece(square.HoldingPiece);
                    break;
            }

            CurrentSquare.HoldPiece(null);
            square.HoldPiece(this);
            CurrentSquare = square;
            if (!started) started = true;

            Board.ChangeTurn();
        }

        BreakPoints.Clear();
        transform.position = new Vector3(CurrentSquare.Coordinate.Position.x, transform.position.y,
            CurrentSquare.Coordinate.Position.z);
        transform.rotation = new Quaternion(0, 0, 0, 0);
    }

    private Coordinate GetCoordinateMove(Square square)
    {
        int coordinateX = (square.Coordinate.X - CurrentSquare.Coordinate.X) * team;
        int coordinateY = (square.Coordinate.Y - CurrentSquare.Coordinate.Y) * team;

        return new Coordinate(coordinateX, coordinateY);
    }

    public bool CheckValidMove(Square square)
    {
        Coordinate coor_move = GetCoordinateMove(square);

        for (int i = 0; i < allowedMoves.Count; i++)
        {
            if (coor_move.X == allowedMoves[i].X && coor_move.Y == allowedMoves[i].Y)
            {
                moveType = allowedMoves[i].Type;
                switch (moveType)
                {
                    case MoveType.StartOnly:
                        // If this piece hasn't been moved before, can move to the square or is trying to castle
                        if (!started && CheckCanMove(square) && CheckCastling(square))
                            return true;
                        break;
                    case MoveType.Move:
                        if (CheckCanMove(square))
                        {
                            return true;
                        }

                        break;
                    case MoveType.Eat:
                        if (CheckCanEat(square))
                            return true;
                        break;
                    case MoveType.EatMove:
                    case MoveType.EatMoveJump:
                        if (CheckCanEatMove(square))
                        {
                            return true;
                        }

                        break;
                }
            }
        }

        return false;
    }

    private bool CheckValidCheckKingMove(Square square)
    {
        bool avoidsCheck = false;

        Piece oldHoldingPiece = square.HoldingPiece;
        Square oldSquare = CurrentSquare;

        CurrentSquare.HoldPiece(null);
        CurrentSquare = square;
        square.HoldPiece(this);

        if (!Board.isCheckKing(Board.currentTurn) || (square == Board.checkingPieces[team].CurrentSquare))
        {
            avoidsCheck = true;
        }

        CurrentSquare = oldSquare;
        CurrentSquare.HoldPiece(this);
        square.HoldPiece(oldHoldingPiece);
        return avoidsCheck;
    }

    private bool CheckCanMove(Square square)
    {
        Coordinate coordinateMove = GetCoordinateMove(square);

        return square.HoldingPiece == null && CheckBreakPoint(coordinateMove) && CheckValidCheckKingMove(square);
    }

    private bool CheckCanEat(Square square)
    {
        Coordinate coordinateMove = GetCoordinateMove(square);

        return square.HoldingPiece != null && square.HoldingPiece.team != team && CheckBreakPoint(coordinateMove) &&
               CheckValidCheckKingMove(square);
    }

    private bool CheckCanEatMove(Square square)
    {
        return CheckCanEat(square) || CheckCanMove(square);
    }

    private bool CheckBreakPoint(Coordinate coor)
    {
        foreach (var coordinate in BreakPoints)
        {
            if (coordinate.X == 0 && coor.X == 0)
            {
                switch (coordinate.Y)
                {
                    case < 0 when (coor.Y < coordinate.Y):
                    case > 0 when (coor.Y > coordinate.Y):
                        return false;
                }
            }
            else
                switch (coordinate.Y)
                {
                    case 0 when coor.Y == 0:
                    {
                        switch (coordinate.X)
                        {
                            case > 0 when (coor.X > coordinate.X):
                            case < 0 when (coor.X < coordinate.X):
                                return false;
                        }

                        break;
                    }
                    case > 0 when (coor.Y > coordinate.Y):
                    {
                        switch (coordinate.X)
                        {
                            case > 0 when (coor.X > coordinate.X):
                            case < 0 when (coor.X < coordinate.X):
                                return false;
                        }

                        break;
                    }
                    case < 0 when (coor.Y < coordinate.Y):
                    {
                        switch (coordinate.X)
                        {
                            case > 0 when (coor.X > coordinate.X):
                            case < 0 when (coor.X < coordinate.X):
                                return false;
                        }

                        break;
                    }
                }
        }

        return true;
    }

    public void AddBreakPoint(Square square)
    {
        Coordinate coordinateMove = GetCoordinateMove(square);

        foreach (var coordinate in allowedMoves)
        {
            if (coordinateMove.X == coordinate.X && coordinateMove.Y == coordinate.Y)
            {
                switch (coordinate.Type)
                {
                    case MoveType.StartOnly:
                    case MoveType.Move:
                    case MoveType.Eat:
                    case MoveType.EatMove:
                        // If square is holding a piece
                        if (square.HoldingPiece != null)
                        {
                            BreakPoints.Add(coordinateMove);
                        }

                        break;
                    case MoveType.EatMoveJump:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }


    private void CastleTower(int coordinateX)
    {
        Coordinate castlingCoordinate = new Coordinate(coordinateX, CurrentSquare.Coordinate.Y);
        Square square = Board.GetSquareFromCoordinate(castlingCoordinate);

        CurrentSquare.HoldPiece(null);
        square.HoldPiece(this);
        CurrentSquare = square;
        if (!started) started = true;

        transform.position = new Vector3(CurrentSquare.Coordinate.Position.x, transform.position.y,
            CurrentSquare.Coordinate.Position.z);
        transform.rotation = new Quaternion(0, 0, 0, 0);
    }

    private bool CheckCastling(Square square)
    {
        if (PieceName == "King")
        {
            float closestCastling = Vector3.Distance(square.Coordinate.Position, castlingTowers[0].transform.position);
            castlingTower = castlingTowers[0];

            foreach (var coordinate in castlingTowers)
            {
                if (Vector3.Distance(square.Coordinate.Position, coordinate.transform.position) <= closestCastling)
                {
                    castlingTower = coordinate;
                }
            }

            bool canCastle = Board.CheckCastlingSquares(CurrentSquare, castlingTower.CurrentSquare, team);

            return (!castlingTower.started && canCastle);
        }

        return true;
    }

    private void addAllowedMove(int coor_x, int coor_y, MoveType type)
    {
        Move newMove = new Move(coor_x, coor_y, type);
        allowedMoves.Add(newMove);
    }

    // Pawns allowed moves
    private void AddPawnAllowedMoves()
    {
        addAllowedMove(0, 1, MoveType.Move);
        addAllowedMove(0, 2, MoveType.StartOnly);
        addAllowedMove(1, 1, MoveType.Eat);
        addAllowedMove(-1, 1, MoveType.Eat);
    }

    private void AddLinealAllowedMoves()
    {
        for (int coor_x = 1; coor_x < 8; coor_x++)
        {
            addAllowedMove(coor_x, 0, MoveType.EatMove);
            addAllowedMove(0, coor_x, MoveType.EatMove);
            addAllowedMove(-coor_x, 0, MoveType.EatMove);
            addAllowedMove(0, -coor_x, MoveType.EatMove);
        }
    }

    private void AddDiagonalAllowedMoves()
    {
        for (int coor_x = 1; coor_x < 8; coor_x++)
        {
            addAllowedMove(coor_x, -coor_x, MoveType.EatMove);
            addAllowedMove(-coor_x, coor_x, MoveType.EatMove);
            addAllowedMove(coor_x, coor_x, MoveType.EatMove);
            addAllowedMove(-coor_x, -coor_x, MoveType.EatMove);
        }
    }

    private void AddHorseAllowedMoves()
    {
        for (int coor_x = 1; coor_x < 3; coor_x++)
        {
            for (int coor_y = 1; coor_y < 3; coor_y++)
            {
                if (coor_y != coor_x)
                {
                    addAllowedMove(coor_x, coor_y, MoveType.EatMoveJump);
                    addAllowedMove(-coor_x, -coor_y, MoveType.EatMoveJump);
                    addAllowedMove(coor_x, -coor_y, MoveType.EatMoveJump);
                    addAllowedMove(-coor_x, coor_y, MoveType.EatMoveJump);
                }
            }
        }
    }

    private void AddKingAllowedMoves()
    {
        addAllowedMove(-2, 0, MoveType.StartOnly);
        addAllowedMove(2, 0, MoveType.StartOnly);

        addAllowedMove(0, 1, MoveType.EatMove);
        addAllowedMove(1, 1, MoveType.EatMove);
        addAllowedMove(1, 0, MoveType.EatMove);
        addAllowedMove(1, -1, MoveType.EatMove);
        addAllowedMove(0, -1, MoveType.EatMove);
        addAllowedMove(-1, -1, MoveType.EatMove);
        addAllowedMove(-1, 0, MoveType.EatMove);
        addAllowedMove(-1, 1, MoveType.EatMove);
    }


    public void SetStartSquare(Square square)
    {
        CurrentSquare = square;
    }

    private void EatMe()
    {
        Board.DestroyPiece(this);
        Destroy(this.gameObject);
    }

    private void EatPiece(Piece piece)
    {
        if (piece != null && piece.team != team) piece.EatMe();
    }
}