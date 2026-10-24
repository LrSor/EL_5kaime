using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour
{
    public static EffectPool Instance { get; private set; }

    [SerializeField]
    private List<EffectData> effectDatas = new();

    private readonly Dictionary<EffectData, Queue<EffectObject>> pools
        = new();
    private readonly Dictionary<string, EffectData> effectDataDictionary
    = new();


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
            Register(data);
        }
    }

    private void Register(EffectData data)
    {
        if (data == null || pools.ContainsKey(data))
        {
            return;
        }

        effectDataDictionary.TryAdd(data.EffectId, data);
        pools.Add(data, new Queue<EffectObject>());
        for (int i = 0; i < data.InitialPoolSize; i++)
        {
            CreateEffect(data);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private EffectObject CreateEffect(EffectData data)
    {
        GameObject obj = Instantiate(
            data.Prefab,
            transform
        );

        EffectObject effect =
            obj.GetComponent<EffectObject>();

        effect.Initialize(
            this,
            data
        );

        obj.SetActive(false);

        pools[data].Enqueue(effect);

        return effect;
    }


    public EffectObject Get(EffectData data)
    {
        // Enemyなどから直接指定されたEffectDataも利用できるようにする。
        Register(data);

        // 使用可能なEffectObjectがなければ追加生成
        if (pools[data].Count == 0)
        {
            CreateEffect(data);
        }

        EffectObject effect =
            pools[data].Dequeue();

        effect.gameObject.SetActive(true);

        return effect;
    }


    public void Release(EffectObject effect)
    {
        effect.gameObject.SetActive(false);

        effect.transform.SetParent(transform);

        pools[effect.EffectData].Enqueue(effect);
    }


    public void Play(
        EffectData data,
        Vector3 position)
    {
        EffectObject effect = Get(data);

        effect.transform.position = position;

        effect.Play();
    }
    public void Play(
    string effectId,
    Vector3 position)
    {
        if (!effectDataDictionary.TryGetValue(
            effectId,
            out EffectData data))
        {
            Debug.LogError(
                $"EffectDataが見つかりません : {effectId}"
            );

            return;
        }

        Play(data, position);
    }
}
