using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;  
using System.Linq;
using System.Security.Cryptography;


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

        public static string GenerateRandomPar(string pan, string exp_month, string exp_year, string salt) {

            string digits = "0123456789";
            string letters = "abcdefghijklmnopqrstuvwxyz";
            string both = letters + digits;

            string par = "";
            par += letters[random.Next() % letters.Length];
            par += digits[random.Next() % digits.Length];
            par += letters[random.Next() % letters.Length];
            par += digits[random.Next() % digits.Length];

            int remainder = 29 - par.Length;
            for (; remainder > 0; --remainder)
                par += both[random.Next() % both.Length];

            return par.ToUpper();
        }
/*
            string[] paramsArray = [pan, exp_month, exp_year, salt];
            // Hash up the salt for use as salt
            string hashS = crypto.createHash("sha256");
            hashS.update(salt + exp_month + exp_year);
            const salt_buffer = hashS.digest();

            // Hashup the PAN into 128bits
            const hashP = crypto.createHash("md5");
            hashP.update(pan);
            const panHash = hashP.digest();
            const PARHash = crypto.pbkdf2Sync(panHash, salt_buffer, 5000, 16, "md5");
            const PAR = "CSAVR" + PARHash.toString('base64');
            return PAR;
        }
*/
    }
}
