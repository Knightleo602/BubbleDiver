using System;

[Serializable]
public enum BubbleSize
{
    Big,
    Medium,
    Small,
    Random,
}

public static class BubbleEnumExtensions
{
    public static BubbleSize GetRandomBubbleSize()
    {
        var values = Enum.GetValues(typeof(BubbleSize));
        return (BubbleSize)values.GetValue(UnityEngine.Random.Range(0, values.Length - 1));
    }
}

