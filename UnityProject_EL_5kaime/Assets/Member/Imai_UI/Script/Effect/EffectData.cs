using UnityEngine;

[CreateAssetMenu(menuName = "EffectData")]
public class EffectData : ScriptableObject
{
    [SerializeField]
    private EffectType effectType;

    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private int initialPoolSize = 5;


    public EffectType EffectType => effectType;
    public GameObject Prefab => prefab;
    public int InitialPoolSize => initialPoolSize;
}
