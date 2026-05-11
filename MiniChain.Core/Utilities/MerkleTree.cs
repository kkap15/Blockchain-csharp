using MiniChain.Core.Interface;

namespace MiniChain.Core.Utilities;

public static class MerkleTree
{
    public static string ComputeRoot(IReadOnlyList<ITransaction> transactions)
    {
        if (transactions is null || transactions.Count == 0)
        {
            return new string('0', 64);
        }

        var leafLayer = transactions
            .Select(t => HashingUtils.Sha256Hex(t.SignablePayload()))
            .ToList();
        while (leafLayer.Count > 1)
        {
            if (leafLayer.Count % 2 != 0)
            {
                leafLayer.Add(leafLayer.Last());
            } 
            var nextLayer = new List<string>();
            for (var i = 0; i < leafLayer.Count; i += 2)
            {
                nextLayer.Add(HashingUtils.Sha256Hex(leafLayer[i] + leafLayer[i + 1]));
            }
            leafLayer = nextLayer;
        }
        
        return leafLayer[0];
    }
}