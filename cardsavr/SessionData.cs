using System;
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
        private string _userName;       // the Switch-provided user name
        private string _password;       // the user password
        private int _encrypt;           // non-zero if encryption is enabled
        private Ecdh _ecdh;             // key pair
        private string _cookie;         // session-specific cookie

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

        public string AppName
        {
            get { return _appName; }
            set { Interlocked.Exchange<string>(ref _appName, value); }
        }

        public string UserName
        {
            get { return _userName; }
            set { Interlocked.Exchange<string>(ref _userName, value); }
        }

        public string Password
        {
            get { return _password; }
            set { Interlocked.Exchange<string>(ref _password, value); }
        }

        public bool Encrypt
        {
            get { return _encrypt != 0; }
            set { Interlocked.Exchange(ref _encrypt, value ? 1 : 0); }
        }

        public Ecdh Ecdh
        {
            get { return _ecdh; }
        }

        public string Cookie
        {
            get { return _cookie; }
            set { Interlocked.Exchange<string>(ref _cookie, value); }
        }

        public SessionData(
            string baseUrl, string staticKey, string appName, string userName, string password)
        {
            _baseUrl = baseUrl;
            _baseUri = new Uri(baseUrl);
            _staticKey = staticKey;
            _appName = appName;
            _userName = userName;
            _password = password;
            _ecdh = Ecdh.Create();
            _cookie = null;

            // encryption turned on by default. will be switched off if told to do so 
            // by the server.
            _encrypt = 1;
        }
    }
}
