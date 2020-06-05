using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using Newtonsoft.Json;  
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Switch.CardSavr.Http
{
    internal static class ApiUtil
    {
        public static string AppendQueryString(string path, QueryDef qd)
        {
            if (qd == null || qd.IsEmpty)
                return path;

            if (qd.IsID)
                return $"{path}?ids={qd.ID}";

            return AppendQueryString(path, qd.Collection);
        }

        public static string AppendQueryString(string path, NameValueCollection nvc)
        {
            if (nvc == null || nvc.Count == 0)
                return path;

            int index = 0;
            string[] parts = new string[nvc.Count];
            foreach (string key in nvc.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException($"Query-string keys is null or empty ({path}).");

                string[] values = nvc.GetValues(key);
                if (values == null || values.Length == 0)
                    throw new ArgumentException($"Value for query parameter \"{key}\" is null or empty.");

                for (int n = 0; n < values.Length; ++n)
                {
                    if (String.IsNullOrEmpty(values[n]))
                        throw new ArgumentException($"A value for query parameter \"{key}\" is null or empty.");
                }

                parts[index++] = String.Format("{0}={1}", key, String.Join(',', values));
            }

            return String.Format("{0}?{1}", path, String.Join('&', parts));
        }

        public static string BuildValidTraceHeader(string currentTrace, string userName) {
            string newTrace = null;
            if (currentTrace == null) {
                newTrace = $"{{\"key\": \"{userName}\"}}";
            } else {
                dynamic traceObject = JsonConvert.DeserializeObject(currentTrace);
                if (traceObject.key == null) {
                    traceObject.Add("key", userName);
                    newTrace = JsonConvert.SerializeObject(traceObject);
                } else {
                    newTrace = currentTrace;
                }
            }
            return newTrace;
        }
        public static PropertyBag BuildPropertyBagFromObject(object obj) {
            string s = JsonConvert.SerializeObject(obj, 
                Newtonsoft.Json.Formatting.None, 
                new JsonSerializerSettings { 
                    NullValueHandling = NullValueHandling.Ignore
                });
            return JsonConvert.DeserializeObject<PropertyBag>(s);
        }

        public static IDictionary<string, object> CreateJsonSerializableObject(NameValueCollection nvc)
        {
            if (nvc == null || nvc.Count == 0)
                return null;

            IDictionary<string,object> obj = new ExpandoObject() as IDictionary<string, object>;
            string[] parts = new string[nvc.Count];
            foreach (string key in nvc.AllKeys)
            {
                if (key == null)
                    throw new ArgumentException("Parameter names cannot be null.");

                string[] values = nvc.GetValues(key);
                if (values == null)
                    throw new ArgumentException("Parameter values cannot be null.");

                if (values.Length == 1)
                    obj.Add(key, values[0]);
                else
                    obj.Add(key, values);
            }

            return obj;
        }

        public static string GetSingleHeaderValue(HttpHeaders headers, string name)
        {
            IEnumerable<string> values;
            if (headers.TryGetValues(name, out values))
            {
                foreach (string s in values)
                {
                    // immediately return the first value found.
                    return s;
                }
            }

            // nothing found.
            return null;
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        //These need to stay consistent across all SDKs
        public static string GenerateRandomPAR(string pan, string expMonth, string expYear, string username)
        {
            string salt = $"{username}{expMonth}{expYear}";
            HashAlgorithm hashS = new SHA256CryptoServiceProvider();
            byte[] resultS = hashS.ComputeHash(Encoding.UTF8.GetBytes(salt));
            Console.WriteLine(Convert.ToBase64String(resultS));
            HashAlgorithm hashP = new MD5CryptoServiceProvider();
            byte[] resultP = hashP.ComputeHash(Encoding.UTF8.GetBytes(pan));
            Console.WriteLine(Convert.ToBase64String(resultP));

            string hashed = "C" + Convert.ToBase64String(KeyDerivation.Pbkdf2(
                Encoding.UTF8.GetString(resultP),
                resultS,
                KeyDerivationPrf.HMACSHA1,
                5000,
                20));
            return hashed;
        }
    }
}
