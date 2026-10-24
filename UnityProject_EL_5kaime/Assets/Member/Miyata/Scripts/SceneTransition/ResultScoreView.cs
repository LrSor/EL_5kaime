using UnityEngine;
using UnityEngine.UI;

namespace SceneFlow
{
    /// <summary>リザルトシーンで、GameScore のスコアをテキストに表示する。</summary>
    public class ResultScoreView : MonoBehaviour
    {
        [SerializeField, Tooltip("スコアを表示するテキスト")] private Text _label;
        [SerializeField, Tooltip("表示の書式。{0} にスコアが入る")] private string _format = "SCORE {0}";

        private void Start()
        {
            if (_label == null) return;
            _label.text = string.Format(_format, GameScore.Value);
        }
    }
}
