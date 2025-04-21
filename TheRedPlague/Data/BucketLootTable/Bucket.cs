using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace TheRedPlague.Data.BucketLootTable;

[System.Serializable]
public class Bucket<T>
{
    [JsonConstructor]
    public Bucket()
    {
        InitializeWithNewData(System.Array.Empty<BucketEntry<T>>());
    }
    
    public Bucket(ICollection<BucketEntry<T>> entries)
    {
        InitializeWithNewData(entries);
    }

    [JsonProperty]
    private BucketEntry<T>[] Entries
    {
        get => _entries;
        set => _entries = value;
    }

    private BucketEntry<T>[] _entries;

    [JsonProperty]
    private List<T[]> _state;

    private void InitializeList()
    {
        _state = new List<T[]>();
        foreach (var entry in _entries)
        {
            for (int i = 0; i < entry.Amount; i++)
            {
                _state.Add(entry.Values);
            }
        }
    }

    public void InitializeWithNewData(ICollection<BucketEntry<T>> data)
    {
        _entries = new BucketEntry<T>[data.Count];
        int i = 0;
        foreach (var entry in data)
        {
            _entries[i] = entry;
            i++;
        }
        InitializeList();
    }

    public int RemainingDraws => _state.Count;

    public T DrawEntry()
    {
        var randomIndex = Random.Range(0, _state.Count);
        T[] dataAtIndex = _state[randomIndex];
        var temp = dataAtIndex[Random.Range(0, dataAtIndex.Length)];
        _state.RemoveAt(randomIndex);
        return temp;
    }
    
    public bool TryDrawEntry(out T value)
    {
        if (_state.Count == 0)
        {
            value = default;
            return false;
        }

        value = DrawEntry();
        return true;
    }

    public void Reset()
    {
        InitializeList();
    }
}