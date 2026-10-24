using UnityEngine;

namespace Disco
{
    /// <summary>メインカメラの XY に追従する。子の演出は、カメラ中心からの相対位置のまま動く。</summary>
    public class DiscoFollowCamera : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;

        private void LateUpdate()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) return;
            var c = targetCamera.transform.position;
            transform.position = new Vector3(c.x, c.y, transform.position.z);
        }
    }
}
