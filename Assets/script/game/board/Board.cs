using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// データ管理

public class Board
{

    /* ---boardの状態定義--- */

    public struct InitialPiecePos
    {
        public Vector2Int pos;
        public ChessPiece piece;

        public InitialPiecePos(int x, int z, PieceColor color, PieceType type)
        {
            pos = new Vector2Int(x, z);
            piece = new ChessPiece
            {
                pieceColor = color,
                pieceType = type
            };
        }
    }

    // boardが保存するもの：ChessPieceの情報のみ
    // ChessPieceは[pieceColor, pieceType, IsEmpty] を保有
    public ChessPiece[,] board = new ChessPiece[8, 8];

    /* ---boardの状態定義--- */

    public void SetUpBoard(BoardCell boardCell, Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard, bool isCastleMode)
    {
        if (boardCell == null)
        {
            Debug.LogError("BoardCell is NULL!");
            return;
        }

        // 盤面を全てnoneで初期化
        ClearBoard();

        // piece_Controllerも初期化
        pieceCtrOnBoard.Clear();

        // 盤面の生成
        boardCell.CreateStage();


        // 駒の配置をランダムにするなど、初期配置を変更したい場合はここで行う
        foreach (var init in GetInitialPieces(isCastleMode))
        {
            board[init.pos.x, init.pos.y] = init.piece;
        }

        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                Debug.Log($"Show now boardState[{x},{z}].{board[x, z].pieceType}.{board[x, z].pieceColor}");

                var piece = GetChessPiece(new Vector2Int(x, z));
                if (!piece.IsEmpty)
                {
                    var ctr = piece_Factory.Spawn(piece.pieceType, piece.pieceColor,
                                        new Vector2Int(x, z), boardCell);

                    ctr.SetBoardPos(new Vector2Int(x, z));
                    pieceCtrOnBoard[new Vector2Int(x, z)] = ctr;
                }
            }
        }
        Debug.Log($"pieceCtrOnBoard count = {pieceCtrOnBoard.Count}");
    }

    void ClearBoard()
    {
        // 駒の情報・盤面の情報をリセット
        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                board[x, z] = new ChessPiece 
                { 
                    pieceType = PieceType.none, 
                    pieceColor = PieceColor.none 
                };
                // Debug.Log($"Initialize board[{x},{z}].{board[x,z].pieceType}");
            }
        }
    }


    // 自分か敵かnullか boardに保存されている駒の情報を読む
    // Empty Mine Enemy がreturn される
    public CellRelation GetCellRelation(Vector2Int pos, PieceColor myColor)
    {
        if (board[pos.x, pos.y].IsEmpty) return CellRelation.Empty;

        // タイトル画面で選択された色(isWhite == myColor)が自分の色だったらmine, 敵の色だったらenemy
        return board[pos.x, pos.y].pieceColor == myColor ? 
                                        CellRelation.Mine : CellRelation.Enemy;
    }

    // 駒の初期配置
    List<InitialPiecePos> GetInitialPieces(bool isCastleMode)
    {
        var list = new List<InitialPiecePos>();

        // もしランダムにしたいなら
        if (isCastleMode)
        {
            /* Kingを護る籠城兵 */     // (x, z)  
            list.Add(new InitialPiecePos(2, 0, PieceColor.white, PieceType.rook));
            list.Add(new InitialPiecePos(3, 0, PieceColor.white, PieceType.pawn));
            list.Add(new InitialPiecePos(4, 0, PieceColor.white, PieceType.rook));
            list.Add(new InitialPiecePos(2, 1, PieceColor.white, PieceType.pawn));
            list.Add(new InitialPiecePos(3, 1, PieceColor.white, PieceType.king));
            list.Add(new InitialPiecePos(4, 1, PieceColor.white, PieceType.pawn));
            list.Add(new InitialPiecePos(2, 2, PieceColor.white, PieceType.bishop));
            list.Add(new InitialPiecePos(3, 2, PieceColor.white, PieceType.pawn));
            list.Add(new InitialPiecePos(4, 2, PieceColor.white, PieceType.bishop));

            /* 歩兵 */
            list.Add(new InitialPiecePos(0, 3, PieceColor.white, PieceType.pawn));
            list.Add(new InitialPiecePos(0, 4, PieceColor.white, PieceType.pawn));
            list.Add(new InitialPiecePos(1, 3, PieceColor.white, PieceType.pawn));
            list.Add(new InitialPiecePos(1, 4, PieceColor.white, PieceType.pawn));

            /* 他侍る駒 */
            list.Add(new InitialPiecePos(0, 1, PieceColor.white, PieceType.knight));
            list.Add(new InitialPiecePos(6, 1, PieceColor.white, PieceType.knight));
            list.Add(new InitialPiecePos(7, 1, PieceColor.white, PieceType.queen));

            // 黒
            list.Add(new InitialPiecePos(3, 7, PieceColor.black, PieceType.rook));
            list.Add(new InitialPiecePos(4, 7, PieceColor.black, PieceType.pawn));
            list.Add(new InitialPiecePos(5, 7, PieceColor.black, PieceType.rook));
            list.Add(new InitialPiecePos(3, 6, PieceColor.black, PieceType.pawn));
            list.Add(new InitialPiecePos(4, 6, PieceColor.black, PieceType.king));
            list.Add(new InitialPiecePos(5, 6, PieceColor.black, PieceType.pawn));
            list.Add(new InitialPiecePos(3, 5, PieceColor.black, PieceType.bishop));
            list.Add(new InitialPiecePos(4, 5, PieceColor.black, PieceType.pawn));
            list.Add(new InitialPiecePos(5, 5, PieceColor.black, PieceType.bishop));

            /* 歩兵 */
            list.Add(new InitialPiecePos(6, 3, PieceColor.black, PieceType.pawn));
            list.Add(new InitialPiecePos(6, 4, PieceColor.black, PieceType.pawn));
            list.Add(new InitialPiecePos(7, 3, PieceColor.black, PieceType.pawn));
            list.Add(new InitialPiecePos(7, 4, PieceColor.black, PieceType.pawn));

            /* 他侍る駒 */
            list.Add(new InitialPiecePos(7, 6, PieceColor.black, PieceType.knight));
            list.Add(new InitialPiecePos(1, 6, PieceColor.black, PieceType.knight));
            list.Add(new InitialPiecePos(0, 6, PieceColor.black, PieceType.queen));

            return list;
        }
        else
        {
            // --- Pawns ---
            for (int x = 0; x < 8; x++)
            {
                list.Add(new InitialPiecePos(x, 1, PieceColor.white, PieceType.pawn));
                list.Add(new InitialPiecePos(x, 6, PieceColor.black, PieceType.pawn));
            }

            // --- White ---
            list.Add(new InitialPiecePos(0, 0, PieceColor.white, PieceType.rook));
            list.Add(new InitialPiecePos(1, 0, PieceColor.white, PieceType.knight));
            list.Add(new InitialPiecePos(2, 0, PieceColor.white, PieceType.bishop));
            list.Add(new InitialPiecePos(3, 0, PieceColor.white, PieceType.queen));
            list.Add(new InitialPiecePos(4, 0, PieceColor.white, PieceType.king));
            list.Add(new InitialPiecePos(5, 0, PieceColor.white, PieceType.bishop));
            list.Add(new InitialPiecePos(6, 0, PieceColor.white, PieceType.knight));
            list.Add(new InitialPiecePos(7, 0, PieceColor.white, PieceType.rook));

            // --- Black ---
            list.Add(new InitialPiecePos(0, 7, PieceColor.black, PieceType.rook));
            list.Add(new InitialPiecePos(1, 7, PieceColor.black, PieceType.knight));
            list.Add(new InitialPiecePos(2, 7, PieceColor.black, PieceType.bishop));
            list.Add(new InitialPiecePos(3, 7, PieceColor.black, PieceType.king));
            list.Add(new InitialPiecePos(4, 7, PieceColor.black, PieceType.queen));
            list.Add(new InitialPiecePos(5, 7, PieceColor.black, PieceType.bishop));
            list.Add(new InitialPiecePos(6, 7, PieceColor.black, PieceType.knight));
            list.Add(new InitialPiecePos(7, 7, PieceColor.black, PieceType.rook));

            return list;
        }
    }

    // 敵か味方か、空かは呼び出し元で判別されている
    // ここで単純にその座標にあったPieceを返す
    public ChessPiece GetChessPiece(Vector2Int pos)
    {
        if (!IsInsideBoard(pos.x, pos.y))
        {
            Debug.LogError($"GetChessPiece: position [{pos.x},{pos.y}] is outside the board!");
            return new ChessPiece { pieceType = PieceType.none, pieceColor = PieceColor.none };
        }

        return board[pos.x, pos.y];
    }

    public void SetChessPiece(Vector2Int pos, ChessPiece piece) => board[pos.x, pos.y] = piece;

    // 移動元の情報は空になる
    public void ClearCell(Vector2Int from)
    {
        board[from.x, from.y] = new ChessPiece
        {
            pieceType = PieceType.none,
            pieceColor = PieceColor.none
        };
    }

    // 単純にマスが埋まっているか 境界か？
    public bool IsCellEmpty(int x, int z)
    {
        return IsInsideBoard(x, z) && board[x, z].IsEmpty;
    }

    // boardの境界に収まってる？
    public bool IsInsideBoard(int x, int z)
    {
        return (x >= 0 && x < 8) && (z >= 0 && z < 8);
    }


    // CPU 専用
    // CPUが使える駒を列挙
    public List<Vector2Int> GetAllCPUPieces(PieceColor color)
    {
        var result = new List<Vector2Int>();

        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                ChessPiece piece = board[x, z];

                if (piece.IsEmpty)
                    continue;

                if (piece.pieceColor != color)
                    continue;

                result.Add(new Vector2Int(x, z));
            }
        }

        return result;
    }

    // CPUが未来予測用に使用する現在の盤面を複製
    public Board Clone()
    {
        Board clone = new Board();

        for (int x=0; x<8; x++)
        {
            for (int z=0; z<8; z++)
            {
                ChessPiece p = board[x, z];

                clone.board[x, z] = new ChessPiece
                {
                    pieceType = p.pieceType,
                    pieceColor = p.pieceColor,
                };
            }
        }

        return clone;
    }

    // CPUの頭脳の中で行われるシミュレーション
    public void ApplyMove(Vector2Int from, Vector2Int to, PieceType? promotoTo = null /* enum型の要素にPieceTypeがある */)
    {
        board[to.x, to.y] = board[from.x, from.y];

        ChessPiece piece = GetChessPiece(from);     // Pawn昇格

        // 元の位置はnoneに
        SetChessPiece(from, new ChessPiece
        {
            pieceType = PieceType.none,
            pieceColor = PieceColor.none
        });

        //board[from.x, from.y] = new ChessPiece
        //{
        //    pieceType = PieceType.none,
        //    pieceColor = PieceColor.none,
        //};

        if (piece.pieceType == PieceType.pawn && TryMove.IsPromotionRank(to, piece.pieceColor))
        {
            // CPUの昇格処理->一旦Queen
            SetChessPiece(to, new ChessPiece
            {
                // 普通は昇格先は決まっているが、なんらかのエラーでnullになったらQueenに昇格
                pieceType = promotoTo ?? PieceType.queen, // CPUは固定でOK
                pieceColor = piece.pieceColor
            });
        }

        // 昇格しない時、他の駒の時はそのまま
        SetChessPiece(to, piece);

    }

}