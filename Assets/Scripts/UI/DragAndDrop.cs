using UnityEngine;

/*
==============================
[DragAndDrop] - Script placed on every piece in the board.
==============================
*/
class DragAndDrop : MonoBehaviour {
    private bool dragging = false;
    private float distance;
    private Piece this_piece;

    [SerializeField]
    private Board board;

    void Start() {
        this_piece = GetComponent<Piece>(); // Get piece's component
    }

    void Update() {
        if (dragging) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayPoint = ray.GetPoint(distance);

            // Update piece's dragging position, we try to place it as close as we can to the mouse
            transform.position = new Vector3(rayPoint.x - 0.5f, 2.7f, rayPoint.z);
            transform.rotation = new Quaternion(0, 0, 0, 0);

            // Hover the square this piece could go id we drop it
            if (board.shouldHover) {
                Square closest_square = board.GetClosestSquare(transform.position);
                board.HoverClosestSquare(closest_square);
            }
        }
    }

    void OnMouseDown() {
        // If it's my turn
        if (board.currentTurn == this_piece.team) {
            GetComponent<Rigidbody>().isKinematic = true;
            // Set distance between the mouse & this piece
            distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            if (board.shouldHover) {
                board.HoverValidSquares(this_piece);
            }
            dragging = true; // Start dragging
        }
    }
 
    void OnMouseUp() {
        if (dragging) {
            GetComponent<Rigidbody>().isKinematic = false;
            // Get closest square & try to move the piece to it
            Square closest_square = board.GetClosestSquare(transform.position);
            this_piece.MovePiece(closest_square);

            if (board.shouldHover) board.ResetHoveredSquares();
            dragging = false; // Stop dragging
        }
    }
}