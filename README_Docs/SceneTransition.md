# シーン遷移・ロード画面の使い方

担当: 宮田  /  置き場所: `UnityProject_EL_5kaime/Assets/Member/Miyata/`

シーンを切り替えるときは、**必ずここの仕組みを通してください**。
`SceneManager.LoadScene` を直接呼ぶと、ロード画面が出ません。

---

## 1. 遷移のしかた

### 方法A: ボタンで遷移する(コード不要)

1. 遷移させたい UI の Button に `SceneTransitionButton` を追加します。
2. `Target` に遷移先のシーンをドラッグします。

### 方法B: クリア判定などのイベントから遷移する(コード不要)

1. 空のオブジェクトに `SceneTransitionTrigger` を追加し、`Target` に遷移先をドラッグします。
2. 呼び出したいイベント(UnityEvent)に、このコンポーネントの `Go()` を接続します。

### 方法C: コードから遷移する

#### 手順

1. ファイルの先頭に `using SceneFlow;` を書きます。
2. 遷移先を持つフィールドを `SceneReference` 型で宣言し、`[SerializeField]` を付けます。
3. Inspector に現れた欄へ、遷移先のシーンをドラッグします。
4. 遷移したい場所で `SceneTransition.Go(フィールド名);` を呼びます。

#### 最小の例

```csharp
using SceneFlow;
using UnityEngine;

public class GameClear : MonoBehaviour
{
    // Inspector に「Result Scene」という欄が出る。ここにシーンをドラッグする
    [SerializeField] private SceneReference _resultScene;

    // クリアした時に呼ぶ
    public void OnClear()
    {
        SceneTransition.Go(_resultScene);
    }
}
```

#### 実際のゲームに近い例(スコアを渡してリザルトへ)

```csharp
using SceneFlow;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SceneReference _resultScene;

    private int _score;

    private void Start()
    {
        GameScore.Reset(); // ゲーム開始時にスコアを 0 に戻す
    }

    public void AddScore(int point)
    {
        _score += point;
    }

    public void OnGameOver()
    {
        GameScore.Value = _score;            // リザルトに表示するスコアを渡す
        SceneTransition.Go(_resultScene);    // ロード画面を挟んでリザルトへ
    }
}
```

#### 知っておくこと

- **`Go` はすぐ戻ります。** 遷移の完了を待たずに次の行へ進みます。`Go` の直後に書いた処理は、ロード画面が出ている最中に実行されます。
  遷移後に何かしたい場合は、`Go` の下に書かず、`OnAfterTransition` を使ってください(下記)。

- **遷移できない場合は、`Go` は何もしません。** 遷移中、遷移先が未指定、Build Settings に未登録、ロード画面が無い、のいずれかのとき、Console に警告が出るか、黙って無視されます。例外は出ません。
- **`Go` を何度呼んでも大丈夫です。** 遷移中の呼び出しは無視されるので、「クリア判定が複数フレーム続けて true になる」ような場合でも、二重には遷移しません。
- **`SceneReference` は `[SerializeField]` で宣言します。** `new SceneReference()` で作っても、シーンは指定できません(エディタ上でシーンアセットをドラッグする型のため)。
- **シーン名の文字列では指定できません。** `SceneTransition.Go("Scene_Result")` のような書き方はできません。必ず `SceneReference` を使います。

#### 遷移の前後に処理を挟む

`SceneTransition` のイベントを購読します。**購読したら、必ず解除してください。** 解除し忘れると、破棄されたオブジェクトが呼ばれてエラーになります。

```csharp
using SceneFlow;
using UnityEngine;

public class TransitionListener : MonoBehaviour
{
    private void OnEnable()
    {
        SceneTransition.OnBeforeTransition += HandleBefore;
        SceneTransition.OnAfterTransition += HandleAfter;
    }

    private void OnDisable()
    {
        SceneTransition.OnBeforeTransition -= HandleBefore;
        SceneTransition.OnAfterTransition -= HandleAfter;
    }

    // 遷移を始める直前(ロード画面が出る前)
    private void HandleBefore(SceneReference scene)
    {
        Debug.Log($"{scene.SceneName} へ遷移します");
    }

    // 新しいシーンの初期化が一巡し、ロード画面が消え始める直前
    private void HandleAfter(SceneReference scene)
    {
        Debug.Log($"{scene.SceneName} の読み込みが終わりました");
    }
}
```

