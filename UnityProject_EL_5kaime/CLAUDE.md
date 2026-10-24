# CLAUDE.md

このファイルは、このリポジトリで作業する Claude Code (claude.ai/code) 向けのガイドです。

## 言語

このプロジェクトでは、ユーザーとのやり取り(返答・説明・質問)を日本語で行うこと。CLAUDE.md も日本語で書く。

## プロジェクト

Unity 6 (`6000.3.14f1`) の 2D ゲームジャム用プロジェクト(URP 17.3、Input System 1.19、Cinemachine 3.1 を導入済み)。Unity プロジェクトのルートはこのディレクトリで、git リポジトリのルートは 1 つ上の階層にある。CLI からのビルド・テスト手順はなく、Unity Editor で開くか、`unity` プラグインのスキル(例: `unity:unity-cli`)で Editor を操作する。`com.unity.test-framework` は導入されているが、テストも `.asmdef` も存在しないため、すべてのスクリプトは `Assembly-CSharp` にコンパイルされる。

## 作業フォルダ

Claude の作業(ファイルの作成・編集)は `Assets/UesugiNaoki/` 内でのみ行う。他メンバーのフォルダの内容は気にせず、編集もしないこと。

## ディレクトリ構成

作業は `Assets/` 配下でメンバーごとに分かれており、それぞれが自分のシーンとスクリプトを持つ。シーンや Prefab(Unity の YAML)はマージが非常に難しいため、変更は自分のフォルダ内にとどめること。以下は他メンバーのフォルダも含む参考情報。

- `Assets/Main/` — 統合用シーン(`testMain.unity`)
- `Assets/Hamada/` — プレイヤー(`PlayerController`。従来の `Input.GetAxisRaw` を使う `NORMAL` / `SIXSEVEN` のステートマシン)
- `Assets/Member/Imai_UI/` — スコア・タイマーのシステムと UI
- `Assets/Member/Miyata/` — シーン遷移の仕組み(`Scenes/SceneTransition/` に `Scene_Title`、`Scene_Game`、`Scene_Result`)

コード中のコメントやツールチップは日本語で書かれている。近くを編集するときは合わせること。

## アーキテクチャ

**シーン遷移(`namespace SceneFlow`、`Assets/Member/Miyata/Scripts/SceneTransition/`)**: 静的なファサードと、ロード画面を持つ実装に分かれている。
- ゲーム側のコードが呼ぶ入口は `SceneTransition.Go(SceneReference)` のみ。遷移先の検証、遷移中の呼び出しの無視、`OnBeforeTransition` / `OnAfterTransition` の通知(購読側の例外は捕まえるので遷移が止まらない)、遷移完了までの `TransitionInputBlocker` による入力停止を担当する。
- シーンの読み込み自体は行わず、登録された `ISceneLoader` に任せる。実装は `SceneLoader` で、`DontDestroyOnLoad` の常駐シングルトン。`[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` で `Resources/PF_SceneLoader.prefab` から生成されるため、どのシーンから Play しても存在する。Prefab が無い場合、`Go` は警告ログを出して何もしない。
- `SceneReference`(エディタ専用の `SceneReferenceDrawer` 付き)で、Inspector からシーンを指定できる。遷移先は Build Settings に登録されている必要があり、未登録だと `Go` は遷移しない。
- `SceneTransitionButton` / `SceneTransitionTrigger` は `Go` を呼ぶ貼り付け式のコンポーネント。`ProgressTrigger` はロードバーが指定%に達したときに `UnityEvent` を発火させるもので、効果音を鳴らす接続口になる。
- `GameScore` はゲームシーンからリザルトシーンへスコアを渡す静的な置き場(`ResultScoreView` が読む)。新しいゲームを始めるときは `GameScore.Reset()` を呼ぶ。

**スコア・タイマー(`Assets/Member/Imai_UI/Script/`)**: `ScoreSystem` はシーン内シングルトン(`Instance`)でスコアを保持する。`ScoreAdd` / `ScoreMinus` は `ScoreUI.Instance.UpdateUI()` を直接呼ぶため、シーンに `ScoreUI` が必要。`ScoreSystem` は `GameScore` には書き込まない。ゲームを終了させる側が、`SceneTransition.Go` を呼ぶ前に `ScoreSystem.Instance.GetScore()` の値を `GameScore.Value` へ写すこと。

## Git

Git の操作(コミット、push、PR、マージなど)はユーザー自身が行う。Claude は依頼されない限り、git のコミット・push・PR 作成・マージを実行しないこと。`ship-pr` スキルも、ユーザーが明示的に頼んだ場合以外は使わない。
