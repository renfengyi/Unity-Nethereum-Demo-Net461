using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Nethereum.ABI.FunctionEncoding.Attributes;
using UnityEngine;
using Nethereum.Contracts;
using Nethereum.Web3;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.JsonRpc.Client;
/// <summary>
/// 多个参数的事务
/// </summary>
public class DecodeData : MonoBehaviour {

   private Web3 _web3;

    async void Start ()
	{
        //code snippet for ssl connections
	    ServicePointManager.ServerCertificateValidationCallback += delegate (object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
	        if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
	        {
	            foreach (X509ChainStatus status in chain.ChainStatus)
	            {
	                if (status.Status != X509ChainStatusFlags.PartialChain)
	                {
	                    return false;
	                }
	            }
	            return true;
	        }
	        return false;
	    };

        //_web3 = new Web3("http://localhost:8545"); //defaults to https://mainnet.infura.io

        //Debug.Log(_web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().Result.Value.ToString());

        await ShouldBeAbleToEncodeDecodeComplexInputOutput();
    }

   
    // Update is called once per frame
    void Update ()
    {
        
    }

    public async Task ShouldBeAbleToEncodeDecodeComplexInputOutput()
    {
        var senderAddress = "0x47E95DCdb798Bc315198138bC930758E6f399f81";
        var password = "12345678";

        var abi = @"[{'constant':false,'inputs':[{'name':'key','type':'bytes32'},{'name':'name','type':'string'},{'name':'description','type':'string'}],'name':'StoreDocument','outputs':[{'name':'success','type':'bool'}],'type':'function'},{'constant':true,'inputs':[{'name':'','type':'bytes32'},{'name':'','type':'uint256'}],'name':'documents','outputs':[{'name':'name','type':'string'},{'name':'description','type':'string'},{'name':'sender','type':'address'}],'type':'function'}]";

        var byteCode = "0x608060405234801561001057600080fd5b50610555806100206000396000f30060806040526004361061004b5763ffffffff7c01000000000000000000000000000000000000000000000000000000006000350416634a75c0ff811461005057806379c17cc514610100575b600080fd5b34801561005c57600080fd5b5060408051602060046024803582810135601f81018590048502860185019096528585526100ec95833595369560449491939091019190819084018382808284375050604080516020601f89358b018035918201839004830284018301909452808352979a99988101979196509182019450925082915084018382808284375094975061022c9650505050505050565b604080519115158252519081900360200190f35b34801561010c57600080fd5b5061011b6004356024356102f7565b6040518080602001806020018473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001838103835286818151815260200191508051906020019080838360005b8381101561018e578181015183820152602001610176565b50505050905090810190601f1680156101bb5780820380516001836020036101000a031916815260200191505b50838103825285518152855160209182019187019080838360005b838110156101ee5781810151838201526020016101d6565b50505050905090810190601f16801561021b5780820380516001836020036101000a031916815260200191505b509550505050505060405180910390f35b600061023661046f565b5060408051606081018252848152602080820185905233828401526000878152808252928320805460018101808355918552938290208351805194959294869460039094029092019261028e9284929091019061048e565b5060208281015180516102a7926001850192019061048e565b50604091909101516002909101805473ffffffffffffffffffffffffffffffffffffffff191673ffffffffffffffffffffffffffffffffffffffff90921691909117905550600195945050505050565b60006020528160005260406000208181548110151561031257fe5b60009182526020918290206003919091020180546040805160026001841615610100026000190190931692909204601f8101859004850283018501909152808252919450925083918301828280156103ab5780601f10610380576101008083540402835291602001916103ab565b820191906000526020600020905b81548152906001019060200180831161038e57829003601f168201915b505050505090806001018054600181600116156101000203166002900480601f0160208091040260200160405190810160405280929190818152602001828054600181600116156101000203166002900480156104495780601f1061041e57610100808354040283529160200191610449565b820191906000526020600020905b81548152906001019060200180831161042c57829003601f168201915b5050506002909301549192505073ffffffffffffffffffffffffffffffffffffffff1683565b6040805160608181018352808252602082015260009181019190915290565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f106104cf57805160ff19168380011785556104fc565b828001600101855582156104fc579182015b828111156104fc5782518255916020019190600101906104e1565b5061050892915061050c565b5090565b61052691905b808211156105085760008155600101610512565b905600a165627a7a723058206c6d91d0ba0e6f7b713c1c7fdc9b771df89abd8dfa5163a90e1d8c8d2c8cdae70029";

        var url = "http://localhost:8545";

        //a managed account uses personal_sendTransanction with the given password, this way we don't need to unlock the account for a certain period of time
        var account = new ManagedAccount(senderAddress, password);

        //using the specific geth web3 library to allow us manage the mining.
        //var web3 = new Geth.Web3Geth(account);
        var web3 = new Web3(url);


        var receipt = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(byteCode, senderAddress, new HexBigInteger(900000));

        var contractAddress = receipt.ContractAddress;

        var contract = web3.Eth.GetContract(abi, contractAddress);
        //var contractAddress = "0xb7ee186eac91aeeb37e395db99132fe7667ac227";

        //var contract = new Contract(null,abi, contractAddress);

        var storeFunction = contract.GetFunction("StoreDocument");
        var documentsFunction = contract.GetFunction("documents");


        var transactionHash = await storeFunction.SendTransactionAsync(senderAddress, new HexBigInteger(900000), null, "key1", "hello", "solidity is great");
        receipt = await storeFunction.SendTransactionAndWaitForReceiptAsync(@from: senderAddress, gas: new HexBigInteger(900000),
            gasPrice: null, receiptRequestCancellationToken: null, value: null, functionInput: new object[] { "key1", "hello again", "ethereum is great" }).ConfigureAwait(false);

        var result = await documentsFunction.CallDeserializingToObjectAsync<Document>("key1", 0);
        var result2 = await documentsFunction.CallDeserializingToObjectAsync<Document>("key1", 1);

        Debug.Log("hello" + result.Name);
        Debug.Log("solidity is great" + result.Description);

        Debug.Log("hello again" + result2.Name);
        Debug.Log("ethereum is great" + result2.Description);

    }

    [FunctionOutput]
    public class Document
    {
        [Parameter("string", "name", 1)]
        public string Name { get; set; }

        [Parameter("string", "description", 2)]
        public string Description { get; set; }

        [Parameter("address", "sender", 3)]
        public string Sender { get; set; }
    }
}
