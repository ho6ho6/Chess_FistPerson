using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class piece_Factory
{
    static piece_Library library;

    public static void Init(piece_Library lib)
    {
        library = lib;
    }


    public static piece_Controller Spawn( PieceType type, PieceColor color, Vector2Int boardPos, BoardCell boardCell)
    {
        GameObject prefab = library.GetPrefab(type, color);
        Vector3 worldPos = boardCell.BoardToWorld(boardPos.x, boardPos.y);

        GameObject obj = Object.Instantiate(prefab, worldPos, Quaternion.identity);
        var ctr = obj.GetComponent<piece_Controller>();

        ctr.Initialize(type, color, boardPos);
        return ctr;
    }

}