- **遷移後のイベントは、新しいシーンでは受け取れません。** 遷移で破棄されるオブジェクトが購読していると、`OnAfterTransition` の時点では、そのオブジェクトはすでに無くなっています。遷移をまたいで動くオブジェクト(`DontDestroyOnLoad` のもの、BGM 管理など)で購読してください。
- 購読側で例外が出ても、遷移は止まりません(例外は Console に出ます)。

#### 状態を調べる

| プロパティ | 内容 |
|---|---|
| `SceneTransition.IsTransitioning` | 遷移中(ロード画面の表示中)なら `true`。この間は他の操作を止めたいときに使う |
| `SceneTransition.HasLoader` | ロード画面が登録済みなら `true` |
| `SceneTransition.IsInputBlockEnabled` | 遷移中に入力を止めるか。既定は `true` |

```csharp
// 遷移中は、ポーズメニューを開かせない例
if (SceneTransition.IsTransitioning) return;
```

`IsInputBlockEnabled` を `false` にすると、遷移中も入力が止まりません。止める必要がある特別な場面でだけ使い、使い終わったら `true` に戻してください。

### 共通の注意

- 遷移先のシーンを **Build Settings に登録**してください。未登録なら警告が出て、遷移しません。
  (`Target` の欄に警告と追加ボタンが出ます)

- 遷移中にもう一度呼んでも無視されます(二重遷移の防止)。
- 遷移中は、キーボード・マウス・パッドの入力が止まります。遷移が終わると自動で戻ります。

### 遷移中の入力について(旧入力を使っている人へ)

遷移中は、新しい Input System のキーボード・マウス・パッドを自動で止めます。
**旧入力(`Input.GetAxisRaw`、`Input.GetKeyDown` など `UnityEngine.Input`)は止まりません。**
旧入力を使うスクリプトは、遷移中の入力を自分で無視してください。

```csharp
private void Update()
{
    if (SceneTransition.IsTransitioning) return; // 遷移中は入力を受け付けない

    float h = Input.GetAxisRaw("Horizontal");
    // ...
}
```

遷移中に動くのは、これから破棄されるシーンのオブジェクトだけです。放っておいても大きな問題にはなりにくいですが、
遷移中の Space 押下でアクションが暴発するような処理がある場合は、上の一行を入れてください。
新しい Input System への移行を検討する場合は、この対処は不要になります。

---

## 2. 遷移の流れ

```
SceneTransition.Go()
  → OnBeforeTransition(遷移前の通知)
  → フェードイン(ロード画面が現れる)
  → バーが 0 → 100%
  → シーン切り替え
  → OnAfterTransition(ロード画面が消え始める直前の通知)
  → フェードアウト
  → 入力が戻る
```

遷移前後に処理を挟みたいときは、イベントを購読します。

```csharp
SceneTransition.OnBeforeTransition += scene => { /* 遷移前 */ };
SceneTransition.OnAfterTransition  += scene => { /* 遷移後 */ };
```

購読を解除する場合は `-=` で外してください(オブジェクトの破棄時など)。

---

## 3. スコアのリザルトへの受け渡し

ゲーム側が `GameScore.Value` に書き込み、リザルトシーンの `ResultScoreView` が表示します。

```csharp
GameScore.Reset();            // ゲーム開始時に 0 に戻す
GameScore.Value += 100;       // スコア加算
SceneTransition.Go(result);   // リザルトへ
```

- リザルトシーンの `Label_Score` に `ResultScoreView` が付いています。表示の書式は `Format` で変えられます(既定 `SCORE {0}`)。
- `GameScore` はアプリを終了すると消えます。ハイスコアの保存は別に必要です。

---

## 4. ロード画面の編集

見た目は `Resources/PF_SceneLoader.prefab` を開いて編集します。

```
PF_SceneLoader
└─ LoadingCanvas   ← フェード対象(Canvas Group)
   ├─ Background   背景
   ├─ Label        「NOW LOADING」の文字
   ├─ PercentLabel 進行%の表示
   └─ ProgressBar  進捗バー
```

ルートの `SceneLoader` の項目(カーソルを合わせると説明が出ます):

| 項目 | 内容 |
|---|---|
| Canvas Group / Progress Bar / Percent Label | 参照。**消さないこと**(Percent Label は空なら%を出さない) |
| Fade Time | フェードの秒数 |
| Min Display Time | バーが 0→1 になる最低秒数 |
| On Load Started | ロード開始時の合図 |
| On Progress Filled | バーが 100% になった時の合図(途中で止めた場合はそのぶん遅れる) |
| On Fade Out Started | ロード画面が消え始める時の合図 |
| Progress Triggers | バーが指定%に達した時の合図と、その位置で止める時間 |

