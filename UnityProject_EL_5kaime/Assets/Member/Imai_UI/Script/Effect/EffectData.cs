using UnityEngine;

[CreateAssetMenu(menuName = "EffectData")]
public class EffectData : ScriptableObject
{

    [SerializeField]
    private string effectId;

    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private int initialPoolSize = 5;

    public string EffectId => effectId;
    public GameObject Prefab => prefab;
    public int InitialPoolSize => initialPoolSize;
}
