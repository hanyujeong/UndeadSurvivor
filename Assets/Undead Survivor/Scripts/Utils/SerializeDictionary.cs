using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializeDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private DataType[] dicData;

    [Serializable]
    public struct DataType
    {
        public TKey key;
        public TValue value;
    }

    public void OnBeforeSerialize()
    {
        if (!Application.isPlaying) { return; }

        RefreshDataType();
    }

    public void OnAfterDeserialize()
    {
        Clear();
        for (int i = 0; i < dicData.Length; i++)
        {
            Add(dicData[i].key, dicData[i].value);
        }
    }

    public DataType[] GetDataType()
    {
        return dicData;
    }

    public void RefreshDataType()
    {
        if (this.Count != dicData.Length) { dicData = new DataType[this.Count]; }
        int num = 0;
        foreach (var keyValuePair in this)
        {
            dicData[num++] = new DataType
            {
                key = keyValuePair.Key,
                value = keyValuePair.Value
            };
        }
    }
    
    public SerializeDictionary() { }
    public SerializeDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
    public SerializeDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary) { }
    public SerializeDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) { }
    public SerializeDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(collection, comparer) { }
    public SerializeDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
    public SerializeDictionary(int capacity) : base(capacity) { }
    public SerializeDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }
}