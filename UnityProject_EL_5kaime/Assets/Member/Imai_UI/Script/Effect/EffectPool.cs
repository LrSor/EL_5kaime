using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour
{
    public static EffectPool Instance { get; private set; }

    [SerializeField]
    private List<EffectData> effectDatas;

    // EffectTypeごとの待機中エフェクト
    private readonly Dictionary<EffectType, Queue<EffectObject>> pools = new();

    // EffectTypeごとの設定データ
    private readonly Dictionary<EffectType, EffectData> dataDictionary = new();
    
    private void Awake()
    {
        // シングルトン化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Initialize();
    }


    private void Initialize()
    {
        foreach (EffectData data in effectDatas)
        {
            dataDictionary.Add(
                data.EffectType,
                data
            );

            pools.Add(
                data.EffectType,
                new Queue<EffectObject>()
            );

            // 初期生成
            for (int i = 0; i < data.InitialPoolSize; i++)
            {
                CreateEffect(data.EffectType);
            }
        }
    }


    private EffectObject CreateEffect(EffectType type)
    {
        EffectData data = dataDictionary[type];

        GameObject obj = Instantiate(
            data.Prefab,
            transform
        );

        EffectObject effect = obj.GetComponent<EffectObject>();

        effect.Initialize(
            this,
            type
        );

        obj.SetActive(false);

        pools[type].Enqueue(effect);

        return effect;
    }


    public EffectObject Get(EffectType type)
    {
        // 全部使用中なら追加生成
        if (pools[type].Count == 0)
        {
            CreateEffect(type);
        }

        EffectObject effect = pools[type].Dequeue();

        effect.gameObject.SetActive(true);

        return effect;
    }


    public void Release(EffectObject effect)
    {
        effect.gameObject.SetActive(false);

        effect.transform.SetParent(transform);

        pools[effect.GetEffectType()].Enqueue(effect);
    }


    public void Play(
        EffectType type,
        Vector3 position)
    {
        EffectObject effect = Get(type);

        effect.transform.position = position;

        effect.Play();
    }
}
