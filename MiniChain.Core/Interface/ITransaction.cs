namespace MiniChain.Core.Interface;

public interface ITransaction
{
    public string From { get; }
    public string To { get; }
    public decimal Amount { get; }
    public string Signature { get; }
    public string SignablePayload();
    public void Sign(IWallet wallet);
    public bool IsValid();
    public bool IsCoinbase { get; }
}