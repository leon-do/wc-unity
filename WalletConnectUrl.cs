using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace WC.Url
{
    public class WalletConnectUrl
    {
        private static string url;
        private static string clientId;
        private static string key;

        public WalletConnectUrl()
        {
            clientId = GenerateGuid();
            key = GenerateKey();
            url = "wc:" + clientId + "@1?bridge=https%3A%2F%2Fbridge.walletconnect.org&key=" + key; 
        }

        public static string Url()
        {
            return url;
        }

        public static string ClientId()
        {
            return clientId;
        }

        public static string Key()
        {
            return key;
        }

        private string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        private string GenerateKey()
        {
            byte[] secret = new byte[32];
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider(); 
            rngCsp.GetBytes(secret);
            string key = BitConverter.ToString(secret).Replace("-", "").ToLower();
            return key;
        }
    }
}
