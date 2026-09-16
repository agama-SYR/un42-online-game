# UN42 / 課題 No.03 オンラインゲーム

Python サーバーと Unity の最小オンラインゲームです。サーバーはプレイヤーの座標を共有します。Unity では WASD または矢印キーで青いキューブを動かし、ほかの Unity プレイヤーは黄色で表示されます。

公開サーバー: https://un42-online-game.onrender.com  
Render コンソール: https://dashboard.render.com/web/srv-dal069tg1s2s73dpa58g

## Unity で確認

この PC の Unity プロジェクトで Assets/Scenes/SampleScene.unity を開き、Play します。オンラインゲームのオブジェクトはスクリプトで自動生成されます。サーバー URL は公開 URL に設定済みです。青いキューブを WASD で動かしてください。

別のブラウザーで公開サーバー URL を開き、ページの矢印ボタンを押します。Unity の赤いキューブが移動します。Unity を二つ起動して同じ URL に接続すると、相手のキューブも表示されます。

## Render で確認

Render コンソールで Web Service が Live であることと、Online game server listening on 0.0.0.0:10000 のログを確認します。公開 URL の /health は {"status":"ok"} を返します。教官には公開サーバー URL を渡してください。

GitHub リポジトリにはサーバーと Unity クライアントスクリプトを公開しています。Unity プロジェクト全体はこの PC のワークスペースにあります。サーバーの座標はメモリ保存なので、Render が再起動するとリセットされます。
