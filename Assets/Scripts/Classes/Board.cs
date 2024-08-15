using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Serialization;

/*
==============================
[Board] - Main script, controls the game
==============================
*/
public class Board : MonoBehaviour
{
    private List<Square> hoveredSquares = new List<Square>();
    private Square closestSquare;
    private int currentTheme = 0;

    public int currentTurn = -1; // -1 = whites; 1 = blacks
    public Dictionary<int, Piece> checkingPieces = new Dictionary<int, Piece>();

    public bool shouldHover;
    public bool shouldRotateCamera;

    [SerializeField] MainCamera mainCamera;

    [SerializeField] Material squareHoverMaterial; // Piece's valid squares material

    [SerializeField] Material squareClosestMaterial; // Piece's closest square material

    [SerializeField] GameObject winMessage;

    [SerializeField] TextMesh winText;

    [SerializeField] List<Theme> themes = new List<Theme>();

    [SerializeField] List<Renderer> boardSides = new List<Renderer>();

    [SerializeField] List<Renderer> boardCorners = new List<Renderer>();

    [SerializeField] List<Square> squares = new List<Square>();

    [SerializeField] List<Piece> pieces = new List<Piece>(); 

    void Start()
    {
        SetBoardTheme();
        AddSquareCoordinates(); 
        SetStartPiecesCoordinates(); 
    }

    public Square GetClosestSquare(Vector3 position)
    {
        Square square = squares[0];
        float closest = Vector3.Distance(position, squares[0].Coordinate.Position);

        for (int i = 0; i < squares.Count; i++)
        {
            float distance = Vector3.Distance(position, squares[i].Coordinate.Position);

            if (distance < closest)
            {
                square = squares[i];
                closest = distance;
            }
        }

        return square;
    }

    public Square GetSquareFromCoordinate(Coordinate coordinate)
    {
        Square square = squares[0];
        for (int i = 0; i < squares.Count; i++)
        {
            if (squares[i].Coordinate.X == coordinate.X && squares[i].Coordinate.Y == coordinate.Y)
            {
                return squares[i];
            }
        }

        return square;
    }

    public void HoverClosestSquare(Square square)
    {
        if (closestSquare)
        {
            closestSquare.UnHoverSquare();
        }

        square.HoverSquare(themes[currentTheme].SquareClosest);
        closestSquare = square;
    }

    public void HoverValidSquares(Piece piece)
    {
        AddPieceBreakPoints(piece);
        for (int i = 0; i < squares.Count; i++)
        {
            if (piece.CheckValidMove(squares[i]))
            {
                squares[i].HoverSquare(themes[currentTheme].SquareHover);
                hoveredSquares.Add(squares[i]);
            }
        }
    }

    public void ResetHoveredSquares()
    {
        for (int i = 0; i < hoveredSquares.Count; i++)
        {
            hoveredSquares[i].ResetMaterial();
        }

        hoveredSquares.Clear();
        closestSquare.ResetMaterial();
        closestSquare = null;
    }

