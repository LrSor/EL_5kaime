# 敵の移動と当たり判定

EnemyA / EnemyB は、足元の CircleCollider2D で壁や家具にぶつかります。
徘徊・追跡の両方が障害物を避けて移動し、到達できない目的地へは進みません。

- 再生時に `Enemy Navigation 2D` が同じシーンに自動生成されます。追加パッケージや事前の Bake は不要です。
- `Floor` という名前の Tilemap を歩行範囲に使用します。別名の床を使う場合は、EnemyNavMesh2D をシーンに追加して Floor に指定してください。
- 壁・家具には、Trigger ではない Collider2D を付けてください。TilemapCollider2D と CompositeCollider2D にも対応します。
- Enemy の Wander Settings → Obstacle Layers が、徘徊・追跡と経路生成に共通の障害物レイヤーです。
- 足元の位置と大きさは CircleCollider2D の Offset / Radius で調整します。EnemyMovement2D はその設定を使用します。
- Collider の移動や有効・無効の変更は、標準では1秒ごとに経路へ反映します。衝突による停止はその場で行います。
- 再生中に敵を選択すると、現在の経路を水色の線で確認できます。

EnemyMovement2D の Use Nav Mesh をオフにすると、迂回せずに目的地へ進み、障害物に接触すると停止・壁沿いに移動します。
