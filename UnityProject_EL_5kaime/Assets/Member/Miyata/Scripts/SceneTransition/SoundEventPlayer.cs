using Miyashita.Audio;
using UnityEngine;

namespace SceneFlow
{
    /// <summary>
    /// Play() を呼ぶと、指定したIDのSEを SoundManager で鳴らす。
    /// UnityEvent は SoundManager.Instance を直接呼べないため、この部品を経由してつなぐ。
    /// SceneLoader に AudioSource は持たせず、合図(UnityEvent)の接続先として使う。
    /// </summary>
    public class SoundEventPlayer : MonoBehaviour
    {
        [SerializeField, Tooltip("SoundCatalog に登録したSEのID")] private string _soundId = "se.six_seven";
        [SerializeField, Tooltip("SoundManager が未配置の場合に生成するPrefab。配置済みなら使わない(重複は自動で破棄される)")] private GameObject _fallbackPrefab;

        /// <summary>SEを1回鳴らす(UnityEvent・ボタンの OnClick から呼ぶ)。</summary>
        public void Play()
        {
            if (SoundManager.Instance == null && _fallbackPrefab != null)
            {
                Instantiate(_fallbackPrefab);
            }

            // 未配置・未登録・Clip空の場合は何もしない(SoundManager 側が false を返す)
            SoundManager.Instance?.PlaySE(_soundId);
        }
    }
}
