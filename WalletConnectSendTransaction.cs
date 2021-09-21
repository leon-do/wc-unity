using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalletConnectSendTransaction
{
	public class @SendTransactionParams
	{
		public string from;
		public string to;
		public string value;
		public string data;
		public string gas;
	}
	public class SendTransaction
	{
		public int id;
		public string jsonrpc;
		public string method;
		public SendTransactionParams[] @params;
	}

	public static string Json()
	{
		SendTransactionParams sendTransactionParams = new SendTransactionParams()
		{
			from = PlayerPrefs.GetString("Account"),
			to = "0xdD4c825203f97984e7867F11eeCc813A036089D1",
			value = "0x1",
			data = "0x",
			gas = "0x5208",
		};

		SendTransaction sendTransaction = new SendTransaction()
		{
			id = Random.Range(0, 2147483647),
			jsonrpc = "2.0",
			method = "eth_sendTransaction",
			@params = new SendTransactionParams[] { sendTransactionParams }
		};

        // convert to json
        return Newtonsoft.Json.JsonConvert.SerializeObject(sendTransaction);
	}

}
