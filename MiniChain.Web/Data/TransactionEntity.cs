using System;

namespace MiniChain.Web.Data;

public class TransactionEntity
{
    public Guid Id { get; set; }
    public int BlockId { get; set; }
    public string From  { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Signature { get; set; } = string.Empty;
    public bool IsCoinbase { get; set; }
}