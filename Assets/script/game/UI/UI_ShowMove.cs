using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_ShowMove : MonoBehaviour
{
    // âΩÇ≈ï\é¶Ç∑ÇÈÅH â~Ç∆Ç©éläpÇ∆Ç©
    [SerializeField] GameObject MovePrefab;
    public Vector2Int boardPos;

    List<GameObject> activeHighlights = new();

    public void ShowMove(List<Vector2Int> cells, BoardCell boardCell)
    {
        Clear();

        foreach (var cell in cells)
        {
            Vector3 pos = boardCell.BoardToWorld(cell.x, cell.y);
            var h = Instantiate(MovePrefab, pos, Quaternion.identity);
            activeHighlights.Add(h);
        }
    }

    public void Clear()
    {
        foreach (var h in activeHighlights)
            Destroy(h);

        activeHighlights.Clear();
    }
}