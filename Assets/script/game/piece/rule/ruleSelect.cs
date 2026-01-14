using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public static class ruleSelect
{
    // ëIëÇ≥ÇÍÇΩãÓÇÕéÑÇÃÅH
    public static bool TrySelectMinePiece(
    Board board,
    PieceColor currentTurn,
    Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard,
    out piece_Controller selected)
    {
        selected = null;
        Debug.Log("TrySelectMinePiece called");

        if (!RaycastHitBoard(out Vector2Int pos))
        {
            Debug.Log("RaycastHitBoard false");
            return false;
        }

        Debug.Log($"Raycast hit pos = {pos}");

        if (!pieceCtrOnBoard.TryGetValue(pos, out var ctr))
        {
            Debug.Log("No piece on this cell");
            return false;
        }

        Debug.Log($"Hit piece {ctr.name}");

        if (!ctr.IsMine(currentTurn))
        {
            Debug.Log("Not my piece");
            return false;
        }

        selected = ctr;
        Debug.Log("Select success");
        return true;
    }

    static bool RaycastHitBoard(out Vector2Int pos)
    {
        pos = default;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            BoardCell cell = hit.collider.GetComponent<BoardCell>();
            if (cell == null)
                return false;

            pos = cell.boardPos;
            return true;
        }

        return false;
    }

}