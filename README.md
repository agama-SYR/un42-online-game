# UN42 / 課題 No.03 オンラインゲーム

最小構成のオンラインゲームです。Python サーバーはプレイヤーの座標を共有し、Unity では WASD または矢印キーで青いキューブを動かします。複数の Unity 実行画面を同じ URL に接続すると、ほかのプレイヤーが黄色で表示されます。サーバー URL のブラウザーページの矢印ボタンを押すと、Unity 内の赤いキューブを動かせます。

## ローカル確認

プロジェクトのルートで `python server.py` を実行します。ブラウザーで `http://localhost:10000/` を開き、Unity で `Assets/Scenes/SampleScene.unity` を Play します。Unity の画面左上の URL 欄には初期値としてローカル URL が入力されています。

## Render への公開

1. このプロジェクトを GitHub リポジトリにアップロードします。Unity の `Library`、`Temp`、`Logs`、`Obj`、`UserSettings` はアップロード不要です。Render にはルートの `server.py` と `render.yaml` が必要です。
2. Render Dashboard の **New > Blueprint** から GitHub リポジトリを選び、`render.yaml` を使って Web Service を作成します。あるいは **New > Web Service** で Language `Python 3`、Build Command `echo "No dependencies"`、Start Command `python server.py`、Health Check Path `/health` を設定します。
3. Deploy が完了したら、Render コンソールで `Online game server listening on 0.0.0.0:...` のログとサービスの Live 状態を確認します。公開 URL の `/health` が `{"status":"ok"}` を返すことを確認します。
4. Unity を Play し、画面左上の URL 欄を Render の `https://...onrender.com` に変更します。WASD で青いキューブを移動させます。別のブラウザーで Render URL を開き、矢印ボタンで赤いキューブが Unity 内で移動することを確認します。
5. 教官に Render URL を渡し、コンソール画面と Unity の動作を見せます。

サーバーの座標はメモリに保存するだけなので、Render の再起動でリセットされます。課題の最小版としての仕様です。
