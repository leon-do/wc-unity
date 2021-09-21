using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace WC.Request
{
	public class WalletConnectRequest
	{
		// https://docs.walletconnect.org/tech-spec#session-request
		private class ClientMeta
		{
			public string description;
			public string url;
			public string[] icons;
			public string name;
		}

		private class WCSessionRequestRequestParams
		{
			public string peerId;
			public int? chainId;
			public ClientMeta peerMeta;
		}

		private class WCSessionRequestRequest
		{
			public int id;
			public string jsonrpc;
			public string method;
			public WCSessionRequestRequestParams[] @params;
		}

		private class WCSessionRequestLogoutParams
		{
			public bool approved;
			public string chainId;
			public string networkId;
			public string accounts;
		}

		private class WCSessionRequestLogout
		{
			public int id;
			public string jsonrpc;
			public string method;
			public WCSessionRequestLogoutParams[] @params;
		}

		// https://docs.walletconnect.org/tech-spec#websocket-messages
		private class SocketMsg
		{
			public string payload; // "" or encrypted payload {data, hmac, iv}
			public string topic; // uid
			public string type; // "pub" or "sub"
		}

		// convert session json string to connect
		public static string Connect(string _peerId)
		{
			ClientMeta clientMeta = new ClientMeta()
			{
				description = "Wallet",
				url = "https://chainsafe.io",
				icons = new string[] { "https://ipfs.io/ipfs/QmVcB5rxQ6Lgfw8G92LJ2mZtegzQWR13mxFN9ghq6gfUfH" },
				name = "Wallet"
			};

			WCSessionRequestRequestParams wcParams = new WCSessionRequestRequestParams()
			{
				peerId = _peerId,
				chainId = null,
				peerMeta = clientMeta,
			};

			WCSessionRequestRequest request = new WCSessionRequestRequest()
			{
				id = Random.Range(0, 2147483647),
				jsonrpc = "2.0",
				method = "wc_sessionRequest",
				@params = new WCSessionRequestRequestParams[] { wcParams }
			};

			// convert to json
			return Newtonsoft.Json.JsonConvert.SerializeObject(request);
		}

		// convert session json string to disconnect
		public static string Disconnect()
		{
			WCSessionRequestLogoutParams logoutParams = new WCSessionRequestLogoutParams()
			{
				approved = false,
				chainId = null,
				networkId = null,
				accounts = null
			};

			WCSessionRequestLogout request = new WCSessionRequestLogout()
			{
				id = Random.Range(0, 2147483647),
				jsonrpc = "2.0",
				method = "wc_sessionUpdate",
				@params = new WCSessionRequestLogoutParams[] { logoutParams }
			};

			// convert to json
			return Newtonsoft.Json.JsonConvert.SerializeObject(request);
		}

		public static string Json(string _topic, string _type, string _payload)
		{
			SocketMsg sub = new SocketMsg()
			{
				payload = _payload,
				topic = _topic,
				type = _type,
			};

			// convert to json
			return Newtonsoft.Json.JsonConvert.SerializeObject(sub);
		}
	}
}
