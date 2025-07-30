using UnityEngine;
using UnityEngine.UI;
using System;

public class PromotionUI : MonoBehaviour
{
    [SerializeField] private GameObject promotionPanel;
    [SerializeField] private Button queenButton;
    [SerializeField] private Button rookButton;
    [SerializeField] private Button bishopButton;
    [SerializeField] private Button knightButton;

    private Action<Type> onPromotionSelected;

    private void Awake()
    {
        queenButton.onClick.AddListener(() => SelectPromotion(typeof(Queen)));
        rookButton.onClick.AddListener(() => SelectPromotion(typeof(Rook)));
        bishopButton.onClick.AddListener(() => SelectPromotion(typeof(Bishop)));
        knightButton.onClick.AddListener(() => SelectPromotion(typeof(Knight)));

        HidePromotionPanel();
    }

    public void ShowPromotionPanel(TeamColor team, Action<Type> callback)
    {
        // You could change button colors based on team here
        onPromotionSelected = callback;
        promotionPanel.SetActive(true);
    }

    public void HidePromotionPanel()
    {
        promotionPanel.SetActive(false);
    }

    private void SelectPromotion(Type pieceType)
    {
        onPromotionSelected?.Invoke(pieceType);
        HidePromotionPanel();
    }
}