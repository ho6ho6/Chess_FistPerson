using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class manager_CPU
{
    public PieceColor cpuColor;

    public manager_CPU(PieceColor color)
    {
        cpuColor = color;
    }

    // CPU専用 Pawn昇格
    static readonly PieceType[] PromotionAll =
    {
        PieceType.queen,
        PieceType.rook,
        PieceType.bishop,
        PieceType.knight
    };

    // Easy専用 Pawn昇格構造体
    struct ScoredMove
    {
        public Vector2Int from;
        public Vector2Int to;
        public PieceType? promotion;
        public int score;
    }

    public MoveResult CPUThinkMove(BoardCell boardCell, Board board, Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard, Difficult difficult)
    {
        // C# 8.0以降で使えるswitch文らしい
        // 普通のとは異なり、値を返せるswitch文
        return difficult switch
        {
            // depth = 1 評価するだけ ->自分
            Difficult.Easy =>
                ThinkEasy(boardCell, board, pieceCtrOnBoard),

            // depth = 2 1手先を考えれる ->自分-相手
            Difficult.Normal =>
                ThinkNormal(boardCell, board, pieceCtrOnBoard),

            // depth = 3 優秀なCPU ->自分-相手-自分
            Difficult.Hard =>
                ThinkHard(boardCell, board, pieceCtrOnBoard),

            // depth = 4 ヤバい ->自分-相手-自分-相手
            Difficult.Nightmare =>
                ThinkNightmare(boardCell, board, pieceCtrOnBoard),

            _ => new MoveResult { moved = false }
        };
    }


    /* ---CPU動作の探索--- */

    /*
        [余談]
        Chessの平均的な合法手数は30~40
        depthによる再帰呼び出し数では
        depth=1 => 葉の数 30
        depth=2 => 葉の数 900
        depth=3 => 葉の数 27000

        depth=4以降はとんでもない
        HardやNightmareはdepth数を増やす代わりに、Alpha-Beta探索で削減を試みる
    */

    // 各駒の価値
    int PieceValue(ChessPiece piece)
    {
        switch (piece.pieceType)
        {
            case PieceType.pawn:    return 100;
            case PieceType.knight:  return 320;
            case PieceType.bishop:  return 330;
            case PieceType.rook:    return 500;
            case PieceType.queen:   return 900;
            case PieceType.king:    return 50000;
            default: return 0;
        }
    }

    // Pawn-Promotion適用
    PieceType[] GetPromotionCandidates()
    {
        return PromotionAll;
    }

    // 難易度別探索アルゴリズム

    // easy -- 全合法手 -> 評価 -> 上位50%からランダムに
    MoveResult ThinkEasy(
        BoardCell boardCell,
        Board board,
        Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard)
    {
        var cpuPieces = board.GetAllCPUPieces(cpuColor);
        var moves = new List<ScoredMove>();

        Debug.Log("Easy探索");

        foreach (var from in cpuPieces)
        {
            ChessPiece piece = board.GetChessPiece(from);

            for (int x = 0; x < 8; x++)
            {
                for (int z = 0; z < 8; z++)
                {
                    Vector2Int to = new Vector2Int(x, z);
                    if (!ruleMove.CanMove(piece, from, to, board)) continue;

                    // 昇格あり
                    if (piece.pieceType == PieceType.pawn &&
                        TryMove.IsPromotionRank(to, piece.pieceColor))
                    {
                        foreach (var promo in GetPromotionCandidates())
                        {
                            Board next = board.Clone();
                            next.ApplyMove(from, to, promo);

                            int score = Evaluate(next, cpuColor);

                            moves.Add(new ScoredMove
                            {
                                from = from,
                                to = to,
                                promotion = promo,
                                score = score
                            });
                        }
                    }
                    // 昇格なし
                    else
                    {
                        Board next = board.Clone();
                        next.ApplyMove(from, to);

                        int score = Evaluate(next, cpuColor);

                        moves.Add(new ScoredMove
                        {
                            from = from,
                            to = to,
                            promotion = null,
                            score = score
                        });
                    }
                }
            }
        }

        if (moves.Count == 0)
            return new MoveResult { moved = false };

        // スコア降順
        moves.Sort((a, b) => b.score.CompareTo(a.score));

        // 上位25%からランダム
        int range = Mathf.Max(1, moves.Count / 4);
        var chosen = moves[Random.Range(0, range)];

        // 実行
        MoveResult result = TryMove.Execute(
            chosen.from,
            chosen.to,
            boardCell,
            board,
            pieceCtrOnBoard
        );

        // 昇格情報を後付け
        if (chosen.promotion.HasValue)
        {
            result.needPromotion = true;
            result.promotionType = chosen.promotion.Value;
            result.promotionPos = chosen.to;
        }

        return result;
    }

    // normal -- MINMAX法 深さ2  CPUは取ったら取り返されるまで想定
    MoveResult ThinkNormal(
        BoardCell boardCell, 
        Board board, 
        Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard)
    {
        var cpuPieces = board.GetAllCPUPieces(cpuColor);
        int best = int.MinValue;
        (Vector2Int from, Vector2Int to) bestMove = default;
        PieceType? bestPromotion = null;

        Debug.Log("Normal探索");

        foreach (var from in cpuPieces)
        {
            ChessPiece piece = board.GetChessPiece(from);

            for (int x=0; x<8; x++)
            {
                for (int z = 0; z < 8; z++)
                {
                    Vector2Int to = new Vector2Int(x, z);

                    if (!ruleMove.CanMove(piece, from, to, board)) continue;

                    // 昇格する
                    if (piece.pieceType == PieceType.pawn &&
                        TryMove.IsPromotionRank(to, piece.pieceColor))
                    {
                        foreach(var promo in GetPromotionCandidates())
                        {
                            Board next = board.Clone();
                            next.ApplyMove(from, to, promo);

                            int score = MINMAX(next, cpuColor, depth: 1, false);

                            if (score > best)
                            {
                                best = score;
                                bestMove = (from, to);
                                bestPromotion = promo;
                            }
                        }
                    }
                    else // 昇格しない
                    {
                        Board next = board.Clone();
                        next.ApplyMove(from, to);   // <- ここで1手読んでいる

                        int score = MINMAX(next, cpuColor, depth: 1, false);   // 深さは2だが渡す値は1

                        if (score > best)
                        {
                            best = score;
                            bestMove = (from, to);
                            bestPromotion = null;
                        }

                    }

                }
            }
        }

        if (best == int.MinValue) return new MoveResult { moved = false };

        // 演算結果反映
        MoveResult result = TryMove.Execute(bestMove.from, bestMove.to, boardCell, board, pieceCtrOnBoard);

        // 昇格情報を付与(TryMove)
        if (bestPromotion.HasValue)
        {
            result.needPromotion = true;
            result.promotionType = bestPromotion.Value;
            result.promotionPos = bestMove.to;
        }

        return result;
    }

    // Hard -- MINMAX + Alpha-Beta法 深さ3  玄人向けに
    MoveResult ThinkHard(
        BoardCell boardCell, 
        Board board, 
        Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard)
    {
        var cpuPieces = board.GetAllCPUPieces(cpuColor);
        int best = int.MinValue;
        (Vector2Int from, Vector2Int to) bestMove = default;
        PieceType? bestPromotion = null;

        Debug.Log("Hard探索");

        foreach (var from in cpuPieces)
        {
            ChessPiece piece = board.GetChessPiece(from);

            for (int x = 0; x < 8; x++)
            {
                for (int z = 0; z < 8; z++)
                {
                    Vector2Int to = new Vector2Int(x, z);

                    if (!ruleMove.CanMove(piece, from, to, board)) continue;
                    
                    // 昇格する
                    if (piece.pieceType == PieceType.pawn &&
                        TryMove.IsPromotionRank(to, piece.pieceColor))
                    {
                        foreach (var promo in GetPromotionCandidates())
                        {
                            Board next = board.Clone();
                            next.ApplyMove(from, to, promo);

                            int score = MINMAXAlphaBeta(next, cpuColor, depth: 2, false,
                                alpha: int.MinValue, beta: int.MaxValue);

                            if (score > best)
                            {
                                best = score;
                                bestMove = (from, to);
                                bestPromotion = promo;
                            }
                        }
                    }
                    else // 昇格しない
                    {
                        Board next = board.Clone();
                        next.ApplyMove(from, to);

                        int score = MINMAXAlphaBeta(
                            next, cpuColor, depth: 2, false,
                            alpha: int.MinValue, beta: int.MaxValue);   // depthの深さは3 Normal同様

                        if (score > best)
                        {
                            best = score;
                            bestMove = (from, to);
                            bestPromotion = null;   // 後に良い非昇格手が見つかった時のために
                        }
                    }
                }
            }
        }

        if (best == int.MinValue) return new MoveResult { moved = false };

        // 演算結果反映
        MoveResult result = TryMove.Execute(bestMove.from, bestMove.to, boardCell, board, pieceCtrOnBoard);

        // 昇格情報を付与(TryMove)
        if (bestPromotion.HasValue)
        {
            result.needPromotion = true;
            result.promotionType = bestPromotion.Value;
            result.promotionPos = bestMove.to;
        }

        return result;
    }

    // Nightmare -- Hard + 深さ4  ミスは許さない
    MoveResult ThinkNightmare(
        BoardCell boardCell, 
        Board board, 
        Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard)
    {
        var cpuPieces = board.GetAllCPUPieces(cpuColor);
        int best = int.MinValue;
        (Vector2Int from, Vector2Int to) bestMove = default;
        PieceType? bestPromotion = null;

        Debug.Log("Nightmare探索");

        foreach (var from in cpuPieces)
        {
            ChessPiece piece = board.GetChessPiece(from);
            for (int x = 0; x < 8; x++)
            {
                for (int z = 0; z < 8; z++)
                {
                    Vector2Int to = new Vector2Int(x, z);

                    if (!ruleMove.CanMove(piece, from, to, board)) continue;

                    // 昇格あり
                    if (piece.pieceType == PieceType.pawn &&
                        TryMove.IsPromotionRank(to, piece.pieceColor))
                    {
                        foreach (var promo in GetPromotionCandidates())
                        {
                            Board next = board.Clone();
                            next.ApplyMove(from, to, promo);

                            int score = MINMAXNightmare(next, cpuColor, 4, false, int.MinValue, int.MaxValue);

                            if (score > best)
                            {
                                best = score;
                                bestMove = (from, to);
                                bestPromotion = promo;
                            }
                        }
                    }
                    else // 昇格なし
                    {
                        Board next = board.Clone();
                        next.ApplyMove(from, to);

                        int score = MINMAXNightmare(next, cpuColor, 4, false, int.MinValue, int.MaxValue);  // depth=4
                        if (score > best)
                        {
                            best = score;
                            bestMove = (from, to);
                            bestPromotion = null;
                        }
                    }
                }
            }
        }

        if (best == int.MinValue) return new MoveResult { moved = false };

        // 演算結果反映
        MoveResult result = TryMove.Execute(bestMove.from, bestMove.to, boardCell, board, pieceCtrOnBoard);

        // 昇格情報を付与(TryMove)
        if (bestPromotion.HasValue)
        {
            result.needPromotion = true;
            result.promotionType = bestPromotion.Value;
            result.promotionPos = bestMove.to;
        }

        return result;
    }

    // easy シンプルに駒の価値で盤面を評価
    int Evaluate(Board board, PieceColor me)
    {
        int score = 0;

        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                ChessPiece piece = board.GetChessPiece(new Vector2Int(x, z));
                if (piece.IsEmpty) continue;

                int value = PieceValue(piece);
                if (piece.pieceColor == me) score += value;
                else score -= value;
            }
        }
        return score;
    }

    // 探索用に使用する駒色判断
    PieceColor Opponent(PieceColor c)
    {
        return c == PieceColor.white ? PieceColor.black : PieceColor.white;
    }

    // MINMAX isMaximizingがtrue => CPUの手番, false => Playerの手番

    // normal 駒の価値+MINMAX (残りの手(depth)を見たとき どれくらい良い盤面があるか？)
    int MINMAX(Board board, PieceColor me, int depth, bool isMaximizing)
    {
        if (depth == 0) return Evaluate(board, me);  //最初のターンがCPUだったら 適当に

        PieceColor turnColor = isMaximizing ? me : Opponent(me);

        var pieces = board.GetAllCPUPieces(turnColor);

        // 自分の葉を探索
        if (isMaximizing)
        {
            int scoreBest = int.MinValue;

            foreach (var from in pieces)
            {
                ChessPiece piece = board.GetChessPiece(from);

                for (int x = 0; x < 8; x++)
                {
                    for (int z = 0; z < 8; z++)
                    {
                        Vector2Int to = new Vector2Int(x, z);
                        if (!ruleMove.CanMove(piece, from, to, board)) continue;

                        Board next = board.Clone();
                        next.ApplyMove(from, to);

                        // 次は相手の葉を探索
                        int score = MINMAX(next, me, depth - 1, !isMaximizing);
                        // CPUの最善手
                        scoreBest = Mathf.Max(scoreBest, score);
                    }
                }
            }
            return scoreBest;
        }
        // 相手の葉を探索
        else
        {
            int scoreBest = int.MaxValue;

            foreach (var from in pieces)
            {
                ChessPiece piece = board.GetChessPiece(from);

                for (int x = 0; x < 8; x++)
                {
                    for (int z=0; z < 8; z++)
                    {
                        Vector2Int to = new Vector2Int(x, z);
                        if (!ruleMove.CanMove(piece, from, to, board)) continue;
                        
                        Board next = board.Clone();
                        next.ApplyMove(from, to);

                        // 次は自分の葉を探索
                        int score = MINMAX(next, me, depth - 1, !isMaximizing);
                        // CPUにとって最悪手
                        scoreBest = Mathf.Min(scoreBest, score);
                    }
                }
            }
            return scoreBest;
        }
    }

    // hard MINMAX + Alpha-Beta
    int MINMAXAlphaBeta(Board board, PieceColor me, int depth, bool isMaximizing, int alpha, int beta)
    {
        if (depth == 0) return Evaluate(board, me);

        PieceColor turnColor = isMaximizing ? me : Opponent(me);
        var pieces = board.GetAllCPUPieces(turnColor);

        // 探索開始
        if (isMaximizing)
        {
            int MaxAEval = int.MinValue;

            foreach (var from in pieces)
            {
                ChessPiece piece = board.GetChessPiece(from);

                for (int x=0; x<8; x++)
                {
                    for (int z=0; z<8; z++)
                    {
                        Vector2Int to = new Vector2Int(x, z);
                        if (!ruleMove.CanMove(piece, from, to, board)) continue;

                        Board next = board.Clone();
                        next.ApplyMove(from, to);

                        // 次は相手の葉を探索
                        int Aval = MINMAXAlphaBeta(next, me, depth - 1, !isMaximizing, alpha, beta);

                        MaxAEval = Mathf.Max(MaxAEval, Aval);
                        alpha = Mathf.Max(alpha, Aval);

                        if (beta <= alpha) return MaxAEval; // betaカット
                    }
                }
            }
            return MaxAEval;
        }
        // 相手の葉を探索
        else
        {
            int MinAEval = int.MaxValue;

            foreach (var from in pieces)
            {
                ChessPiece piece = board.GetChessPiece(from);

                for (int x = 0; x < 8; x++)
                {
                    for (int z = 0; z < 8; z++)
                    {
                        Vector2Int to = new Vector2Int(x, z);
                        if (!ruleMove.CanMove(piece, from, to, board)) continue;

                        Board next = board.Clone();
                        next.ApplyMove(from, to);

                        // 次は自分の葉を探索
                        int Aval = MINMAXAlphaBeta(next, me, depth - 1, !isMaximizing, alpha, beta);

                        MinAEval = Mathf.Min(MinAEval, Aval);
                        beta = Mathf.Min(beta, Aval);

                        if (alpha <= beta) return MinAEval; // alphaカット
                    }
                }
            }
            return MinAEval;
        }
    }

    // Nightmare 専用の評価関数 + MINMAX + Alpha + Beta ペナルティ付与

    // 盤面の評価 (Nightmare特有の偏りを持たせる)
    /*
        単純に強く、合理的だが
        犠牲を嫌い、長期計画に弱く、怖がり
     */

    /*
    [Nightmareの攻略法]

    1.この難易度では偏りがある
        王の安全性：重視
        中央支配：重視
        可動性：重視
        駒損：嫌う
        長期的なポーン構造：軽視
        犠牲手：基本しない

    2.各駒に与えられた価値と重み付けがある
        Pawn    : 100
        Knight  : 320
        Bishop  : 330
        Rook    : 500
        Queen   : 900
        King    : 10000

    3.駒犠牲を絶対にしない

    4.王の安全を過剰に重視する
      -無意味なキングサイド後退
      -無理な守備的手

      攻略法
      フェイント攻撃とか

    5.「未来4手以上」は見ない
       ->「5手後に崩壊する配置に誘導」とか

    6.Nightmareの性格
    特性	    内容
    冷静	    無駄な突撃をしない
    ケチ	    駒損を嫌う
    臆病	    王が危険だと逃げる
    勝負師   王が安全なら、突撃してPlayerの王を引きずり出すような行動をする
    短期思考	depth外は見ない

    7.全体スコアの構成
    TOTAL = MATERIAL + KING_SAFETY + MOBILITY + CENTER_CONTROL
            + PAWN_POSITION + PAWN_STRUCTURE + CHECK_THREATS - RISK
    */

    int MINMAXNightmare(Board board, PieceColor me, int depth, bool isMaximizing, int alpha, int beta)
    {
        if (depth == 0) return EvaluateNightmare(board, me);

        PieceColor turnColor = isMaximizing ? me : Opponent(me);

        var allMoves = GetAllLegalMoves(board, turnColor);

        // 探索開始
        if (isMaximizing)
        {
            int MaxAEval = int.MinValue;

            foreach (var move in allMoves)
            {
                Board next = board.Clone();
                next.ApplyMove(move.from, move.to);
                int AEval = MINMAXNightmare(next, me, depth - 1, !isMaximizing, alpha, beta);
                MaxAEval = Mathf.Max(MaxAEval, AEval);
                alpha = Mathf.Max(alpha, AEval);
                if (beta <= alpha) return MaxAEval; // betaカット
            }
            return MaxAEval;
        }
        // 相手の葉を探索
        else
        {
            int MinBEval = int.MaxValue;

            foreach (var move in allMoves)
            {
                Board next = board.Clone();
                next.ApplyMove(move.from, move.to);
                int BEval = MINMAXNightmare(next, me, depth - 1, !isMaximizing, alpha, beta);
                MinBEval = Mathf.Min(MinBEval, BEval);
                beta = Mathf.Min(beta, BEval);
                if (alpha <= beta) return MinBEval; // betaカット
            }
            return MinBEval;
        }
    }


    int EvaluateNightmare(Board board, PieceColor me)
    {
        PieceColor enemy = Opponent(me);
        int score = 0;

        score += EvaluateMaterial(board, me) * 12 / 10;   // 駒得絶対主義
        score += EvaluateKingSafety(board, me);
        score += EvaluateMobility(board, me);
        score += EvaluateCenter(board, me);
        score += EvaluatePawnAdvance(board, me);
        score += EvaluatePawnStructure(board, me);
        score += EvaluateCheckThreat(board, me);
        score -= EvaluateRisk(board, me);

        return score;
    }

    // 駒価値評価
    int EvaluateMaterial(Board board, PieceColor me)
    {
        int score = 0;

        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                ChessPiece p = board.GetChessPiece(new Vector2Int(x, z));
                if (p.IsEmpty) continue;

                int v = PieceValue(p);
                score += (p.pieceColor == me) ? v : -v;
            }
        }
        return score;
    }

    // 可動性の評価
    int EvaluateMobility(Board board, PieceColor me)
    {
        int myMoves = GetAllLegalMoves(board, me).Count;
        int enemyMoves = GetAllLegalMoves(board, Opponent(me)).Count;

        return (myMoves - enemyMoves) * 80;
    }

    // Kingの保護
    int EvaluateKingSafety(Board board, PieceColor me)
    {
        int score = 0;
        Vector2Int kingPos = FindKing(board, me);

        // 王の周囲8マス
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;

                Vector2Int p = kingPos + new Vector2Int(dx, dz);
                if (!board.IsInsideBoard(p.x, p.y)) continue;

                if (IsSquareAttacked(board, p, Opponent(me)))
                    score -= 120;
            }
        }

        // チェック中は激減点
        if (IsSquareAttacked(board, kingPos, Opponent(me)))
            score -= 500;

        return score * 3; // Nightmare補正
    }

    Vector2Int FindKing(Board board, PieceColor me)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                ChessPiece piece = board.board[x, z];

                if (piece.pieceType == PieceType.king &&
                    piece.pieceColor == me)
                {
                    return new Vector2Int(x, z);
                }
            }
        }

        // 見つからなかった場合 ありえないが
        return new Vector2Int(-1, -1);
    }

    // 中央を大切に
    int EvaluateCenter(Board board, PieceColor me)
    {
        Vector2Int[] centers =
        {
        new(3,3), new(3,4),
        new(4,3), new(4,4)
    };

        int score = 0;

        foreach (var c in centers)
        {
            if (IsSquareAttacked(board, c, me))
                score += 120;
        }
        return score;
    }

    // Pawnは昇格しようとする
    int EvaluatePawnAdvance(Board board, PieceColor me)
    {
        int score = 0;

        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                ChessPiece p = board.GetChessPiece(new Vector2Int(x, z));
                if (p.IsEmpty || p.pieceType != PieceType.pawn || p.pieceColor != me)
                    continue;

                int rank = (me == PieceColor.white) ? z : (7 - z);
                score += rank * 60;
            }
        }
        return score;
    }

    // 盤面に配置されているPawn陣形を評価
    int EvaluatePawnStructure(Board board, PieceColor me)
    {
        int penalty = 0;

        // NightmareCPUはPawn陣形を軽視する
        foreach (var pos in board.GetAllCPUPieces(me))
        {
            ChessPiece p = board.GetChessPiece(pos);
            if (p.pieceType != PieceType.pawn) continue;

            if (IsIsolatedPawn(board, pos))
                penalty += 20;
        }

        return -penalty;
    }

    // 短期戦略に弱い = 揺さぶりが効く
    int EvaluateCheckThreat(Board board, PieceColor me)
    {
        int score = 0;

        foreach (var move in GetAllLegalMoves(board, me))
        {
            Board next = board.Clone();
            next.ApplyMove(move.from, move.to);

            Vector2Int enemyKing = FindKing(next, Opponent(me));
            if (IsSquareAttacked(next, enemyKing, me))
                score += 150;
        }

        return score;
    }

    // 敵の有利に過敏
    int EvaluateRisk(Board board, PieceColor me)
    {
        int risk = 0;

        foreach (var pos in board.GetAllCPUPieces(me))
        {
            if (IsSquareAttacked(board, pos, Opponent(me)))
                risk += 200;
        }

        return risk;
    }

    // 捕獲の評価
    bool IsSquareAttacked(Board board, Vector2Int target, PieceColor byColor)
    {
        // 1. Pawn
        if (IsAttackedByPawn(board, target, byColor)) return true;

        // 2. Knight
        if (IsAttackedByKnight(board, target, byColor)) return true;

        // 3. Bishop / Queen（斜め）
        if (IsAttackedBySlider(board, target, byColor, bishopDirs)) return true;

        // 4. Rook / Queen（直線）
        if (IsAttackedBySlider(board, target, byColor, rookDirs)) return true;

        // 5. King
        if (IsAttackedByKing(board, target, byColor)) return true;

        return false;
    }

    // 各駒の捕獲 Pawn,Knight,King,その他
    bool IsAttackedByPawn(Board board, Vector2Int t, PieceColor byColor)
    {
        int dir = (byColor == PieceColor.white) ? -1 : 1;

        Vector2Int[] pawnAttacks =
        {
        new Vector2Int(t.x - 1, t.y + dir),
        new Vector2Int(t.x + 1, t.y + dir)
    };

        foreach (var p in pawnAttacks)
        {
            if (!board.IsInsideBoard(p.x,p.y)) continue;
            var piece = board.board[p.x, p.y];
            if (piece.pieceType == PieceType.pawn && piece.pieceColor == byColor)
                return true;
        }
        return false;
    }

    static readonly Vector2Int[] knightOffsets =
    {
    new(1,2), new(2,1), new(2,-1), new(1,-2),
    new(-1,-2), new(-2,-1), new(-2,1), new(-1,2)
    };

    bool IsAttackedByKnight(Board board, Vector2Int t, PieceColor byColor)
    {
        foreach (var o in knightOffsets)
        {
            Vector2Int p = t + o;
            if (!board.IsInsideBoard(p.x, p.y)) continue;

            var piece = board.board[p.x, p.y];
            if (piece.pieceType == PieceType.knight && piece.pieceColor == byColor)
                return true;
        }
        return false;
    }

    Vector2Int[] bishopDirs =
    {
        new(1,1), new(1,-1), new(-1,1), new(-1,-1)
    };

    Vector2Int[] rookDirs =
    {
        new(1,0), new(-1,0), new(0,1), new(0,-1)
    };

    bool IsAttackedBySlider(
    Board board,
    Vector2Int t,
    PieceColor byColor,
    Vector2Int[] dirs)
    {
        foreach (var d in dirs)
        {
            Vector2Int p = t + d;
            while (board.IsInsideBoard(p.x, p.y))
            {
                var piece = board.board[p.x, p.y];
                if (piece.pieceType != PieceType.none)
                {
                    if (piece.pieceColor == byColor &&
                        (piece.pieceType == PieceType.queen ||
                         piece.pieceType == PieceType.bishop ||
                         piece.pieceType == PieceType.rook))
                    {
                        return true;
                    }
                    break;
                }
                p += d;
            }
        }
        return false;
    }

    bool IsAttackedByKing(Board board, Vector2Int t, PieceColor byColor)
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Vector2Int p = new(t.x + dx, t.y + dy);
                if (!board.IsInsideBoard(p.x, p.y)) continue;

                var piece = board.board[p.x, p.y];
                if (piece.pieceType == PieceType.king && piece.pieceColor == byColor)
                    return true;
            }
        return false;
    }

    // Pawnの孤立を評価
    bool IsIsolatedPawn(Board board, Vector2Int pos)
    {
        ChessPiece piece = board.GetChessPiece(pos);

        if (piece.IsEmpty) return false;
        if (piece.pieceType != PieceType.pawn) return false;

        int x = pos.x;
        PieceColor color = piece.pieceColor;

        // 左右のマスを確認
        for (int dx=-1; dx<=1; dx+=2)
        {
            int cell = x + dx;
            if (cell < 0 || cell >= 8) continue;

            for (int z=0; z<8; z++)
            {
                ChessPiece p = board.GetChessPiece(new Vector2Int(cell,z));
                if (!p.IsEmpty &&
                     p.pieceType == PieceType.pawn &&
                     p.pieceColor == color)
                    return false;   // 左右どこかに自分のPawnがある
            }
        }

        return true;    // 左右にPawnがない
    }

    List<Move> GetAllLegalMoves(Board board, PieceColor me)
    {
        var moves = new List<Move>();

        var myPieces = board.GetAllCPUPieces(me);

        foreach (var from in myPieces)
        {
            ChessPiece piece = board.GetChessPiece(from);

            for (int x = 0; x < 8; x++)
            {
                for (int z = 0; z < 8; z++)
                {
                    Vector2Int to = new Vector2Int(x, z);

                    if (!ruleMove.CanMove(piece, from, to, board))
                        continue;

                    moves.Add(new Move { from = from, to = to });
                }
            }
        }

        return moves;
    }

}