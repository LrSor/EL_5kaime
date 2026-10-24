using UnityEngine;

namespace Disco
{
    /// <summary>拍ごとに画面上部から紙吹雪を降らせる。</summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class DiscoConfetti : MonoBehaviour
    {
        [SerializeField] private DiscoBeat beat;
        [Tooltip("1 拍あたりに出す枚数")]
        [SerializeField] private int countPerBeat = 25;
        [Tooltip("紙吹雪を出す横幅")]
        [SerializeField] private float width = 20f;
        [SerializeField] private float gravity = 0.12f;

        private ParticleSystem _ps;

        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
            var main = _ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 12f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 1000;

            var grad = new Gradient();
            grad.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.magenta, 0f), new GradientColorKey(Color.cyan, 0.33f),
                    new GradientColorKey(Color.yellow, 0.66f), new GradientColorKey(new Color(0.4f, 1f, 0.4f), 1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            main.startColor = new ParticleSystem.MinMaxGradient(grad) { mode = ParticleSystemGradientMode.RandomColor };

            var emission = _ps.emission;
            emission.enabled = false;

            var shape = _ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(width, 1f, 1f);

            var rot = _ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-3f, 3f);

            // ひらひらと横に揺らす
            var noise = _ps.noise;
            noise.enabled = true;
            noise.strengthX = 0.8f;
            noise.strengthY = 0.2f;
            noise.frequency = 0.5f;
        }

        private void OnEnable() => beat.OnBeat += HandleBeat;
        private void OnDisable() => beat.OnBeat -= HandleBeat;

        private void HandleBeat(int n) => _ps.Emit(countPerBeat);
    }
}
