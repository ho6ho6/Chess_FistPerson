using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class manager_game : MonoBehaviour
{
    /* ---初期設定--- */

    // Debug Mode
    bool isDebugMode;

    // playerが操作する色 titleシーンで決定される
    bool isWhite;

    // 難易度設定
    Difficult difficult;

    // turn管理
    PieceColor currentTurn;

    int randomTurn;

    // 選択された駒
    Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard;

    // 盤面座標
    Vector2Int from;
    Vector2Int tempTargetPos;

    // 駒は選択されている？
    piece_Controller selectedPiece;
    [SerializeField] Camera godCam;
    
    // Pawnの昇格用変数
    private Vector2Int promotionPos;

    // manager_gameのインスタンス
    public static manager_game Instance { get; private set; }

    // Playerの状態
    PlayerState playerState;

    // 配置されたときのSE設定
    [SerializeField] AudioSource seSource;
    [SerializeField] AudioClip SEmove;

    /* ---初期設定--- */


    /* ---game画面で使用されるスクリプト--- */

    GameToEnd gameToEnd;
    BoardCell boardCell;
    Board     board;
    piece_Library piece_library;
    UI_ShowMove ui_ShowMove;
    UI_ReticleController ui_ReticleCtr;
    UI_Promotion ui_Promotion;
    SkyboxAudio skyboxAudio;
    private manager_CPU cpu;

    /* ---game画面で使用されるスクリプト--- */

    // インスタンス生成　UI_Promotionのみに参照される
    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // game画面で使用される全てのスクリプトを取得
        gameToEnd     = GetComponent<GameToEnd>();
        boardCell     = GetComponent<BoardCell>();
        piece_library = GetComponent<piece_Library>();
        ui_ShowMove   = GetComponent<UI_ShowMove>();
        ui_ReticleCtr = GetComponent<UI_ReticleController>();
        ui_Promotion  = GetComponent<UI_Promotion>();
        skyboxAudio   = GetComponent<SkyboxAudio>();


        // 各駒の個性を取得
        piece_Factory.Init(piece_library);
        pieceCtrOnBoard = piece_library.GetPieceControllers();

        // title画面で選択された色を取得
        isWhite = title.GetIsWhite();
        Debug.Log("ゲーム開始: " + (isWhite ? "白" : "黒"));

        // DebugMode
        isDebugMode = title.GetIsDebugMode();
        if (isDebugMode)
            Debug.Log("デバック用で開始");

        // ゲームモード取得
        bool isCastleMode = title.IsCastleMode();
        Debug.Log("ゲームモード: " + (isDebugMode ? "籠城戦開始" : "クラシックモード"));

        // 難易度取得
        difficult = title.GetDifficult();
        Debug.Log($"難易度は{difficult}が選択された");

        // 世界の生成
        skyboxAudio.SetSky(skyboxAudio.DrawRandomSky(difficult));

        // 各モジュールの初期化処理
        Debug.Log("Board Start");
        Debug.Log("Piece Start");
        board = new Board();
        board.SetUpBoard(boardCell, pieceCtrOnBoard, isCastleMode);

        // turnの初期化
        randomTurn = Random.Range(1, 11);
        if (randomTurn % 2 == 0)
            currentTurn = PieceColor.white;
        else
            currentTurn = PieceColor.black;

        // レティクル初期化
        ui_ReticleCtr.SetReticleActive(false);

        // カーソル設定
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playerState = PlayerState.GodView;

        Debug.Log($"先攻は{currentTurn}");

        // titleで選択した色とは反対のものをCPUに
        cpu = new manager_CPU(!isWhite ? PieceColor.white : PieceColor.black);
        
        // CPUの色で先攻になったら
        if (cpu.cpuColor == currentTurn)
        {
            StartCPUTurn();
        }

    }


    // Update is called once per frame
    void Update()
    {
        UpdateGodCamView();

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Debug.Log("Switch Turn");
            SwitchTurn();
        }

        // playerのターンでなければ何もしない
        if (!IsPlayerTurn()) return;

        switch (playerState)
        {
            case PlayerState.GodView:
                HandleGodViewInput();
                break;

            case PlayerState.FirstPersonAim:
                HandleAimInput();
                break;

            case PlayerState.ConfirmMove:
                HandleConfirmInput();
                break;
        }

    }


    bool IsPlayerTurn()
    {
        return currentTurn == (isWhite ? PieceColor.white : PieceColor.black);
    }

    void SwitchTurn()
    { 
        currentTurn = (currentTurn == PieceColor.white) ? PieceColor.black : PieceColor.white;

        // CPUの操作開始
        if (!IsPlayerTurn()) StartCPUTurn();
    }

    /* ---右クリックされた1プロセスずつ戻る 俯瞰視点が終点--- */

    // 駒選択が成功したら
    void OnPieceSelected(piece_Controller pc)
    {
        Vector2Int from = pc.boardPos;
        ChessPiece piece = board.GetChessPiece(from);

        var movable = ruleMove.GetMovableCells(piece, from, board);
        ui_ShowMove.ShowMove(movable, boardCell);
    }

    // 俯瞰視点から
    void HandleGodViewInput()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // 駒選択が自分のものなら
        if (ruleSelect.TrySelectMinePiece(
            board,
            currentTurn,
            pieceCtrOnBoard,
            out selectedPiece))
        {
            Debug.Log($"selectedPiece instanceID = {selectedPiece.GetInstanceID()}");
            Debug.Log($"selectedPiece.boardPos = {selectedPiece.boardPos}");
            foreach (var kv in pieceCtrOnBoard)
            { 
                Debug.Log($"dict key = {kv.Key}, instanceID = {kv.Value.GetInstanceID()}, boardPos = {kv.Value.boardPos}");
            }

            // 駒選択成功
            OnPieceSelected(selectedPiece);

            ui_ReticleCtr.SetReticleActive(true);

            godCam.gameObject.SetActive(false);
            selectedPiece.ActiveCamera();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            ShowMovableCells(selectedPiece);
            playerState = PlayerState.FirstPersonAim;
        }

        return;
    }

    // 一人称視点へ
    void HandleAimInput()
    {

        // 右クリックされたら 神視点へ
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            selectedPiece.notActiveCamera();
            godCam.gameObject.SetActive(true);

            ui_ReticleCtr.SetReticleActive(false);

            playerState = PlayerState.GodView;
            return;
        }

        // 左クリックでターゲット選択
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // 駒のカメラ取得
        Camera cam = selectedPiece.GetActiveCamera();
        if (cam == null)
        {
            Debug.LogError("Active camera is null");
            return;
        }

        // 一人称になったら
        ui_ReticleCtr.SetReticleActive(true);

        // 画面中央 Raycast
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, 200f))
            return;

        var cell = hit.collider.GetComponent<BoardCell>();
        if (cell == null)
            return;

        Vector2Int to = cell.boardPos;
        Debug.Log($"Clicked board pos: {to.x},{to.y}");
        Vector2Int from = selectedPiece.boardPos;

        // boardからChessPieceを取得
        ChessPiece piece = board.GetChessPiece(from);

        if (!ruleMove.CanMove(piece, from, to, board))
        {
            Debug.Log($"Cannot move from {from} to {to}");
            return;
        }

        tempTargetPos = to;

        playerState = PlayerState.ConfirmMove;

    }

    // 移動先を決定する？
    void HandleConfirmInput()
    {
        // --- キャンセル（エイムに戻る） ---
        if (Mouse.current.rightButton.wasPressedThisFrame) playerState = PlayerState.FirstPersonAim;

        // 移動先が良いなら 決定
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {

            MoveResult moveResult = TryMove.Execute(
                selectedPiece.boardPos,
                tempTargetPos,
                boardCell,
                board,
                pieceCtrOnBoard
            );

            if (!moveResult.moved)
            {
                Debug.Log("move失敗");
                return;
            }

            // 移動先の結果が、盤面最奥(最前)かプレーヤーが昇格を選んでいる最中ならば
            if (moveResult.needPromotion)
            {
                Debug.Log("moveResultのneedPromotionを行います");

                promotionPos = selectedPiece.boardPos;

                // Pawn昇格UI表示
                ui_ReticleCtr.ShowPromotionUI();

                playerState = PlayerState.PromotionSelect;
                return; // このreturnは昇格が完了するまでゲームを止める意味
            }
            Debug.Log("moveResultのneedPromotionは終わりました。または、通ってません");

            // ハイライト消去とレティクルも
            ui_ShowMove.Clear();                    // 移動可能マス表示
            ui_ReticleCtr.SetReticleActive(false);  // レティクル表示

            // 視点リセット
            selectedPiece.notActiveCamera();
            godCam.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            selectedPiece = null;

            if (moveResult.isKingCaptured)
            {
                gameToEnd.GameToEnd_Update();
                return; // ターン切り替えは不要
            }

            seSource.PlayOneShot(SEmove);

            SwitchTurn();
            playerState = PlayerState.GodView;

        }
    }

    // 神視点では自分の色しかみれない
    void UpdateGodCamView()
    {
        if (godCam == null) return;

        string[] layers;

        if (isDebugMode)
        {
            layers = new string[] { "white", "black", "Stage_Cell" };
        }
        else
        {
            layers = isWhite ? new string[] { "white", "Stage_Cell" } : new string[] { "black", "Stage_Cell" };
        }

        int mask = 0;
        foreach (var layer in layers)
        {
            int layerIndex = LayerMask.NameToLayer(layer);
            if (layerIndex >= 0)
                mask |= (1 << layerIndex);
        }

        godCam.cullingMask = mask;
    }

    /* ---右クリックされた1プロセスずつ戻る 俯瞰視点が終点--- */



    /* ---Pawnの昇格--- */

    // UIボタンから呼ばれる
    public void OnSelectPromotion(PieceType type)
    {
        Debug.Log($"Promotion selected: {type}");

        // Pawn → 選択駒に昇格
        TryMove.PromotePawn(
            promotionPos,
            type,
            boardCell,
            board,
            pieceCtrOnBoard
        );

        ui_ReticleCtr.HidePromotionUI();        // Pawn昇格UI非表示

        // 共通後処理（ターン切り替えなど）
        AfterMoveCommon();
    }

    void AfterMoveCommon()
    {
        ui_ShowMove.Clear();

        if (selectedPiece != null)
            selectedPiece.notActiveCamera();

        godCam.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        selectedPiece = null;

        seSource.PlayOneShot(SEmove);

        SwitchTurn();
        playerState = PlayerState.GodView;
    }

    /* ---Pawnの昇格--- */



    /* ---CPUの動き--- */

    void ShowMovableCells(piece_Controller pc)
    {
        ChessPiece piece = board.GetChessPiece(pc.boardPos);
        List<Vector2Int> movableCells = new List<Vector2Int>();

        for (int x=0; x<8; x++)
        {
            for (int z=0; z<8; z++)
            {
                Vector2Int target = new Vector2Int(x, z);
                if (ruleMove.CanMove(piece, pc.boardPos, target, board)) movableCells.Add(target);
            }
        }
    }

    // CPUターンの開始
    void StartCPUTurn()
    {
        StartCoroutine(WaitCPUMove());
    }

    // CPUを動かす前に数秒待つ Update()で呼び出すと、遅延した分の処理が押し寄せCPUのターンが複数続く
    IEnumerator WaitCPUMove()
    {
        Debug.Log("CPUは1秒待った");
        yield return new WaitForSeconds(1.5f);  // 1.5秒 Nightmareモードだけ演算に時間がかかったため、その違和感を少なくしたい
        Debug.Log("CPU行動開始");
        HandleCPUTurn();
    }

    void HandleCPUTurn()
    {
        MoveResult result = cpu.CPUThinkMove(boardCell, board, pieceCtrOnBoard, difficult);

        if (result.isKingCaptured)
        {
            gameToEnd.GameToEnd_Update();
            return;
        }

        if (result.needPromotion)
        {
            TryMove.PromotePawn(
                result.promotionPos,
                result.promotionType,
                boardCell,
                board,
                pieceCtrOnBoard
                );
        }

        seSource.PlayOneShot(SEmove);
        SwitchTurn();
    }

    /* ---CPUの動き--- */
}