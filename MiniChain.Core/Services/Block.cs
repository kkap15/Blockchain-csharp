using System.Globalization;
using MiniChain.Core.Interface;
using MiniChain.Core.Utilities;

namespace MiniChain.Core.Services;

public sealed class Block(int index, DateTimeOffset timestamp, string previousHash, IReadOnlyList<ITransaction> transactions, long nonce = 0) : IBlock
{
    public int Index => index;
    public DateTimeOffset Timestamp => timestamp;
    public string PreviousHash => previousHash;
    public IReadOnlyList<ITransaction> Transactions => transactions;
    public long Nonce { get; set; } = nonce;

    public string ComputeHash()
    {
        var raw = string.Join("|",
            Index.ToString(CultureInfo.InvariantCulture),
            Timestamp.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture),
            PreviousHash,
            string.Join(',', Transactions.Select(t => t.SignablePayload())),
            Nonce.ToString(CultureInfo.InvariantCulture));
        var hash = HashingUtils.Sha256Hex(raw);
        
        return hash;
    }
    
    public static Block CreateGenesis()
    {
        return new Block(0, DateTimeOffset.FromUnixTimeSeconds(1_700_000_000), new string('0', 64), [new Transaction(new string('0', 64), "genesis-reward", 50m)]);
    }

    public override string ToString()
        => $"Block #{Index} [{ComputeHash()[..12]}...] prev={PreviousHash[..12]}... tx={Transactions.Count}";
}
