using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models
{
    public class PageModel
    {
        public string search { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }

    public class PageModel<T>
    {
        public PageValue<T> value { get; set; }
        public List<object> formatters { get; set; }
        public List<object> contentTypes { get; set; }
        public object declaredType { get; set; }
        public int statusCode { get; set; }
    }

    public class PageValue<T>
    {
        public List<T> results { get; set; }
        public int currentPage { get; set; }
        public int pageCount { get; set; }
        public int pageSize { get; set; }
        public int rowCount { get; set; }
        public object linkTemplate { get; set; }
        public int firstRowOnPage { get; set; }
        public int lastRowOnPage { get; set; }
    }
}