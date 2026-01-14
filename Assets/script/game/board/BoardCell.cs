using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardCell : MonoBehaviour
{
    public Vector2Int boardPos;
    [SerializeField] GameObject cellWhite;
    [SerializeField] GameObject cellBlack;


    public void Initialize(Vector2Int pos)
    {
        boardPos = pos;
    }

    // その座標にある駒単体を渡す
    public piece_Controller GetPieceController(Vector2Int pos, Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard)
    {
        if (pieceCtrOnBoard.TryGetValue(pos, out var ctr))
            return ctr;

        return null;
    }

    // 盤面生成
    public void CreateStage()
    {
        // ここだけzを基準に
        for (int z = 0; z < 8; z++)
        {
            for (int x = 0; x < 8; x++)
            {
                Vector3 pos = makeWorld(x, z);
                // zが偶数 黒*4 白*4
                if (z % 2 == 0)
                {
                    // xが偶数 黒
                    if (x % 2 == 0)
                    {
                        var cell = Instantiate(cellBlack, pos, Quaternion.identity)
                                    .GetComponent<BoardCell>();
                        cell.Initialize(new Vector2Int(x, z));
                    }
                    // xが奇数 白
                    else
                    {
                        var cell = Instantiate(cellWhite, pos, Quaternion.identity)
                                    .GetComponent<BoardCell>();
                        cell.Initialize(new Vector2Int(x, z));
                    }
                }
                // zが奇数 白*4 黒*4
                else
                {
                    // xが偶数 白
                    if (x % 2 == 0)
                    {
                        var cell = Instantiate(cellWhite, pos, Quaternion.identity)
                                    .GetComponent<BoardCell>();
                        cell.Initialize(new Vector2Int(x, z));
                    }
                    // xが奇数 黒
                    else
                    {
                        var cell = Instantiate(cellBlack, pos, Quaternion.identity)
                                    .GetComponent<BoardCell>();
                        cell.Initialize(new Vector2Int(x, z));
                    }
                }
            }
        }
    }


    // 盤面生成用
    public Vector3 makeWorld(float x, float z)
    {
        x = x * 1.5f;
        z = z * 1.5f;
        return new Vector3(x, -0.8f, z);   // Gridのサイズが1.5fずつ増えているため
    }


    // Playerが見る世界に合わせる
    public Vector3 BoardToWorld(float x, float z)
    {
        x = x * 1.5f;
        z = z * 1.5f;
        return new Vector3(x, 0, z);   // Gridのサイズが1.5fずつ増えているため
    }

}