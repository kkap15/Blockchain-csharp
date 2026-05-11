using MiniChain.Core.Services;
using MiniChain.Core.Utilities;

namespace MiniChain.Tests;

public class MerkleTreeTests
{
    private static Transaction Tx(string to, decimal amount) =>
        new(new string('0', 64), to, amount, isCoinbase: false);

    [Fact]
    public void ComputeRoot_IsDeterministic()
    {
        var txs = new[] { Tx("alice", 10m), Tx("bob", 20m) };

        var root1 = MerkleTree.ComputeRoot(txs);
        var root2 = MerkleTree.ComputeRoot(txs);

        Assert.Equal(root1, root2);
    }

    [Fact]
    public void ComputeRoot_SingleTransaction_EqualsLeafHash()
    {
        var tx = Tx("alice", 10m);
        var expected = HashingUtils.Sha256Hex(tx.SignablePayload());

        var root = MerkleTree.ComputeRoot([tx]);

        Assert.Equal(expected, root);
    }

    [Fact]
    public void ComputeRoot_EmptyList_ReturnsZeroHash()
    {
        var root = MerkleTree.ComputeRoot([]);

        Assert.Equal(new string('0', 64), root);
    }

    [Fact]
    public void ComputeRoot_TwoTransactions_EqualsPairHash()
    {
        var tx1 = Tx("alice", 10m);
        var tx2 = Tx("bob", 20m);
        var h1 = HashingUtils.Sha256Hex(tx1.SignablePayload());
        var h2 = HashingUtils.Sha256Hex(tx2.SignablePayload());
        var expected = HashingUtils.Sha256Hex(h1 + h2);

        var root = MerkleTree.ComputeRoot([tx1, tx2]);

        Assert.Equal(expected, root);
    }

    [Fact]
    public void ComputeRoot_ChangingOneTransaction_ChangesRoot()
    {
        var tx1 = Tx("alice", 10m);
        var tx2 = Tx("bob", 20m);
        var tx2Tampered = Tx("bob", 99m);

        var original = MerkleTree.ComputeRoot([tx1, tx2]);
        var tampered = MerkleTree.ComputeRoot([tx1, tx2Tampered]);

        Assert.NotEqual(original, tampered);
    }
}