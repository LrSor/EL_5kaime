using UnityEngine;
using UnityEngine.InputSystem;

//---------------これはテストコードです-------------------//
namespace Miyashita.Audio
{
    public class SoundManagerTester : MonoBehaviour
    {
        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            SoundManager sound = SoundManager.Instance;

            // キーボードやSoundManagerがない場合は何もしない。
            if (keyboard == null || sound == null) return;

            // 1キー：BGMを再生
            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                sound.PlayBGM("bgm.main");
            }

            // 2キー：別のBGMに切り替え
            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                sound.PlayBGM("bgm.second");
            }

            // 3キー：効果音を再生
            if (keyboard.digit3Key.wasPressedThisFrame)
            {
                sound.PlaySE("ui.confirm");
            }

            // 4キー：声を再生
            if (keyboard.digit4Key.wasPressedThisFrame)
            {
                sound.PlayVoice("voice.start");
            }

            // Bキー：BGMだけ停止
            if (keyboard.bKey.wasPressedThisFrame)
            {
                sound.StopBGM();
            }

            // Vキー：声だけ停止
            if (keyboard.vKey.wasPressedThisFrame)
            {
                sound.StopVoice();
            }

            // Xキー：すべて停止
            if (keyboard.xKey.wasPressedThisFrame)
            {
                sound.StopAll();
            }

            // Mキー：全体の音量を0にする
            if (keyboard.mKey.wasPressedThisFrame)
            {
                sound.SetMasterVolume(0f);
            }

            // Nキー：全体の音量を100%にする
            if (keyboard.nKey.wasPressedThisFrame)
            {
                sound.SetMasterVolume(1f);
            }
        }
    }
}
