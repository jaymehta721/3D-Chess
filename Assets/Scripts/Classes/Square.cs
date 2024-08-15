using UnityEngine;

public class Square : MonoBehaviour
{
    private Material CurrentMaterial { get; set; } 

    public Coordinate Coordinate { get; set; }
    public Piece HoldingPiece { get; private set; } 
    public Material StartMat { get; set; }

    [SerializeField] public int team;

    [SerializeField] public Board board;

    private void Start()
    {
        StartMat = GetComponent<Renderer>().material;
    }

    public void HoldPiece(Piece piece)
    {
        HoldingPiece = piece;
    }
    
    public void HoverSquare(Material mat)
    {
        CurrentMaterial = GetComponent<Renderer>().material;
        GetComponent<Renderer>().material = mat;
    }

    public void UnHoverSquare()
    {
        GetComponent<Renderer>().material = CurrentMaterial;
    }

    public void ResetMaterial()
    {
        CurrentMaterial = StartMat;
        GetComponent<Renderer>().material = StartMat;
    }
}