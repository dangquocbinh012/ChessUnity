using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ChessUIManager : MonoBehaviour 
{
    [SerializeField] private GameObject UIParent;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private PiecesCreator piecesCreator;

    public void HideUI()
    {
        UIParent.SetActive(false);
    }
    public void OnGameFinished(string winner)
    {
        UIParent.SetActive(true);
        resultText.text = string.Format("{0} won", winner);
    }
    public void TriggerPawnPromotion(TeamColor teamColor, Vector3 position, Action<GameObject> onPromoted)
    {
        PromotionUI.Instance.Show(type => {
            GameObject piece = piecesCreator.CreatePiece(type);
            piece.GetComponent<Piece>().team = teamColor;
            piece.transform.position = position;
            onPromoted?.Invoke(piece); // Let board logic know the piece
        });
    }

}
