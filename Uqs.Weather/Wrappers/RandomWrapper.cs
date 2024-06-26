namespace Uqs.Weather.Wrappers;

public class RandomWrapper : IRandowWrapper
{
    private readonly Random _random = Random.Shared;
    
    public int Next(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }
}