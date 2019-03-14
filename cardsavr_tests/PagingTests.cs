using System;

using Xunit;

using Switch.CardSavr.Http;


namespace cardsavr_tests
{
    public class PagingTests
    {
        [Fact]
        public void Basic()
        {
            Paging p = new Paging(5);
            Assert.Equal(1, p.GetCount());
            p = Paging.FromHeader(p.Stringify());
            Assert.Equal(5, p.Page);
            Assert.Equal(0, p.PageLength);
            Assert.Null(p.Sort);
            Assert.Null(p.IsDescending);
            Assert.Equal(-1, p.TotalResults);

            p = new Paging(5, 20);
            Assert.Equal(2, p.GetCount());
            p = Paging.FromHeader(p.Stringify());
            Assert.Equal(5, p.Page);
            Assert.Equal(20, p.PageLength);
            Assert.Null(p.Sort);
            Assert.Null(p.IsDescending);
            Assert.Equal(-1, p.TotalResults);

            p = new Paging(5, 20, "id");
            Assert.Equal(3, p.GetCount());
            p = Paging.FromHeader(p.Stringify());
            Assert.Equal(5, p.Page);
            Assert.Equal(20, p.PageLength);
            Assert.Equal("id", p.Sort);
            Assert.Null(p.IsDescending);
            Assert.Equal(-1, p.TotalResults);

            p = new Paging(5, 20, "id", false);
            Assert.Equal(4, p.GetCount());
            p = Paging.FromHeader(p.Stringify());
            Assert.Equal(5, p.Page);
            Assert.Equal(20, p.PageLength);
            Assert.Equal("id", p.Sort);
            Assert.Equal(false, p.IsDescending);
            Assert.Equal(-1, p.TotalResults);
        }

        [Fact]
        public void FromHeaderWithResults()
        {
            string s = "{\"page\":3,\"page_length\":15,\"sort\":\"id\",\"is_descending\":false,\"total_results\":11}";
            Paging p = Paging.FromHeader(s);
            Assert.Equal(3, p.Page);
            Assert.Equal(15, p.PageLength);
            Assert.Equal("id", p.Sort);
            Assert.Equal(false, p.IsDescending);
            Assert.Equal(11, p.TotalResults);
        }
    }
}
