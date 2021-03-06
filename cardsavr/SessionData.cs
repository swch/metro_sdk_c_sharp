﻿using System;
using System.Threading;

using Switch.Security.Cryptography;


namespace Switch.CardSavr.Http
{
    public sealed class SessionData
    {
        private string _baseUrl;        // passed-in base URL
        private Uri _baseUri;           // a Uri created from _baseUrl
        private string _staticKey;      // the base64 Switch-provided static key
        private string _appName;        // the Switch-provided app name
        private string _username;       // the Switch-provided user name
        private string _password;       // the user password
        private string _grant;          // a temporary short lived grant
        private string _trace;          // a temporary short lived grant
        private string _cert;           // self signing cert for developement servers
        private Ecdh _ecdh;             // key pair
        private string _sessionToken;         // session-specific token

        public string BaseUrl
        {
            get { return _baseUrl; }
            set { Interlocked.Exchange<string>(ref _baseUrl, value); }
        }

        public Uri BaseUri
        {
            get { return _baseUri; }
            private set { Interlocked.Exchange<Uri>(ref _baseUri, value); }
        }

        public string StaticKey
        {
            get { return _staticKey;  }
            set { Interlocked.Exchange<string>(ref _staticKey, value); }
        }

        public string SessionToken
        {
            get { return _sessionToken;  }
            set { Interlocked.Exchange<string>(ref _sessionToken, value); }
        }

        public string AppName
        {
            get { return _appName; }
            set { Interlocked.Exchange<string>(ref _appName, value); }
        }

        public string UserName
        {
            get { return _username; }
            set { Interlocked.Exchange<string>(ref _username, value); }
        }

        public string Password
        {
            get { return _password; }
            set { Interlocked.Exchange<string>(ref _password, value); }
        }
    
        public string Grant
        {
            get { return _grant; }
            set { Interlocked.Exchange<string>(ref _grant, value); }
        }

        public string Trace
        {
            get { return _trace; }
            set { Interlocked.Exchange<string>(ref _trace, value); }
        }

        public Ecdh Ecdh
        {
            get { return _ecdh; }
        }

        public SessionData(
            string baseUrl, string staticKey, string appName, string username, string password, string grant = null, string traceOverride = null, string cert = null)
        {
            _baseUrl = baseUrl;
            _baseUri = new Uri(baseUrl);
            _staticKey = staticKey;
            _appName = appName;
            _username = username;
            _password = password;
            _grant = grant;
            _trace = traceOverride;
            _cert = cert;
            _ecdh = Ecdh.Create();
            _sessionToken = "null";
        }
    }
}
