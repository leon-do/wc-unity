using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Net.WebSockets;
using UnityEngine;
using WC.Url;
using WC.Cryptography;
using WC.Request;
using WC.Response;

public class WalletConnect
{
	private ClientWebSocket ws;
	private string key;
	private string clientId;

	async public void Open()
	{
		// open walletconnect socket connection
		ws = new ClientWebSocket();
		await ws.ConnectAsync(new Uri("wss://bridge.walletconnect.org"), CancellationToken.None);

		// Listen for messages
		Listen();

		// generate wallet connect url
		new WalletConnectUrl();
		string url = WalletConnectUrl.Url();
		key = WalletConnectUrl.Key();
		clientId = WalletConnectUrl.ClientId();
		Debug.Log("url: " + url);

		// generate sub socket message
		string subMsg = WalletConnectRequest.Json(clientId, "sub", "");
		Debug.Log("subMsg: " + subMsg);

		// send sub socket message
		await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(subMsg)), WebSocketMessageType.Text, true, CancellationToken.None);

		// generate session request: https://docs.walletconnect.org/tech-spec#session-request
		string session = WalletConnectRequest.Connect(clientId);
		Debug.Log("session: " + session);

		// encrypt session request
		string encryptedSession = await WalletConnectCryptography.AESEncode(key, session);
		Debug.Log("encryptedSession: " + encryptedSession);

		// generate pub socket message with encrypted session
		string pubMsg = WalletConnectRequest.Json(clientId, "pub", encryptedSession);
		Debug.Log("pubMsg: " + pubMsg);

		// send pub socket message
		await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(pubMsg)), WebSocketMessageType.Text, true, CancellationToken.None);
	}

	async private void Listen()
	{
		Debug.Log("WalletConnect Socket Listening...");
		// receive response
		ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);
		while (ws.State == WebSocketState.Open)
		{
			WebSocketReceiveResult result = await ws.ReceiveAsync(buffer, CancellationToken.None);
			string message = System.Text.Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
			ParseMessage(message);
		};
	}

	async private void ParseMessage(string _message)
	{
		Debug.Log("WalletConnect Socket Message: " + _message);

		// if message is empty from Disconnect()
		if (_message == "") return;

		// parse message https://docs.walletconnect.org/tech-spec#cryptography
		WalletConnectResponse.EncryptionPayload parsedMsg = WalletConnectResponse.ParseEncryptionPayload(_message);
		Debug.Log("parsedMsg.data: " + parsedMsg.data);
		Debug.Log("parsedMsg.iv: " + parsedMsg.iv);

		// decrypt message
		string decryptedMsg = await WalletConnectCryptography.AESDecode(key, parsedMsg.data, parsedMsg.iv);
		Debug.Log("decryptedMsg: " + decryptedMsg);

		// if user canceled transaction
		if (decryptedMsg.Contains("-32000")) return;

		// if message is about user account
		if (decryptedMsg.Contains("accounts"))
		{
			// parse out account
			string account = WalletConnectResponse.ParseWCSessionRequestResponseResult(decryptedMsg).accounts[0];
			Debug.Log("account: " + account);
		}

		// if message is about transaction receipt
		if (decryptedMsg.Contains("\"result\":\"0x"))
		{
			// parse out transaction
			string receipt = WalletConnectResponse.ParseSendTransactionResponse(decryptedMsg);
			Debug.Log("receipt: " + receipt);
		}
	}


	async public void Disconnect()
	{
		// generate logout json request
		string logoutJson = WalletConnectRequest.Disconnect();
		Debug.Log("logoutJson: " + logoutJson);

		// encrypt logout json
		string encryptedLogout = await WalletConnectCryptography.AESEncode(key, logoutJson);
		Debug.Log("encryptedLogout: " + encryptedLogout);

		// create socket message to close session 
		string encryptedLogoutMsg = WalletConnectRequest.Json(clientId, "pub", encryptedLogout);
		Debug.Log("encryptedLogoutMsg: " + encryptedLogoutMsg);

		// send encrypted message to close on wallet side
		await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(encryptedLogoutMsg)), WebSocketMessageType.Text, true, CancellationToken.None);

		// close socket on game side
		await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

		Debug.Log("Disconnected from wallet");
	}

	async public void Send()
	{
		// generate send transaction request json
		string sendTx = WalletConnectSendTransaction.Json();
		Debug.Log("sendTx: " + sendTx);

		// encrypt send transaction message
		string encryptedSendTx = await WalletConnectCryptography.AESEncode(key, sendTx);
		Debug.Log("encryptedSendTx: " + encryptedSendTx);

		// create socket message to send transaction
		string encryptedSendTxMsg = WalletConnectRequest.Json(clientId, "pub", encryptedSendTx);
		Debug.Log("encryptedSendTxMsg: " + encryptedSendTxMsg);

		// send encrypted message
		await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(encryptedSendTxMsg)), WebSocketMessageType.Text, true, CancellationToken.None);
	}

}
