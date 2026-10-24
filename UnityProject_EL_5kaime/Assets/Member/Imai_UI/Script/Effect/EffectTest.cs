using UnityEngine;
using UnityEngine.InputSystem;

public class EffectTest : MonoBehaviour
{

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("エフェクト発生");
            EffectPool.Instance.Play(
                EffectId.Six,
                transform.position + Vector3.up * 1.5f
            );
        }
    }
}
