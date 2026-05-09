namespace MiniChain.Core;

public static class Miner
{
    public static string? Mine(Block block, int difficulty)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(difficulty);
        block.Nonce = 0;
        var targetString = new string('0', difficulty);
        string? hash;
        while (true)
        {
            hash = block.ComputeHash();
            if (hash.StartsWith(targetString))
            {
                break;
            }
            block.Nonce++;
        }
        return hash;
    }
    
    public static bool MeetsTarget(string hash, int difficulty)
    {
        if (hash.StartsWith(new string('0', difficulty)))
        {
            return true;
        }
        return false;
    }
}