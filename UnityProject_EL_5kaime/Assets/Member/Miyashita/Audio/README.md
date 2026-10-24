# サウンド管理

BGM・SE・VoiceをIDで呼び出す、2Dサウンド用の共通窓口です。

## 最初の設定

1. `Prefabs/SoundManager.prefab` を、最初に起動するシーンのHierarchyに配置します。
2. 音素材を `Clips/BGM`、`Clips/SE`、`Clips/Voice` に入れます。
3. `Data/SoundCatalog.asset` を選び、各項目のClipに素材をドラッグします。
4. 再生を指示するスクリプトから、登録したIDを指定します。

PrefabにはCatalog・Mixer・3つのAudioSourceが設定済みです。シーン間で保持され、重複配置された場合は後から生成された方を破棄します。テスト用シーンを直接起動する場合もPrefabを配置してください。カメラ等にAudioListenerが1つ必要です。

## 素材の登録

| 項目 | 意味 |
| --- | --- |
| Id | 呼び出し名。大文字・小文字を区別し、全カテゴリで一意にします |
| Category | BGM / SE / Voice |
| Clip | 音素材。未登録でも動作します |
| Volume | 素材ごとの音量（0～1） |
| Loop | BGM・Voiceのループ。SEでは無視します |

初期データは `bgm.main`（ループ）、`ui.confirm`、`voice.start` の3項目です。Clipは空なので、素材を割り当てるまで鳴りません。Entriesを増やして、セリフごとにIDを追加できます。

## Mixerとは

Mixerは、音の種類ごとに音量をまとめて調整するものです。今回の `Mixer/MainMixer.mixer` は次の構成です。

```text
Master（すべての音）
├─ BGM（背景音楽）
├─ SE（効果音）
└─ Voice（声・セリフ）
```

例えば「BGMを小さくして、声を聞き取りやすくする」場合は、BGMだけの音量を下げます。Masterを下げると、BGM・SE・Voiceすべてが小さくなります。

| 担当 | 役割 |
| --- | --- |
| SoundCatalog | 音のIDと素材、カテゴリ、素材ごとの音量を登録する |
| SoundManager | いつ、どの音を再生・停止するかという指示を受け取る |
| AudioSource | 登録した音素材を実際に再生する |
| Mixer | 再生された音を種類ごとにまとめ、音量を調整する |

ゲーム中の音量変更は、SoundManagerを通してMixerへ反映します。

```csharp
SoundManager.Instance?.SetMasterVolume(0.8f); // 全体を80%に
SoundManager.Instance?.SetBGMVolume(0.5f);    // BGMだけ50%に
SoundManager.Instance?.SetSEVolume(1f);       // SEは100%に
SoundManager.Instance?.SetVoiceVolume(1f);    // 声は100%に
```

値は0～1です。素材ごとのVolume、カテゴリの音量、Masterの音量は組み合わさって適用されます。設定画面のSliderを使う場合はMin Valueを0、Max Valueを1にし、シーン上のSoundManagerの対応する `Set...Volume(float)` をOn Value Changedの動的floatに指定できます。音量設定の保存は別途必要です。

## BGMを切り替える

切り替えたいタイミングで `PlayBGM("登録したID")` を1回呼びます。音声ファイルを毎回自分で読み込むコードは不要で、SoundCatalogに設定済みのAudioClipを使用します。

### 1. 曲を登録する

`Data/SoundCatalog.asset` のEntriesに、例えば次の2項目を追加します。以下のIDは説明用なので、使う前に自分で登録してください。

| Id | Category | Clip | Loop |
| --- | --- | --- | --- |
| `bgm.title` | BGM | タイトル用の音素材 | オン |
| `bgm.game` | BGM | ゲーム中の音素材 | オン |

### 2. 場面に応じて呼び出す

次の例は `TitleAudioExample.cs` として保存し、タイトルシーンの有効なGameObjectに追加する使い方です。SoundManager Prefabも配置してください。

```csharp
using UnityEngine;
using Miyashita.Audio;

public class TitleAudioExample : MonoBehaviour
{
    private void Start()
    {
        // タイトルシーンの開始時に1回再生。
        SoundManager.Instance?.PlayBGM("bgm.title");
    }

    public void OnGameStarted()
    {
        // ゲーム開始処理から、このメソッドを呼ぶ。
        SoundManager.Instance?.PlayBGM("bgm.game");
    }
}
```

`OnGameStarted` は名前だけで自動実行されるものではありません。ゲーム開始処理から呼ぶか、開始ボタンのOn Clickに設定してください。別シーンへ移動してから曲を変えたい場合は、移動先のスクリプトのStartで `PlayBGM("bgm.game")` を呼ぶ方法もあります。

