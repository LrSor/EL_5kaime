namespace SceneFlow
{
    /// <summary>
    /// ゲームシーンからリザルトシーンへスコアを受け渡すための置き場。
    /// ゲーム側が Value に書き込み、リザルト側(ResultScoreView)が読む。シーンをまたいで値が残る。
    /// </summary>
    public static class GameScore
    {
        /// <summary>直近のゲームのスコア。</summary>
        public static int Value { get; set; }

        /// <summary>新しいゲームを始める時に呼んで 0 に戻す。</summary>
        public static void Reset()
        {
            Value = 0;
        }
    }
}