    public bool CheckCastlingSquares(Square square1, Square square2, int castlingTeam)
    {
        List<Square> castlingSquares = new List<Square>();

        if (square1.Coordinate.X < square2.Coordinate.X)
        {
            for (int i = square1.Coordinate.X; i < square2.Coordinate.X; i++)
            {
                Coordinate coordinate = new Coordinate(i, square1.Coordinate.Y);
                castlingSquares.Add(GetSquareFromCoordinate(coordinate));
            }
        }
        else
        {
            for (int i = square1.Coordinate.X; i > square2.Coordinate.X; i--)
            {
                Coordinate coordinate = new Coordinate(i, square1.Coordinate.Y);
                castlingSquares.Add(GetSquareFromCoordinate(coordinate));
            }
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].team != castlingTeam)
            {
                AddPieceBreakPoints(pieces[i]);
                for (int j = 0; j < castlingSquares.Count; j++)
                {
                    if (pieces[i].CheckValidMove(castlingSquares[j])) return false;
                }
            }
        }

        return true;
    }

    private void AddSquareCoordinates()
    {
        int coordinateX = 0;
        int coordinateY = 0;
        for (int i = 0; i < squares.Count; i++)
        {
            squares[i].Coordinate = new Coordinate(coordinateX, coordinateY);
            squares[i].Coordinate.Position = new Vector3(squares[i].transform.position.x - 0.5f,
                squares[i].transform.position.y, squares[i].transform.position.z - 0.5f);
            if (squares[i].team == -1) squares[i].GetComponent<Renderer>().material = themes[currentTheme].SquareWhite;
            else if (squares[i].team == 1)
                squares[i].GetComponent<Renderer>().material = themes[currentTheme].SquareBlack;
            squares[i].StartMat = squares[i].GetComponent<Renderer>().material;

            if (coordinateY > 0 && coordinateY % 7 == 0)
            {
                coordinateX++;
                coordinateY = 0;
            }
            else
            {
                coordinateY++;
            }
        }
    }
    
    public void AddPieceBreakPoints(Piece piece)
    {
        piece.BreakPoints.Clear();
        for (int i = 0; i < squares.Count; i++)
        {
            piece.AddBreakPoint(squares[i]);
        }
    }

    public bool isCheckKing(int team)
    {
        Piece king = GetKingPiece(team);

        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].team != king.team)
            {
                AddPieceBreakPoints(pieces[i]);
                if (pieces[i].CheckValidMove(king.CurrentSquare))
                {
                    checkingPieces[team] = pieces[i];
                    return true;
                }
            }
        }

        return false;
    }

    public bool isCheckMate(int team)
    {
        if (isCheckKing(team))
        {
            int validMoves = 0;

            for (int i = 0; i < squares.Count; i++)
            {
                for (int j = 0; j < pieces.Count; j++)
                {
                    if (pieces[j].team == team)
                    {
                        if (pieces[j].CheckValidMove(squares[i]))
                        {
                            validMoves++;
                        }
                    }
                }
            }

            if (validMoves == 0)
            {
                return true;
            }
        }

        return false;
    }

    public Piece GetKingPiece(int team)
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].team == team && pieces[i].PieceName == "King")
            {
                return pieces[i];
            }
        }

        return pieces[0];
    }

    // Remove the given piece from the pieces list
    public void DestroyPiece(Piece piece)
    {
        pieces.Remove(piece);
    }

    private void SetStartPiecesCoordinates()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            Square closestSquare = GetClosestSquare(pieces[i].transform.position);
            closestSquare.HoldPiece(pieces[i]);
            pieces[i].SetStartSquare(closestSquare);
            pieces[i].Board = this;
            if (pieces[i].team == -1) SetPieceTheme(pieces[i].transform, themes[currentTheme].PieceWhite);
            else if (pieces[i].team == 1) SetPieceTheme(pieces[i].transform, themes[currentTheme].PieceBlack);
        }
    }

    private void SetPieceTheme(Transform pieceTransform, Material material)
    {
        for (int i = 0; i < pieceTransform.childCount; ++i)
        {
            Transform child = pieceTransform.GetChild(i);
            try
            {
                child.GetComponent<Renderer>().material = material;
            }
            catch (Exception e)
            {
                for (int j = 0; j < child.childCount; ++j)
                {
                    Transform child2 = child.GetChild(j);
                    child2.GetComponent<Renderer>().material = material;
                }
            }
        }
    }
    
    public void ChangeTurn()
    {
        currentTurn = (currentTurn == -1) ? 1 : -1;
        if (isCheckMate(currentTurn))
        {
            DoCheckMate(currentTurn);
        }
        else if (shouldRotateCamera)
        {
            mainCamera.ChangeTeam(currentTurn);
        }
    }

    public void DoCheckMate(int loser)
    {
        string winner = (loser == 1) ? "White" : "Black";

        winText.text = winner + winText.text;
        int txtRotation = (currentTurn == -1) ? 0 : 180;

        winMessage.transform.rotation = Quaternion.Euler(0, txtRotation, 0);
        winMessage.GetComponent<Rigidbody>().useGravity = true;
    }
    
    public void HoverSquare(bool use)
    {
        shouldHover = use;
    }

    public void RotateCamera(bool rotate)
    {
        shouldRotateCamera = rotate;
    }

    public void SetBoardTheme()
    {
        for (int i = 0; i < boardSides.Count; i++)
        {
            boardSides[i].material = themes[currentTheme].BoardSide;
            boardCorners[i].material = themes[currentTheme].BoardCorner;
        }
    }

    public void UpdateGameTheme(int theme)
    {
        currentTheme = theme;
        SetBoardTheme();
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].team == -1) SetPieceTheme(pieces[i].transform, themes[currentTheme].PieceWhite);
            else if (pieces[i].team == 1) SetPieceTheme(pieces[i].transform, themes[currentTheme].PieceBlack);
        }

        for (int i = 0; i < squares.Count; i++)
        {
            if (squares[i].team == -1) squares[i].GetComponent<Renderer>().material = themes[currentTheme].SquareWhite;
            else if (squares[i].team == 1)
                squares[i].GetComponent<Renderer>().material = themes[currentTheme].SquareBlack;
            squares[i].StartMat = squares[i].GetComponent<Renderer>().material;
        }
    }
}