現在の仕様は次のとおりです。

- 新しいBGMを指定すると、前のBGMを止め、新しい曲を最初から再生します。
- 同じIDをもう一度指定した場合も、最初から再生します。
- Updateなどで毎フレーム呼ぶと繰り返し先頭に戻るため、場面が変わったときなどに1回だけ呼んでください。
- シーンを切り替えるだけではBGMは止まりません。曲を継続したい場合は再度呼び出す必要はありません。
- 止めたいときは `SoundManager.Instance?.StopBGM();` を呼びます。
- フェードやクロスフェードは実装していないため、曲は即座に切り替わります。

## 外部で用意した声を任意のタイミングで鳴らす

声素材の録音・生成・音声合成は、このプログラムでは行いません。別途用意した音声ファイルをUnityに入れ、Voice機能で再生します。

### 1. 声素材を登録する

1. 用意した音声ファイル（例：WAV）を `Clips/Voice` に入れます。
2. `Data/SoundCatalog.asset` を選択します。
3. 既存の `voice.start` に素材を割り当てるか、Entriesに項目を追加します。
4. CategoryをVoice、Clipを用意した素材、Volumeを任意の音量、Loopをオフにします。

セリフが増えたら、例えば `voice.clear`、`voice.miss` のように別々のIDで登録します。ファイル名とIDを一致させる必要はありません。

### 2. イベントが発生したときに呼び出す

再生の基本は次の1行です。ゲーム開始、クリア、ボタン操作など、好きなタイミングの処理内に書きます。

```csharp
SoundManager.Instance?.PlayVoice("voice.start");
```

ボタンから呼び出す場合は、例えば次のコードを `VoiceEventExample.cs` として保存し、シーンの有効なGameObjectに追加します。

```csharp
using UnityEngine;
using Miyashita.Audio;

public class VoiceEventExample : MonoBehaviour
{
    public void PlayStartVoice()
    {
        SoundManager.Instance?.PlayVoice("voice.start");
    }

    public void StopVoice()
    {
        SoundManager.Instance?.StopVoice();
    }
}
```

ButtonのOn Clickにこのコンポーネントを付けたGameObjectを指定し、`VoiceEventExample.PlayStartVoice()` を選ぶと、クリックしたタイミングで声が流れます。止めるボタンには `VoiceEventExample.StopVoice()` を指定できます。

現在の仕様は次のとおりです。

- Loopがオフなら、声は最後まで流れると自然に終了します。
- 声の再生中に別の声を呼ぶと、前の声を止めて新しい声を再生します。
- 同じ声をもう一度呼んだ場合も、最初から再生します。
- Voiceの再生・停止では、BGMやSEは止まりません。
- 途中で止める場合は `SoundManager.Instance?.StopVoice();` を呼びます。
- 「前のセリフが終わるまで待つ」「順番にセリフを流す」処理は含まれていません。必要になったら会話管理側で制御します。
- BGMと同じく、Updateなどから毎フレーム再生を指示しないでください。

## 音が鳴らないとき

次を確認してください。

1. シーンに有効なSoundManager Prefabがあり、SoundCatalogとMixerが割り当てられているか。
2. IDのつづり・大文字・小文字がSoundCatalogと一致しているか。
3. Clipに素材を割り当てているか。初期状態はすべて空です。
4. `PlayBGM` にはBGM、`PlayVoice` にはVoiceなど、呼び出しとCategoryが一致しているか。
5. 素材のVolume、カテゴリの音量、Masterの音量が0になっていないか。
6. カメラなどに有効なAudioListenerがあるか。

`SoundManager.Instance?.` の `?.` は、SoundManagerが存在しない場合に呼び出しを省略する記法です。エラーは防げますが、その場合は音も鳴りません。再生の呼び出しは、SoundManagerのAwakeが完了した後のStartやユーザー操作などで行ってください。

登録に問題がないかをコードで確認したい場合は、戻り値を利用できます。

```csharp
if (SoundManager.Instance == null)
{
    Debug.LogWarning("SoundManagerをシーンに配置してください。");
}
else if (!SoundManager.Instance.PlayVoice("voice.start"))
{
    Debug.LogWarning("音のID・Category・Clipの登録を確認してください。");
}
```

trueは再生指示を受け付けたことを表し、実際に聞こえていることまで保証するものではありません。未登録IDやClipが空の場合はfalseを返し、現在再生中の音を変えません。

## APIの早見表・呼び出し例

