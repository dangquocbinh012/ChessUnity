using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override List<Vector2Int> SelectAvaliableSquares()
    {
        avaliableMoves.Clear();

        Vector2Int direction = team == TeamColor.White ? Vector2Int.up : Vector2Int.down;
        float range = hasMoved ? 1 : 2;

        // Normal pawn moves
        for (int i = 1; i <= range; i++)
        {
            Vector2Int nextCoords = occupiedSquare + direction * i;
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinatesAreOnBoard(nextCoords))
                break;
            if (piece == null)
                TryToAddMove(nextCoords);
            else
                break;
        }

        // Attack moves including en passant
        Vector2Int[] takeDirections = new Vector2Int[] { new Vector2Int(1, direction.y), new Vector2Int(-1, direction.y) };

        foreach (var takeDir in takeDirections)
        {
            Vector2Int targetCoords = occupiedSquare + takeDir;
            if (!board.CheckIfCoordinatesAreOnBoard(targetCoords))
                continue;

            Piece targetPiece = board.GetPieceOnSquare(targetCoords);

            // Regular capture
            if (targetPiece != null && !IsFromSameTeam(targetPiece))
            {
                TryToAddMove(targetCoords);
            }
            // En passant capture
            else if (targetPiece == null)
            {
                // Check if this is the en passant square and there's an enemy pawn beside us
                Vector2Int potentialPawnSquare = new Vector2Int(targetCoords.x, occupiedSquare.y);
                Piece potentialPawn = board.GetPieceOnSquare(potentialPawnSquare);

                if (potentialPawn != null &&
                    potentialPawn is Pawn &&
                    !IsFromSameTeam(potentialPawn) &&
                    potentialPawn == board.lastMovedPiece &&
                    Mathf.Abs(potentialPawn.previousSquare.y - potentialPawn.occupiedSquare.y) == 2)
                {
                    TryToAddMove(targetCoords);
                    board.enPassantSquare = targetCoords; // Mark this as valid en passant square
                }
            }
        }

        return avaliableMoves;
    }

    public override void MovePiece(Vector2Int coords)
    {
        Vector2Int direction = team == TeamColor.White ? Vector2Int.up : Vector2Int.down;

        // Check for en passant capture
        if (board.enPassantSquare == coords)
        {
            // The pawn to capture is actually one square behind the target coordinate
            Vector2Int captureSquare = new Vector2Int(coords.x, coords.y - direction.y);
            Piece capturedPawn = board.GetPieceOnSquare(captureSquare);

            if (capturedPawn != null && capturedPawn is Pawn)
            {
                board.RemovePieceAtSquare(captureSquare);
            }
        }

        base.MovePiece(coords);

        // Track this pawn for potential en passant capture next move
        if (Mathf.Abs(previousSquare.y - occupiedSquare.y) == 2)
        {
            board.lastMovedPiece = this;
        }
        else
        {
            board.lastMovedPiece = null;
        }

        // Clear en passant square after move
        board.enPassantSquare = null;

        CheckPromotion();
    }


    private void CheckPromotion()
    {
        int endOfBoardYCoord = team == TeamColor.White ? Board.BOARD_SIZE - 1 : 0;
        if (occupiedSquare.y == endOfBoardYCoord)
        {
            board.PromotePiece(this);
        }
    }


}
