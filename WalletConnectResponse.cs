using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WC.Cryptography;
using Newtonsoft.Json;

namespace WC.Response
{
	public class WalletConnectResponse
	{
		// https://docs.walletconnect.org/tech-spec#websocket-messages
		private class SocketMessage
		{
			public string payload { get; set; }
		}

		public class WCSessionRequestResponseResult
		{
			public int chainId { get; set; }
			public string[] accounts { get; set; }
		}

		// https://docs.walletconnect.org/tech-spec#session-request
		private class WCSessionRequestResponse
		{
			public WCSessionRequestResponseResult result { get; set; }
		}

		// https://docs.walletconnect.org/tech-spec#cryptography
		public class EncryptionPayload
		{
			public string data { get; set; }
			public string hmac { get; set; }
			public string iv { get; set; }
		}

		// https://docs.walletconnect.org/json-rpc-api-methods/ethereum#example-3
		private class SendTransactionResponse
		{
			public string result { get; set; }
		}

		public static EncryptionPayload ParseEncryptionPayload(string _json)
		{
			SocketMessage response = JsonConvert.DeserializeObject<SocketMessage>(_json);
			EncryptionPayload payload = JsonConvert.DeserializeObject<EncryptionPayload>(response.payload);
			return payload;
		}

		public static WCSessionRequestResponseResult ParseWCSessionRequestResponseResult(string _json)
		{
			WCSessionRequestResponse response = JsonConvert.DeserializeObject<WCSessionRequestResponse>(_json);
			return response.result;
		}

		public static string ParseSendTransactionResponse(string _json)
		{
			SendTransactionResponse response = JsonConvert.DeserializeObject<SendTransactionResponse>(_json);
			return response.result;
		}
	}
}