```csharp
using Miyashita.Audio;

// SoundManagerのAwake完了後（Startやボタン操作など）に呼び出す。
SoundManager.Instance?.PlayBGM("bgm.main");
SoundManager.Instance?.PlaySE("ui.confirm");
SoundManager.Instance?.PlayVoice("voice.start");

// カテゴリをCatalogから判断させる場合。
SoundManager.Instance?.Play("ui.confirm");

SoundManager.Instance?.StopVoice();
SoundManager.Instance?.StopBGM();
SoundManager.Instance?.StopSE();
SoundManager.Instance?.StopAll();

SoundManager.Instance?.SetMasterVolume(0.8f);
SoundManager.Instance?.SetBGMVolume(0.5f);
SoundManager.Instance?.SetSEVolume(1f);
SoundManager.Instance?.SetVoiceVolume(1f);
```

- BGMとVoiceは、それぞれ前の音を止めて新しい音を再生します。同じIDも最初から再生します。
- SEはPlayOneShotで重ねて再生します。StopSEは再生中のSEをまとめて止めます。
- 未登録ID、Clipが空、カテゴリ違いの呼び出しはfalseを返し、現在再生中の音を変えません。
- 音量はMaster → BGM / SE / VoiceのMixerグループで調整します。0は-80dBです。
- 音量の保存、フェード、会話の順番待ち、字幕、3D音響はこの初期構成には含めていません。

## キー操作で試聴するテストコード

`Scripts/SoundManagerTester.cs` は、自分で用意した素材を再生して動作を確認する手動テスト用コンポーネントです。自動テストや音声生成は行いません。プロジェクトで使用しているInput Systemに対応しています。

### 準備

1. シーンに `Prefabs/SoundManager.prefab` を配置します。
2. 空のGameObjectを作り、名前を `SoundTest` にします。
3. `SoundManagerTester.cs` をSoundTestにドラッグして追加します。
4. `Data/SoundCatalog.asset` のClipに素材を設定します。初期データでは `bgm.main` がBGM、`ui.confirm` がSE、`voice.start` がVoiceです。
5. BGM切り替えも試す場合は、SoundCatalogに `bgm.second` を追加し、CategoryをBGM、Clipを別の曲、Loopをオンにします。
6. カメラ等にAudioListenerがあることを確認して、Unityの再生ボタンを押します。
7. Gameビューをクリックしてから、下のキーを押します。

音素材をフォルダへ入れただけでは再生できません。必ずSoundCatalogのClipに割り当ててください。別のIDを使用する場合は、テストコード内の `"bgm.main"` などの文字列を変更します。登録不足やSoundManagerがない場合は音が鳴りません。この簡易版ではConsoleへの通知は行いません。

### 操作

| キー（数字はキーボード上段） | 動作 |
| --- | --- |
| 1 | BGMを再生（初期ID: `bgm.main`） |
| 2 | 別のBGMに切り替え（初期ID: `bgm.second`、追加登録が必要） |
| 3 | SEを再生（初期ID: `ui.confirm`） |
| 4 | 声を再生（初期ID: `voice.start`） |
| B | BGMだけ停止 |
| V | 声だけ停止 |
| X | BGM・SE・声をすべて停止 |
| M | Master音量を0にする（再生位置は進みます） |
| N | Master音量を100%にする（以前の音量を復元する機能ではありません） |

1→2で曲の切り替え、1→4でBGMと声の同時再生、3を何度か押してSEの重なり、4→Vで声の途中停止、M→Nで音量変更を確認できます。キーを押した瞬間だけ実行するので、押しっぱなしで再生し直し続けることはありません。

### コードの読み方

`Update()` は毎フレーム実行されますが、`wasPressedThisFrame` はキーを押した瞬間だけtrueになります。

```csharp
// 4キーを押したら、SoundManagerに声の再生をお願いする。
if (keyboard.digit4Key.wasPressedThisFrame)
{
    sound.PlayVoice("voice.start");
}
```

`sound` はシーン内のSoundManager、`keyboard` はキーボードです。各キーの処理はすべて「押したか確認 → SoundManagerを呼ぶ」の形です。テストコードはキー操作専用にしています。UIボタンから声を鳴らす例は、上の「外部で用意した声を任意のタイミングで鳴らす」を参照してください。

確認が終わったらSoundTestを無効化するか、テスト用シーンだけに配置してください。

## ファイル構成

```text
Audio/
├─ Scripts/   SoundManager.cs / SoundCatalog.cs / SoundManagerTester.cs
├─ Data/      SoundCatalog.asset
├─ Prefabs/   SoundManager.prefab
├─ Mixer/     MainMixer.mixer
└─ Clips/     BGM / SE / Voice
```
