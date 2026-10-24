using UnityEngine;

public abstract class EffectObject : MonoBehaviour
{
    protected EffectPool pool;

    public EffectData EffectData { get; private set; }

    public void Initialize(
        EffectPool pool,
        EffectData effectData)
    {
        this.pool = pool;
        EffectData = effectData;
    }

    public abstract void Play();

    protected void Release()
    {
        pool.Release(this);
    }
}
