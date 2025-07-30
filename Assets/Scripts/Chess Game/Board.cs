using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(SquareSelectorCreator))]
public class Board : MonoBehaviour
{
    public const int BOARD_SIZE = 8;

    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float squareSize;
    [SerializeField] private PromotionUI promotionUI;

    //private Piece[,] grid;
    public Piece[,] grid = new Piece[8, 8];
    public Vector2Int? enPassantSquare = null;

    protected Piece selectedPiece;
    private ChessGameController chessController;
    private SquareSelectorCreator squareSelector;
    public Piece lastMovedPiece;
    private List<Piece> pieces = new List<Piece>();

    public virtual void SelectPieceMoved(Vector2 coords) { }
    public virtual void SetSelectedPiece(Vector2 coords) { }




    protected virtual void Awake()
    {
        squareSelector = GetComponent<SquareSelectorCreator>();
        CreateGrid();
    }

    public void SetDependencies(ChessGameController chessController)
    {
        this.chessController = chessController;
    }



    private void CreateGrid()
    {
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
    }

    public Vector3 CalculatePositionFromCoords(Vector2Int coords)
    {
        return bottomLeftSquareTransform.position + new Vector3(coords.x * squareSize, 0f, coords.y * squareSize);
    }

    private Vector2Int CalculateCoordsFromPosition(Vector3 inputPosition)
    {
        int x = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).x / squareSize) + BOARD_SIZE / 2;
        int y = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).z / squareSize) + BOARD_SIZE / 2;
        return new Vector2Int(x, y);
    }

    public void OnSquareSelected(Vector3 inputPosition)
    {
        Vector2Int coords = CalculateCoordsFromPosition(inputPosition);
        Piece piece = GetPieceOnSquare(coords);
        if (selectedPiece)
        {
            if (piece != null && selectedPiece == piece)
                DeselectPiece();
            else if (piece != null && selectedPiece != piece && chessController.IsTeamTurnActive(piece.team))
                SelectPiece(coords);
            else if (selectedPiece.CanMoveTo(coords))
                SelectPieceMoved(coords);
        }
        else
        {
            if (piece != null && chessController.IsTeamTurnActive(piece.team))
                SelectPiece(coords);
        }
    }

    

    private void SelectPiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        chessController.RemoveMovesEnablingAttakOnPieceOfType<King>(piece);
        SetSelectedPiece(coords);
        List<Vector2Int> selection = selectedPiece.avaliableMoves;
        ShowSelectionSquares(selection);
    }

    private void ShowSelectionSquares(List<Vector2Int> selection)
    {
        Dictionary<Vector3, bool> squaresData = new();
        for (int i = 0; i < selection.Count; i++)
        {
            Vector3 position = CalculatePositionFromCoords(selection[i]);
            bool isSquareFree = GetPieceOnSquare(selection[i]) == null;
            squaresData.Add(position, isSquareFree);
        }
        squareSelector.ShowSelection(squaresData);
    }

    private void DeselectPiece()
    {
        selectedPiece = null;
        squareSelector.ClearSelection();
    }
    public virtual void OnSelectedPieceMoved(Vector2Int coords, Piece piece)
    {
        bool isPromotionMove = (piece is Pawn) && IsPromotionSquare(coords, piece.team);

        TryToTakeOppositePiece(coords);
        UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null);
        selectedPiece.MovePiece(coords);
        DeselectPiece();

        if (!isPromotionMove)
        {
            EndTurn();
        }
    }

    private bool IsPromotionSquare(Vector2Int coords, TeamColor team)
    {
        int promotionRank = team == TeamColor.White ? Board.BOARD_SIZE - 1 : 0;
        return coords.y == promotionRank;
    }

    public virtual void OnSetSelectedPiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        selectedPiece = piece;
    }

    private void EndTurn()
    {
        chessController.EndTurn();
    }

    public void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece)
    {
        grid[oldCoords.x, oldCoords.y] = oldPiece;
        grid[newCoords.x, newCoords.y] = newPiece;
    }

    public Piece GetPieceOnSquare(Vector2Int coords)
    {
        if (CheckIfCoordinatesAreOnBoard(coords))
            return grid[coords.x, coords.y];
        return null;
    }



    public bool CheckIfCoordinatesAreOnBoard(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0 || coords.x >= BOARD_SIZE || coords.y >= BOARD_SIZE)
            return false;
        return true;
    }

    public bool HasPiece(Piece piece)
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (grid[i, j] == piece)
                    return true;
            }
        }
        return false;
    }

    public void SetPieceOnBoard(Vector2Int coords, Piece piece)
    {
        if (CheckIfCoordinatesAreOnBoard(coords))
            grid[coords.x, coords.y] = piece;
    }

    private void TryToTakeOppositePiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        if (piece && !selectedPiece.IsFromSameTeam(piece))
        {
            TakePiece(piece);
        }
    }

    private void TakePiece(Piece piece)
    {
        if (piece)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            chessController.OnPieceRemoved(piece);
            Destroy(piece.gameObject);
        }
    }

    public void RemovePiece(Piece piece)
    {
        if (pieces.Contains(piece))
        {
            pieces.Remove(piece);
            Destroy(piece.gameObject);
        }
    }

    public void RemovePieceAtSquare(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        if (piece != null)
        {
            TakePiece(piece);
        }
    }




    public void PromotePiece(Piece piece)
    {
        chessController.HandlePromotion(piece, () =>
        {
            // This callback runs after promotion is complete
            //EndTurn();
        });
    }

    internal void OnGameRestarted()
    {
        selectedPiece = null;
        CreateGrid();
    }


}
