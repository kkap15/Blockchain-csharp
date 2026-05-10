using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MiniChain.Core.Models;
using MiniChain.Core.Interface;

namespace MiniChain.Core.Services;

public sealed class Wallet : IWallet
{
    private readonly ECDsa _keyPair;
    public string PublicKeyHex { get; }

    private Wallet(ECDsa keyPair)
    {
        _keyPair = keyPair;
        PublicKeyHex = Convert.ToHexStringLower(_keyPair.ExportSubjectPublicKeyInfo());
    }

    public Wallet()
    {
        _keyPair = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        PublicKeyHex = Convert.ToHexStringLower(_keyPair.ExportSubjectPublicKeyInfo());
    }
    
    public string Sign(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var signature = _keyPair.SignData(bytes, HashAlgorithmName.SHA256);
        return Convert.ToHexStringLower(signature);
    }
    
    public bool Verify(string data, string signatureHex)
    {
       using var instance = ECDsa.Create();
       var signatureBytes = Convert.FromHexString(signatureHex);
       var dataBytes = Encoding.UTF8.GetBytes(data);
       instance.ImportSubjectPublicKeyInfo(Convert.FromHexString(PublicKeyHex), out _);
       return instance.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256);
    }

    public static IWallet ImportWalletFromPrivateKey(string hexString)
    {
        var instance = ECDsa.Create();
        var privateKeyBytes = Convert.FromHexString(hexString);
        instance.ImportECPrivateKey(privateKeyBytes, out _);
        return new Wallet(instance);
    }

    public string ExportPrivateKey()
    {
        return Convert.ToHexStringLower(_keyPair.ExportECPrivateKey());
    }

    public void SaveWallet(string path)
    {
        var privateKeyHex = ExportPrivateKey();
        var walletFile = new WalletFile(privateKeyHex);
        File.WriteAllText(path, JsonSerializer.Serialize(walletFile));
    }

    public static IWallet LoadWallet(string path)
    {
        var text = JsonSerializer.Deserialize<WalletFile>(File.ReadAllText(path));
        var privateKey = text!.PrivateKeyHex;
        return ImportWalletFromPrivateKey(privateKey);
    }
}