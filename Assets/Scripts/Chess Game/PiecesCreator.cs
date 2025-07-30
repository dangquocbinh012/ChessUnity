using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PiecesCreator : MonoBehaviour
{
    [SerializeField] private GameObject[] piecesPrefabs;
    [SerializeField] private Material blackMaterial;
    [SerializeField] private Material whiteMaterial;
    private Dictionary<string, GameObject> nameToPieceDict = new Dictionary<string, GameObject>();

    private void Awake()
    {
        foreach (var piece in piecesPrefabs)
        {
            string key = piece.GetComponent<Piece>().GetType().Name; // Use simple name
            nameToPieceDict.Add(key, piece);
        }
    }


    public GameObject CreatePiece(Type type)
    {
        if (type == null)
        {
            Debug.LogError("CreatePiece called with null type.");
            return null;
        }

        string key = type.Name;
        if (!nameToPieceDict.TryGetValue(key, out GameObject prefab))
        {
            Debug.LogError($"No prefab found for type: {key}");
            return null;
        }

        return Instantiate(prefab);
    }

    public Material GetTeamMaterial(TeamColor team)
    {
        return team == TeamColor.White ? whiteMaterial : blackMaterial;
    }
}
