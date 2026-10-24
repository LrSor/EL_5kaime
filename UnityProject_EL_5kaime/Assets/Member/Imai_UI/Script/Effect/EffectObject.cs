using UnityEngine;

public abstract class EffectObject : MonoBehaviour
{
    protected EffectPool pool;
    protected EffectType effectType;

    public void Initialize(EffectPool pool, EffectType effectType)
    {
        this.pool = pool;
        this.effectType = effectType;
    }

    public abstract void Play();

    protected void Release()
    {
        pool.Release(this);
    }

    public EffectType GetEffectType()
    {
        return effectType;
    }
}
