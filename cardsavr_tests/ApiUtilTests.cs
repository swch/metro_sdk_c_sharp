using System;
//using System.Dynamic;
using System.Collections.Generic;
using System.Collections.Specialized;

using Newtonsoft.Json;
using Xunit;

using Switch.CardSavr.Http;


namespace cardsavr_tests
{
    public class ApiUtilTests
    {
        public ApiUtilTests()
        {
        }

        [Fact]
        public void AppendQueryString()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("one", "1");
            string s = ApiUtil.AppendQueryString("/path", nvc);
            Assert.Equal("/path?one=1", s);

            nvc.Add("one", "2");
            s = ApiUtil.AppendQueryString("/path", nvc);
            Assert.Equal("/path?one=1,2", s);

            nvc.Add("twenty", "20");
            nvc.Add("thirty", "30");
            s = ApiUtil.AppendQueryString("/path", nvc);
            Assert.Equal("/path?one=1,2&twenty=20&thirty=30", s);
        }

        [Fact]
        public void AppendQueryStringNullKey()
        {
            // null key.
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("one", "1");
            nvc.Add(null, "a non-null value");
            Assert.Throws<ArgumentException>(() => ApiUtil.AppendQueryString("path", nvc));

            // empty key.
            nvc.Clear();
            nvc.Add("one", "1");
            nvc.Add("", "a non-empty value");
            Assert.Throws<ArgumentException>(() => ApiUtil.AppendQueryString("path", nvc));
        }

        [Fact]
        public void AppendQueryStringNullValue()
        {
            // null value.
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("one", "1");
            nvc.Add("two", null);
            Assert.Throws<ArgumentException>(() => ApiUtil.AppendQueryString("path", nvc));

            // empty value.
            nvc.Add("one", "1");
            nvc.Add("two", "");
            Assert.Throws<ArgumentException>(() => ApiUtil.AppendQueryString("path", nvc));
        }

        [Fact]
        public void CreateObject()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("one", "1");
            nvc.Add("two", "2");
            nvc.Add("v1", "1.0");

            IDictionary<string, object> obj = ApiUtil.CreateJsonSerializableObject(nvc);
            Assert.Equal("1", obj["one"]);
            Assert.Equal("2", obj["two"]);
            Assert.Equal("1.0", obj["v1"]);

            string json = JsonConvert.SerializeObject(obj);
            obj = JsonConvert.DeserializeObject<IDictionary<string, object>>(json);
            Assert.Equal("1", obj["one"]);
            Assert.Equal("2", obj["two"]);
            Assert.Equal("1.0", obj["v1"]);
            Assert.Equal(3, obj.Count);
        }
    }
}
