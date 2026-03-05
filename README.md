# First-Person Chess Simulator (一人称チェス)

## 概要
全世界で人気のあるボードゲームのチェスをモチーフにしたゲームです </br>
プレイヤーが操作するのは、チェスの駒ですが自駒の移動範囲内でしかプレイヤーは盤面を見ることが出来ません </br>
旧作の一人称チェスを作り直しました </br>
-> 以前のGithub URL: https://github.com/ho6ho6/ho66Games_FPSChess </br>

## ゲーム概要
プレイヤーはチェスの駒の視点で盤面を見ることになり、駒の移動可能範囲がそのまま視野として表現される独自ルールを採用しています。</br>
例えばナイトは視野が広く、ルークは正面のみを見渡せるなど、駒ごとに異なる視界の中で戦略を考えるゲーム性を実装しました。</br>
対戦相手にはAIを実装しており、Minimax法とαβ探索による思考アルゴリズムを用いて難易度を調整しています。</br>
ゲームは開始から終了までプレイ可能で、BGM・効果音・3DモデルなどのアセットはSkyboxを除きすべて自作しています。</br>

![FirstPersonChessGame](./img/FirstPersonChessGame.png)


### ビルド/実行方法
FirstPersonChessSimulator.exe を実行する - UnityRoomにも投稿してあります </br>
UnityRoomのURLです </br>
https://unityroom.com/games/ho66games_fpschess </br>

## 設計と実装ポイント
- スクリプトを分けて役割を明確にして開発を行いやすくする努力をしました。
- ゲーム全体の雰囲気を損なわなく、プレイヤーの気を散らさないような音楽を作りました。

## 動作環境
Windows11

## 使用技術
- Unity        ゲーム製作
- Blender      モデリング
- MuseScore4   BGM製作

## 今後の改善案
- UIの華やかさ
