using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models
{
    public class ResponseApiModel<TData>
    {
        public string StatusCode { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public TData Data { get; set; }
        public bool IsSuccessStatusCode { get; set; }
        public string redirectUrl { get; set; }

        // The API rejected the request on a business rule (400/404/409 -- e.g. "Packing
        // Done Against This Blend Sheet", month lock) rather than failing. Those are shown
        // to the user as a warning, not an error.
        public bool IsValidationFailure =>
            !IsSuccessStatusCode && (StatusCode == "BadRequest" || StatusCode == "Conflict" || StatusCode == "NotFound");

        // TempData key for a failed call: validation failures get the warning toast.
        public string FailureToastKey => IsValidationFailure ? "toastrWarning" : "toastrError";
    }
}