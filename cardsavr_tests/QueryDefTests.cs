using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using Xunit;

using Switch.CardSavr.Http;


namespace cardsavr_tests
{
    public class QueryDefTests
    {
        public QueryDefTests()
        {
        }

        [Fact]
        public void nullArg()
        {
            QueryDef q = new QueryDef(null);
            Assert.False(q.IsID);
            Assert.True(q.IsEmpty);
        }

        [Fact]
        public void emptyArg()
        {
            QueryDef q = new QueryDef("");
            Assert.False(q.IsID);
            Assert.True(q.IsEmpty);

            NameValueCollection nvc = new NameValueCollection();
            q = new QueryDef(nvc);
            Assert.False(q.IsID);
            Assert.True(q.IsEmpty);

            nvc.Add("id", null);
            q = new QueryDef(nvc);
            Assert.False(q.IsID);
            Assert.True(q.IsEmpty);

            List<string> lst = new List<string>();
            q = new QueryDef(lst);
            Assert.False(q.IsID);
            Assert.True(q.IsEmpty);

            lst.Add(null);
            q = new QueryDef(lst);
            Assert.False(q.IsID);
            Assert.True(q.IsEmpty);
        }

        [Fact]
        public void intArg()
        {
            QueryDef q = new QueryDef(2);
            Assert.True(q.IsID);
            Assert.False(q.IsEmpty);
            Assert.Equal("2", q.ID);
        }

        [Fact]
        public void stringArg()
        {
            QueryDef q = new QueryDef("22");
            Assert.True(q.IsID);
            Assert.False(q.IsEmpty);
            Assert.Equal("22", q.ID);
        }

        [Fact]
        public void stringListArg()
        {
            string[] ids = new string[] { "22", "23", "24" };
            QueryDef q = new QueryDef(string.Join(',', ids));
            Assert.False(q.IsID);
            Assert.False(q.IsEmpty);
            Assert.Equal("22,23,24", q.Collection.Get("ids"));

            q = new QueryDef("50,51,");
            Assert.False(q.IsID);
            Assert.False(q.IsEmpty);
            Assert.Equal("50,51", q.Collection.Get("ids"));
        }

        [Fact]
        public void NameValueCollectionArg()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("ids", "100");
            nvc.Add("ids", "200");
            nvc.Add("ids", "300");

            QueryDef q = new QueryDef(nvc);
            Assert.False(q.IsID);
            Assert.False(q.IsEmpty);
            Assert.Equal("100,200,300", q.Collection["ids"]);

            nvc.Add("brand", "switch");
            Assert.False(q.IsID);
            Assert.False(q.IsEmpty);
            Assert.Equal("100,200,300", q.Collection["ids"]);
            Assert.Equal("switch", q.Collection["brand"]);
        }
    }
}
