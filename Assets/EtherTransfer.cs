using Nethereum.Hex.HexTypes;
using Nethereum.KeyStore;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// 创建账户-转账交易-从本地配置读取账号
/// </summary>
public class EtherTransfer : MonoBehaviour {

	// Use this for initialization
	async void Start () {
        //Debug.Log(Application.streamingAssetsPath);
        //Debug.Log(CreateAccount("12345678",Application.streamingAssetsPath));

        //await ShouldBeAbleToTransferBetweenAccountsUsingManagedAccount();//托管帐户是由客户端维护的帐户(Geth /Parity)
        await ShouldBeAbleToTransferBetweenAccountsUsingThePrivateKey();//默认账户
        //await ShouldBeAbleToTransferBetweenAccountsLoadingEncryptedPrivateKey();//从keystore文件导入账号

    }

    public void ShouldCreateKeyPair()
    {
        var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
        //Get the public address (derivied from the public key)
        var address = ecKey.GetPublicAddress();
        var privateKey = ecKey.GetPrivateKey();
    }

    public string CreateAccount(string password, string path)
    {
        //Generate a private key pair using SecureRandom
        var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
        //Get the public address (derivied from the public key)
        var address = ecKey.GetPublicAddress();
        Debug.Log(address);
        //Create a store service, to encrypt and save the file using the web3 standard
        var service = new KeyStoreService();
        var encryptedKey = service.EncryptAndGenerateDefaultKeyStoreAsJson(password, ecKey.GetPrivateKeyAsBytes(), address);
        var fileName = service.GenerateUTCFileName(address);
        //save the File
        using (var newfile = File.CreateText(Path.Combine(path, fileName)))
        {
            newfile.Write(encryptedKey);
            newfile.Flush();
        }

        return fileName;
    }

    /// <summary>
    /// 提示无效的账户--（Geth / Parity）
    /// </summary>
    /// <returns></returns>
    public async Task ShouldBeAbleToTransferBetweenAccountsUsingManagedAccount()
    {
        var senderAddress = "0x09f8F8c219D94A5db8Ee466dC072748603A7A0D9";
        var addressTo = "0xCbbA01495C541FDcCE6F05b52f420D7c9eA018B4";
        var password = "12345678";

        // A managed account is an account which is maintained by the client (Geth / Parity)
        var account = new ManagedAccount(senderAddress, password);

        var web3 = new Web3(account);
        Debug.Log(account.Address);
        Debug.Log(addressTo);
        //The transaction receipt polling service is a simple utility service to poll for receipts until mined
        var transactionPolling = web3.TransactionManager.TransactionReceiptService;

        var currentBalance = await web3.Eth.GetBalance.SendRequestAsync(addressTo);
        //assumed client is mining already

        //When sending the transaction using the transaction manager for a managed account, personal_sendTransaction is used.
        var transactionReceipt = await transactionPolling.SendRequestAndWaitForReceiptAsync(() =>
            web3.TransactionManager.SendTransactionAsync(account.Address, addressTo, new HexBigInteger(1000000000000000000))//1ETH
        );

        var newBalance = await web3.Eth.GetBalance.SendRequestAsync(addressTo);

        Debug.Log((currentBalance.Value + 1000000000000000000) +" "+  newBalance.Value);
    }
    /// <summary>
    /// 执行没有问题
    /// </summary>
    /// <returns></returns>
    public async Task ShouldBeAbleToTransferBetweenAccountsUsingThePrivateKey()
        {
            var senderAddress = "0x47E95DCdb798Bc315198138bC930758E6f399f81";
            var addressTo = "0xCbbA01495C541FDcCE6F05b52f420D7c9eA018B4";
            var password = "12345678";

            var privateKey = "0xa5ca770c997e53e182c5015bcf1b58ba5cefe358bf217800d8ec7d64ca919edd";


            // The default account is an account which is mananaged by the user

            var account = new Account(privateKey);
            var web3 = new Web3(account);

            //The transaction receipt polling service is a simple utility service to poll for receipts until mined
            var transactionPolling = web3.TransactionManager.TransactionReceiptService;

            var currentBalance = await web3.Eth.GetBalance.SendRequestAsync(addressTo);
            //assumed client is mining already

            //when sending a transaction using an Account, a raw transaction is signed and send using the private key
            var transactionReceipt = await transactionPolling.SendRequestAndWaitForReceiptAsync(() =>
                web3.TransactionManager.SendTransactionAsync(account.Address, addressTo, new HexBigInteger(10000000000000000000))//10ETH
            );

            var newBalance = await web3.Eth.GetBalance.SendRequestAsync(addressTo);

            Debug.Log((currentBalance.Value + 10000000000000000000) +"  "+ newBalance.Value);
        }
    /// <summary>
    /// 执行没有问题
    /// </summary>
    /// <returns></returns>
    public async Task ShouldBeAbleToTransferBetweenAccountsLoadingEncryptedPrivateKey()
    {
        var senderAddress = "0x47E95DCdb798Bc315198138bC930758E6f399f81";
        var addressTo = "0x09f8F8c219D94A5db8Ee466dC072748603A7A0D9";
        var password = "12345678";

        var keyStoreEncryptedJson =
            @"{""crypto"":{""cipher"":""aes-128-ctr"",""ciphertext"":""b4f42e48903879b16239cd5508bc5278e5d3e02307deccbec25b3f5638b85f91"",""cipherparams"":{""iv"":""dc3f37d304047997aa4ef85f044feb45""},""kdf"":""scrypt"",""mac"":""ada930e08702b89c852759bac80533bd71fc4c1ef502291e802232b74bd0081a"",""kdfparams"":{""n"":65536,""r"":1,""p"":8,""dklen"":32,""salt"":""2c39648840b3a59903352b20386f8c41d5146ab88627eaed7c0f2cc8d5d95bd4""}},""id"":""19883438-6d67-4ab8-84b9-76a846ce544b"",""address"":""12890d2cce102216644c59dae5baed380d84830c"",""version"":3}";
        //this is your wallet key  file which can be found on
        //Linux: ~/.ethereum/keystore
        //Mac: /Library/Ethereum/keystore
        //Windows: %APPDATA%/Ethereum

        //if not using portable or netstandard (^net45) you can use LoadFromKeyStoreFile to load the file from the file system.
        var account = Account.LoadFromKeyStoreFile(Application.streamingAssetsPath+ "/UTC--2018-08-21T08-44-51.9844187Z--CbbA01495C541FDcCE6F05b52f420D7c9eA018B4", password);
        //var account = Account.LoadFromKeyStore(keyStoreEncryptedJson, password);
        var web3 = new Web3(account);

        //The transaction receipt polling service is a simple utility service to poll for receipts until mined
        var transactionPolling = web3.TransactionManager.TransactionReceiptService;

        var currentBalance = await web3.Eth.GetBalance.SendRequestAsync(addressTo);

        //assumed client is mining already
        //when sending a transaction using an Account, a raw transaction is signed and send using the private key
        var transactionReceipt = await transactionPolling.SendRequestAndWaitForReceiptAsync(() =>
            web3.TransactionManager.SendTransactionAsync(account.Address, addressTo, new HexBigInteger(1000000000000000000))//1ETH
        );

        var newBalance = await web3.Eth.GetBalance.SendRequestAsync(addressTo);

        Debug.Log((currentBalance.Value + 1000000000000000000) + " " + newBalance.Value);
    }
}
