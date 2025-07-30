using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PiecesCreator))]
public class ChessGameController : MonoBehaviour
{
    protected enum GameState
    {
        Init, Play, Finished
    }

    [SerializeField] private BoardLayout startingBoardLayout;
    [SerializeField] private Board board;
    [SerializeField] private ChessUIManager uiManager;
    [SerializeField] private PromotionUI promotionUI;
    private Piece pawnToPromote;

    private PiecesCreator pieceCreator;
    protected ChessPlayer whitePlayer;
    protected ChessPlayer blackPlayer;
    protected ChessPlayer activePlayer;
    private CameraSetup cameraSetup;

    protected GameState state;
    private static readonly Dictionary<PieceType, Type> pieceTypeMap = new Dictionary<PieceType, Type>
{
    { PieceType.Pawn, typeof(Pawn) },
    { PieceType.Rook, typeof(Rook) },
    { PieceType.Knight, typeof(Knight) },
    { PieceType.Bishop, typeof(Bishop) },
    { PieceType.Queen, typeof(Queen) },
    { PieceType.King, typeof(King) }
};

    


    public virtual void TryToStartCurrentGame() { }
    public virtual bool CanPerformMove()
    {
        return IsGameInProgress(); 
    }

    private void Awake()
    {
        pieceCreator = GetComponent<PiecesCreator>();
        board = FindAnyObjectByType<Board>();
        uiManager = FindAnyObjectByType<ChessUIManager>();
        cameraSetup = FindAnyObjectByType<CameraSetup>();

        SetDependencies(cameraSetup, uiManager, board);
        CreatePlayers();
    }

    public void SetDependencies(CameraSetup cameraSetup, ChessUIManager uiManager, Board board)
    {
        this.cameraSetup = cameraSetup;
        this.uiManager = uiManager;
        this.board = board;
        this.pieceCreator = GetComponent<PiecesCreator>();
    }

    


    private void CreatePlayers()
    {
        whitePlayer = new ChessPlayer(TeamColor.White, board);
        blackPlayer = new ChessPlayer(TeamColor.Black, board);
    }

    private void Start()
    {
        if (pieceCreator == null)
        {
            Debug.LogError("PiecesCreator component is missing on ChessGameController!");
            return;
        }

        StartNewGame();
    }


    public void StartNewGame()
    {
        //uiManager.HideUI();
        SetGameState(GameState.Init);
        board.SetDependencies(this);
        CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);
        SetGameState(GameState.Play);
        
    }

    
    protected virtual void SetGameState(GameState state)
    {
        this.state = state;
    }

    internal bool IsGameInProgress()
    {
        return state == GameState.Play;
    }



    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            Vector2Int squareCoords = layout.GetSquareCoordsAtIndex(i);
            TeamColor team = layout.GetSquareTeamColorAtIndex(i);
            PieceType pieceType = layout.GetSquarePieceTypeAtIndex(i); 

            Type type = GetPieceTypeFromEnum(pieceType);
            CreatePieceAndInitialize(squareCoords, team, type);
        }
    }

    private Type GetPieceTypeFromEnum(PieceType pieceType)
    {
        return pieceTypeMap[pieceType];
    }



    public void CreatePieceAndInitialize(Vector2Int squareCoords, TeamColor team, Type type)
    {
        if (pieceCreator == null)
        {
            Debug.LogError("pieceCreator is null!");
            return;
        }

        GameObject pieceObj = pieceCreator.CreatePiece(type);
        if (pieceObj == null)
        {
            Debug.LogError($"Failed to create piece of type {type}");
            return;
        }

        Piece newPiece = pieceObj.GetComponent<Piece>();
        if (newPiece == null)
        {
            Debug.LogError($"Created GameObject does not have a Piece component: {pieceObj.name}");
            return;
        }

        // Proceed with initialization...
    }



    private void GenerateAllPossiblePlayerMoves(ChessPlayer player)
    {
        player.GenerateAllPossibleMoves();
    }

    public bool IsTeamTurnActive(TeamColor team)
    {
        return activePlayer.team == team;
    }

    public void EndTurn()
    {
        board.enPassantSquare = null;
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        if (CheckIfGameIsFinished())
        {
            EndGame();
        }
        else
        {
            ChangeActiveTeam();
        }
    }

    private bool CheckIfGameIsFinished()
    {
        Piece[] kingAttackingPieces = activePlayer.GetPieceAtackingOppositePiceOfType<King>();
        if (kingAttackingPieces.Length > 0)
        {
            ChessPlayer oppositePlayer = GetOpponentToPlayer(activePlayer);
            Piece attackedKing = oppositePlayer.GetPiecesOfType<King>().FirstOrDefault();
            oppositePlayer.RemoveMovesEnablingAttakOnPieceOfType<King>(activePlayer, attackedKing);

            int avaliableKingMoves = attackedKing.avaliableMoves.Count;
            if (avaliableKingMoves == 0)
            {
                bool canCoverKing = oppositePlayer.CanHidePieceFromAttack<King>(activePlayer);
                if (!canCoverKing)
                    return true;
            }
        }
        return false;
    }

    private void EndGame()
    {
        //uiManager.OnGameFinished(activePlayer.team.ToString());
        SetGameState(GameState.Finished);
    }

    public void RestartGame()
    {
        DestroyPieces();
        board.OnGameRestarted();
        whitePlayer.OnGameRestarted();
        blackPlayer.OnGameRestarted();
        StartNewGame();
    }

    private void DestroyPieces()
    {
        whitePlayer.activePieces.ForEach(p => Destroy(p.gameObject));
        blackPlayer.activePieces.ForEach(p => Destroy(p.gameObject));
    }

    private void ChangeActiveTeam()
    {
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer player)
    {
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }

    internal void OnPieceRemoved(Piece piece)
    {
        ChessPlayer pieceOwner = (piece.team == TeamColor.White) ? whitePlayer : blackPlayer;
        pieceOwner.RemovePiece(piece);
    }

    internal void RemoveMovesEnablingAttakOnPieceOfType<T>(Piece piece) where T : Piece
    {
        activePlayer.RemoveMovesEnablingAttakOnPieceOfType<T>(GetOpponentToPlayer(activePlayer), piece);
    }
    public void HandlePromotion(Piece pawn, Action onPromotionComplete)
    {
        pawnToPromote = pawn;
        promotionUI.ShowPromotionPanel(pawn.team, (pieceType) =>
        {
            PromotePawn(pieceType);
            onPromotionComplete?.Invoke();
        });
    }

    private void PromotePawn(Type pieceType)
    {
        if (pawnToPromote == null) return;

        Vector2Int coords = pawnToPromote.occupiedSquare;
        TeamColor team = pawnToPromote.team;

        // Remove the pawn
        OnPieceRemoved(pawnToPromote);
        Destroy(pawnToPromote.gameObject);

        // Create new piece
        CreatePieceAndInitialize(coords, team, pieceType);

        pawnToPromote = null;
    }


}