### 指定%で処理を呼ぶ・止める(Progress Triggers)

「+」で要素を追加し、次を設定します。

| 項目 | 内容 |
|---|---|
| Percent | 発火する%(0〜100) |
| Hold Seconds | その位置でバーを止める秒数(0なら止めない) |
| On Reached | 到達時に呼ぶ処理 |
| On Hold Finished | 止まりが終わった時に呼ぶ処理(止めている間だけ画像を出す場合に、非表示にする処理を接続する) |

- 現在の設定: 67% で1秒止まり、その間だけ `HoldImage`(`Prefabs/PF_Man67.prefab`)が出ます。到達時に `HoldImage` を表示し、止まりが終わると非表示にしています。
- `PF_Man67` は、背景(`Textures/Man67_Background.jpg`)と、左右の手(`Man67_HandLeft.png`、`Man67_HandRight.png`)でできています。表示されている間、手が交互に上下します(`HandSwing`。1秒で2往復、振れ幅 150)。
- 背景は、画面を覆うように拡大され、縦横比は崩れません(はみ出す部分は切れます)。画像を差し替える場合は、各 Image の Source Image を変えます。
- 各要素は、1回のロードにつき1回だけ発火します。
- 止めている間もロードは裏で進みます。止めたぶん、遷移全体は長くなります。

---

## 5. 効果音(SE)を鳴らす

音の基盤は別担当のため、`SceneLoader` は音を直接持ちません。
上の合図(On Load Started など、Progress Triggers の On Reached)に、鳴らす処理を接続してください。

注意:

- 遷移先のシーンに `AudioListener` が無いと、遷移直後は無音になります。
- `timeScale` が 0 のとき(ポーズ中など)に鳴らす場合は、`AudioSource` の `Ignore Listener Pause` を ON にしてください。

---

## 6. 動作確認用のシーン

`Assets/Member/Miyata/Scenes/SceneTransition/` に Title → Game → Result の確認用シーンがあります。

| シーン | 内容 |
|---|---|
| Scene_Title | 「ゲームへ」で Game へ |
| Scene_Game | 5秒後に自動でリザルトへ(仮スコア 1234 を書き込む)。ボタンでも遷移可 |
| Scene_Result | スコア表示。「もう一度」で Game へ、「タイトルへ」で Title へ。右下の「67」ボタンで、`PF_Man67` が1秒だけ背景に出る(手が上下する) |

`Scene_Title` から Play すると一通り確認できます。

---

## 7. ファイル一覧

`Assets/Member/Miyata/Scripts/SceneTransition/`

| ファイル | 役割 |
|---|---|
| SceneTransition.cs | 遷移の窓口(`Go`、前後の通知、二重遷移の防止) |
| SceneReference.cs | Inspector でシーンをドラッグして指定するための型 |
| SceneLoader.cs | ロード画面の本体(フェード、バー、%、合図) |
| ProgressTrigger.cs | 指定%での合図と停止 |
| ISceneLoader.cs | ロード画面側が満たす窓口 |
| TransitionInputBlocker.cs | 遷移中の入力停止 |
| SceneTransitionButton.cs / SceneTransitionTrigger.cs | ボタン・イベントから遷移するための部品 |
| GameScore.cs / ResultScoreView.cs | スコアの受け渡しと表示 |
| Editor/SceneReferenceDrawer.cs | `SceneReference` の Inspector 表示 |
| HandSwing.cs | 左右の手を交互に上下させる(`PF_Man67` で使用) |
| TimedImageToggle.cs | `Show()` で指定のオブジェクトを一定時間だけ表示する(リザルトの「67」ボタンで使用) |
| Demo/CountdownEvent.cs | 確認用(実際のゲームでは使わない) |

---

## 8. 困ったとき

| 症状 | 確認すること |
|---|---|
| 遷移しない・警告が出る | 遷移先が Build Settings に登録されているか。`Target` が空でないか |
| 「ISceneLoader が登録されていません」と出る | `Resources/PF_SceneLoader.prefab` が存在するか |
| ロード画面が出ない | `SceneManager.LoadScene` を直接呼んでいないか |
| 遷移中にキャラが動いてしまう | 旧入力(`Input.GetAxisRaw` など)は自動では止まらない。`SceneTransition.IsTransitioning` を見て無視する |
| 遷移後にキーボードが効かないまま | 通常は自動で戻る。戻らない場合は担当(宮田)へ |
| 遷移直後に音が出ない | 遷移先シーンに `AudioListener` があるか |
