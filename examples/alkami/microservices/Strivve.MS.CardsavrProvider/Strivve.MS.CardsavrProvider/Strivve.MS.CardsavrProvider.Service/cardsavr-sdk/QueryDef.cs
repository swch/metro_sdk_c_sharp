using System;
using System.Collections;
using System.Collections.Specialized;

using Switch.CardSavr.Exceptions;


namespace Switch.CardSavr.Http
{
    /// <summary>
    /// This class lets methods in CardSavrHttpClient accept an "object" parameter that
    /// represents either an entity ID, a list of IDs, or a general query composed of 
    /// multiple key/value pairs. This is a maybe a litte unorthodox in the strongly-typed
    /// world of C#, but it simplifies the client programming mode quite a bit.
    /// 
    /// The QueryDef constructor can accept any of the following inputs of type int,
    /// string, IList, or NameValueCollection:
    /// 
    /// Query                   Input Parameter
    /// --------                -----------
    /// empty query             null, an empty IList or NameValueCollection, or a
    ///                         NameValueCollection containing the "id" key with
    ///                         no values
    /// single ID               int
    /// single ID               string
    /// single ID               NameValueCollection containing the "id" key with a 
    ///                         single value
    /// single ID               IList containing a single value
    /// collection              string containing comma-separated IDs
    /// collection              IList containing 2 or more values
    /// collection              NameValueCollection containing the "ids" key
    /// collection              NameValueCollection not meeting any of the criteria above.
    /// </summary>
    internal sealed class QueryDef
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private object _arg;
        private NameValueCollection _nvc;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Switch.QueryDef"/> is empty.
        /// </summary>
        /// <value><c>true</c> if is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty { get { return _arg == null && _nvc == null; } }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Switch.QueryDef"/> is a 
        /// single identifier (as opposed to a list of identifiers or a more complex
        /// query with multiple key/value pairs.
        /// </summary>
        /// <value><c>true</c> if a single identifier; otherwise, <c>false</c>.</value>
        public bool IsID { get { return _arg != null; } }

        /// <summary>
        /// Returns the single identifier if IsID==true, otherwise returns null.
        /// </summary>
        /// <value>The single-identifier or null.</value>
        public string ID { get { return Convert.ToString(_arg); } }

        /// <summary>
        /// Returns the query as a NameValueCollection, which can contain multiple
        /// key/value pairs. If IsID==true, this property will be false.
        /// </summary>
        /// <value>The query as a NameValueCollection.</value>
        public NameValueCollection Collection { get { return _nvc; } }

        public QueryDef(object arg, PropertyBag bag = null, bool allowEmpty = true, bool requireSingle = false)
        {
            if (arg == null)
            {
                _arg = null;
                _nvc = null;
            }
            else if (arg is NameValueCollection)
            {
                ConvertCollection(arg as NameValueCollection);
            }
            else if (arg is IList)
            {
                ConvertList(arg as IList);
            }
            else if (arg is string)
            {
                ConvertStringList((arg as string).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                _arg = Convert.ToString(arg);
                _nvc = null;
            }

            if (IsEmpty && bag != null && bag.ContainsKey("id"))
            {
                // many endpoints allow the ID to be specified in the body or the URL path.
                log.Info($"taking entity ID={bag["id"]} from PropertyBag");
                _arg = bag["id"];
            }

            if (IsEmpty && !allowEmpty)
                throw new ArgumentException("query cannot be empty or null.");

            if (!IsID && requireSingle)
                throw new InvalidQueryException("query/filter must specify exactly one entity.");
        }

        private void ConvertList(IList lst)
        {
            while (lst.Contains(null))
                lst.Remove(null);

            if (lst.Count == 0)
            {
                log.Warn("list of IDs is empty, resulting in an empty query.");
                _arg = null;
                _nvc = null;
            }
            else if (lst.Count == 1)
            {
                // a single ID.
                _arg = Convert.ToString(lst[0]);
                _nvc = null;
            }
            else
            {
                _arg = null;
                _nvc = new NameValueCollection();
                foreach (object o in lst)
                    _nvc.Add("ids", Convert.ToString(o));
            }
        }

        private void ConvertStringList(string[] lst)
        {
            if (lst == null || lst.Length == 0)
            {
                // an empty query.
                _arg = null;
                _nvc = null;
                log.Warn("list of IDs is empty, resulting in an empty query.");
            }
            else if (lst.Length == 1)
            {
                // a single ID.
                _arg = lst[0];
                _nvc = null;
            }
            else
            {
                // a list of IDs gets converted into a NameValueCollection.
                _arg = null;
                _nvc = new NameValueCollection();
                foreach (string s in lst)
                    _nvc.Add("ids", s);
            }
        }

        private void ConvertCollection(NameValueCollection col)
        {
            // this test is a little odd. we can't just use Get() or GetValues() because it
            // can't distinguish between not having the key and having the key with a null value.
            if (Array.Find<string>(col.AllKeys, k => k == "id") != null)
            {
                string[] ids = col.GetValues("id");
                if (ids == null || ids.Length == 0 || string.IsNullOrEmpty(ids[0]))
                {
                    // an empty query.
                    _arg = null;
                    _nvc = null;
                    log.Warn("\"id\" key is empty, resulting in an empty query.");
                }
                else if (ids.Length == 1)
                {
                    // a single ID.
                    _arg = ids[0];
                    _nvc = null;
                }
                else
                {
                    // a list of IDs. this actually shouldn't happen (should use "ids" instead) 
                    // but we can support it anyway.
                    ConvertStringList(ids);
                }
            }
            else if (!col.HasKeys())
            {
                // an empty query.
                _arg = null;
                _nvc = null;
            }
            else
            {
                // a more complex query, possibly containing many key/value pairs.
                _arg = null;
                _nvc = col;
            }
        }
    }
}
