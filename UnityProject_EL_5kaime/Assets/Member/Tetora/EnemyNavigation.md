# 敵の移動と当たり判定

EnemyA / EnemyB は、足元の CircleCollider2D で壁や家具にぶつかります。
徘徊・追跡の両方が障害物を避けて移動し、到達できない目的地へは進みません。

Enemy本体の原点は画像の下中央（足元）です。EnemyA / EnemyB の全アニメーションも下中央のPivotで統一しています。
プレイヤーも足元が原点なので、追跡は足元同士を合わせます。近距離検知・徘徊範囲の中心も同じ原点です。
足元のCircleCollider2DはOffset Y = 0.35 / Radius = 0.35で、円の下端を画像の足元に合わせています。
半径を変更するときは、円の下端を足元に合わせたい場合にOffset Yも同じ値へ変更してください。

視界の起点はEnemyのVision Offsetで足元からの位置を調整します。EnemyA / EnemyBはY = 4.1で顔付近に合わせています。
半透明の表示・検知・壁による遮断は同じ起点を使い、プレイヤー画像の中央を見て判定します。追跡先は足元のままです。

- 再生時に `Enemy Navigation 2D` が同じシーンに自動生成されます。追加パッケージや事前の Bake は不要です。
- `Floor` という名前の Tilemap を歩行範囲に使用します。別名の床を使う場合は、EnemyNavMesh2D をシーンに追加して Floor に指定してください。
- 壁・家具には、Trigger ではない Collider2D を付けてください。TilemapCollider2D と CompositeCollider2D にも対応します。
- Enemy の Wander Settings → Obstacle Layers が、徘徊・追跡と経路生成に共通の障害物レイヤーです。
- 足元の位置と大きさは CircleCollider2D の Offset / Radius で調整します。EnemyMovement2D はその設定を使用します。
- Collider の移動や有効・無効の変更は、標準では1秒ごとに経路へ反映します。衝突による停止はその場で行います。
- 再生中に敵を選択すると、現在の経路を水色の線で確認できます。

EnemyMovement2D の Use Nav Mesh をオフにすると、迂回せずに目的地へ進み、障害物に接触すると停止・壁沿いに移動します。
