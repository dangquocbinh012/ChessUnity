using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class MultiplayerBoard : Board
{
    private PhotonView photonView;

    protected override void Awake()
    {
        photonView = GetComponent<PhotonView>();
        base.Awake(); 
    }

    public override void SelectPieceMoved(Vector2 coords)
    {
        photonView.RPC(nameof(RPC_OnSelectedPieceMoved), RpcTarget.AllBuffered, coords);
    }

    public override void SetSelectedPiece(Vector2 coords)
    {
        photonView.RPC(nameof(RPC_SetSelectedPiece), RpcTarget.AllBuffered, coords);
    }

    [PunRPC]
    private void RPC_SetSelectedPiece(Vector2 coords)
    {
        Vector2Int intCoords = new Vector2Int(Mathf.RoundToInt(coords.x), Mathf.RoundToInt(coords.y));
        base.OnSetSelectedPiece(intCoords); // Call base implementation
    }

    [PunRPC]
    private void RPC_OnSelectedPieceMoved(Vector2 coords)
    {
        Vector2Int intCoords = new Vector2Int(Mathf.RoundToInt(coords.x), Mathf.RoundToInt(coords.y));
        base.OnSelectedPieceMoved(intCoords, selectedPiece); // Call base implementation
    }
}