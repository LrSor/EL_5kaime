using System;
using UnityEngine;
using UnityEngine.Events;

namespace SceneFlow
{
    /// <summary>
    /// ロード画面のバーが指定の％に達した時に発火するイベント。
    /// SceneLoader の Inspector で並べて使う(音などを鳴らす合図)。
    /// </summary>
    [Serializable]
    public class ProgressTrigger
    {
        private const float PercentMax = 100f;

        [SerializeField, Range(0f, PercentMax), Tooltip("発火する%(0〜100)。バーの表示値がこの値に達した時に1回だけ発火する")] private float _percent = 50f;
        [SerializeField, Min(0f), Tooltip("到達した位置でバーを止める秒数。0なら止めない。ロードは裏で進み続ける")] private float _holdSeconds; // 到達した位置でバーを止める秒数(0なら止めない)
        [SerializeField, Tooltip("到達時に呼ぶ処理。SEなどを接続する")] private UnityEvent _onReached;

        // 1回のロードにつき1回だけ発火するための記録(シリアライズしない)
        [NonSerialized] private bool _isFired;

        /// <summary>ロード開始時に呼び、発火済みの記録を戻す。</summary>
        public void Reset()
        {
            _isFired = false;
        }

        /// <summary>
        /// 表示中のバー値(0〜1)がしきい値以上になった時、未発火なら1回だけ発火する。
        /// 発火した場合は止める秒数を返す(発火しなかった場合は 0)。
        /// </summary>
        public float Check(float shown)
        {
            if (_isFired || shown * PercentMax < _percent) return 0f;
            _isFired = true;
            _onReached.Invoke();
            return _holdSeconds;
        }
    }
}
