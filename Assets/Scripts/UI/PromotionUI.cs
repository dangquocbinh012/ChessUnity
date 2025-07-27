using System;
using UnityEngine;
using UnityEngine.UI;


public class PromotionUI : MonoBehaviour
{
    public GameObject panel;
    public Button queenButton, rookButton, bishopButton, knightButton;

    private Action<Type> onPieceSelected;

    public static PromotionUI Instance;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        queenButton.onClick.AddListener(() => SelectPiece(typeof(Queen)));
        rookButton.onClick.AddListener(() => SelectPiece(typeof(Rook)));
        bishopButton.onClick.AddListener(() => SelectPiece(typeof(Bishop)));
        knightButton.onClick.AddListener(() => SelectPiece(typeof(Knight)));
    }

    public void Show(Action<Type> callback)
    {
        onPieceSelected = callback;
        panel.SetActive(true);
    }

    private void SelectPiece(Type pieceType)
    {
        panel.SetActive(false);
        onPieceSelected?.Invoke(pieceType);
    }
}