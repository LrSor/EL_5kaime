using System;
using UnityEngine;

namespace Miyashita.Audio
{
    public enum SoundCategory { BGM, SE, Voice }

    [CreateAssetMenu(fileName = "SoundCatalog", menuName = "Miyashita/Audio/Sound Catalog")]
    public sealed class SoundCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            [Tooltip("呼び出しに使う一意の名前（例: ui.confirm）")]
            public string id;
            public SoundCategory category;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            [Tooltip("BGM・Voice用。SEは常に単発再生です。")]
            public bool loop;
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        public bool TryGet(string id, out Entry entry)
        {
            entry = null;
            if (string.IsNullOrWhiteSpace(id) || entries == null) return false;
            foreach (var candidate in entries)
            {
                if (candidate != null && candidate.id == id)
                {
                    entry = candidate;
                    return true;
                }
            }
            return false;
        }

        private void OnValidate()
        {
            var ids = new System.Collections.Generic.HashSet<string>();
            if (entries == null) return;
            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id)) continue;
                if (!ids.Add(entry.id))
                    Debug.LogWarning($"SoundCatalog: ID '{entry.id}' が重複しています。最初の登録を使用します。", this);
            }
        }
    }
}
