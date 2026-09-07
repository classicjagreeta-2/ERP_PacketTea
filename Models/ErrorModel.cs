using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models
{
    public class ErrorModel
    {
        public class Messages
        {
            public List<string> messages { get; set; }
        }

        public class Errors
        {
            public string type { get; set; }
            public string title { get; set; }
            public int status { get; set; }
            public Messages messages { get; set; }
            public string traceId { get; set; }
        }


    }
}