using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace WC.Cryptography
{
	public class AESEncodeMessage
	{
		public string data;
		public string hmac;
		public string iv;
	}

	public class WalletConnectCryptography
	{
		static async public Task<string> AESEncode(string _key, string _message)
		{
			byte[] key = HexToByteArray(_key);
			byte[] msg = Encoding.UTF8.GetBytes(_message);

			byte[] encryptedContent, iv, hmac;

			using (MemoryStream ms = new MemoryStream())
			{
				using (AesManaged aes = new AesManaged())
				{
					aes.Mode = CipherMode.CBC;
					aes.Padding = PaddingMode.PKCS7;
					aes.KeySize = 256;

					iv = aes.IV;

					using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
					{
						await cs.WriteAsync(msg, 0, msg.Length);
					}

					encryptedContent = ms.ToArray();
				}
			}

			using (HMACSHA256 hmac256 = new HMACSHA256(key))
			{
				hmac256.Initialize();

				byte[] toSign = new byte[iv.Length + encryptedContent.Length];

				//copy our 2 array into one
				Buffer.BlockCopy(encryptedContent, 0, toSign, 0, encryptedContent.Length);
				Buffer.BlockCopy(iv, 0, toSign, encryptedContent.Length, iv.Length);

				hmac = hmac256.ComputeHash(toSign);
			}

			AESEncodeMessage aesEncodeMessage = new AESEncodeMessage()
			{
				data = BitConverter.ToString(encryptedContent).Replace("-", ""),
				hmac = BitConverter.ToString(hmac).Replace("-", ""),
				iv = BitConverter.ToString(iv).Replace("-", "")
			};

			return Newtonsoft.Json.JsonConvert.SerializeObject(aesEncodeMessage);
		}

		static async public Task<string> AESDecode(string _key, string _encryptedContent, string _iv)
		{
			byte[] encrypted = HexToByteArray(_encryptedContent);

			using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
			{
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;
				aes.KeySize = 256;

				aes.Key = HexToByteArray(_key);
				aes.IV = HexToByteArray(_iv);

				ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

				using (MemoryStream ms = new MemoryStream(encrypted))
				{
					using (MemoryStream sink = new MemoryStream())
					{
						using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
						{
							await cs.CopyToAsync(sink);
							string decrypted = Encoding.UTF8.GetString(sink.ToArray());
							return decrypted;
						}
					}
				}
			}
		}

		static private byte[] HexToByteArray(string hex)
		{
			// https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
			return Enumerable.Range(0, hex.Length)
			.Where(x => x % 2 == 0)
			.Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
			.ToArray();
		}
	}
}