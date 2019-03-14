using System;

using Newtonsoft.Json;


namespace Switch.CardSavr.Http
{
    /// <summary>
    /// Represents paging parameters that can be used with many client HTTP endpoints.
    /// A valid page or pageLength is >= 1. Any other value will be ignored.
    /// 
    /// The "Page" and "PageLength" parameters are ignored if the value is less than 1.
    /// The "Sort" parameter is ignore if its value is null.
    /// The "IsDescending" parameter is ignored unless it is set explicitly.
    /// 
    /// The "TotalResults" parameter is never included in the Stringify() results. Its value
    /// is always -1 unless the object was created from a response header using FromHeader().
    /// 
    /// Using the default constructor without changing any properties will cause Stringify()
    /// to return null (and no paging header being passed to the endpoint). The caller must 
    /// use one of the non-default constructors, and/or set properties directly.
    /// </summary>
    public sealed class Paging
    {
        [JsonProperty(PropertyName = "page")]
        public int Page { get; set; }

        [JsonProperty(PropertyName = "page_length")]
        public int PageLength { get; set; }

        [JsonProperty(PropertyName = "sort")]
        public string Sort { get; set; }

        [JsonProperty(PropertyName = "is_descending")]
        public bool? IsDescending { get; set; }

        [JsonProperty(PropertyName = "total_results")]
        public int TotalResults { get; private set; }

        public static Paging FromHeader(string header)
        {
            return JsonConvert.DeserializeObject<Paging>(header);
        }

        public Paging()
        {
            // these are default values, and will all be ignored in the Stringify() 
            // output if left as-is.
            Page = 0;
            PageLength = 0;
            Sort = null;
            IsDescending = null;
            TotalResults = -1;
        }

        public Paging(int page)
            : this()
        {
            Page = page;
        }

        public Paging(int page, int pageLength)
            : this(page)
        {
            PageLength = pageLength;
        }

        public Paging(int page, int pageLength, string sort)
            : this(page, pageLength)
        {
            Sort = sort;
        }

        public Paging(int page, int pageLength, string sort, bool isDescending)
            : this(page, pageLength, sort)
        {
            IsDescending = isDescending;
        }

        public int GetCount()
        {
            // return non-zero if the caller has set any parameters.
            int count = 0;
            if (Page >= 1) count++;
            if (PageLength >= 1) count++;
            if (Sort != null) count++;
            if (IsDescending.HasValue) count++;
            return count;
        }

        /// <summary>
        /// Returns value for the HTTP "Paging" header, or null if none.
        /// 
        /// If GetCount() returns zero, this method will return null. Otherwise it
        /// returns a string represenging the paging header containing one or more of the
        /// parameters "page", "page_length", "sort", and "is_descending".
        /// 
        /// If the caller has not set a parameter explicity, either via a constructor or
        /// by setting a property, it will not be included in the result.
        /// </summary>
        /// <returns>The paging header value.</returns>
        public string Stringify()
        {
            int count = GetCount();
            if (count == 0)
                return null;

            return JsonConvert.SerializeObject(this);
        }

        public bool ShouldSerializePage()
        {
            return Page >= 1;
        }

        public bool ShouldSerializePageLength()
        {
            return PageLength >= 1;
        }

        public bool ShouldSerializeTotalResults()
        {
            return false;
        }
    }
}
