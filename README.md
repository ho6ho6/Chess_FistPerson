## FPS Chess (一人称チェス)

# 概要
全世界で人気のあるボードゲームのチェスをモチーフにしたゲームです。
プレイヤーが操作するのは、チェスの駒ですが自駒の移動範囲内でしかプレイヤーは盤面を見ることが出来ません。
未だバグが多いですが、直していきます。

# ゲーム画面
![FPSChess_Debug](./img/Chess_Debug.png)
![FPSChess_play1](./img/Chess_play1.png)
![FPSChess_play2](./img/Chess_play2.png)

# ビルド/実行方法
FPS_Chess_試作品.exe を実行する - UnityRoomにも投稿してあります。
UnityRoomのURLです
https://unityroom.com/games/ho66games_fpschess

# 設計と実装ポイント
- スクリプトを分けて役割を明確にして開発を行いやすくする努力をしました。
- UnityのTagやLayerなどを使い、スクリプト内で簡潔に書けるようにしました。
- ゲーム全体の雰囲気を損なわなく、プレイヤーの気を散らさないような音楽を作りました。

# 動作環境
Windows10/11

# 使用技術
- Unity        ゲーム製作
- Blender      モデリング
- MuseScore4   BGM製作

## 今後の改善案
- UIの工夫
- 敵駒の動きに不具合がある (敵のポーンが2マス移動し続けるなど)
- 難易度選択
