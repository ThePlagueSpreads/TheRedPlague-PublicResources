using Newtonsoft.Json;

namespace TheRedPlague.Data.BucketLootTable;

[System.Serializable]
public struct BucketEntry<T>
{
    [JsonProperty]
    public T[] Values { get; private set; }
    [JsonProperty]
    public int Amount { get; private set; }

    public BucketEntry(T value, int amount)
    {
        Values = new[] { value };
        Amount = amount;
    }
    
    [JsonConstructor]
    public BucketEntry(T[] values, int amount)
    {
        Values = values;
        Amount = amount;
    }
}