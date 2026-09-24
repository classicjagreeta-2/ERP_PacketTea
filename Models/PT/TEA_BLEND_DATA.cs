using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Save payload posted to /api/TeaBlend/SaveOrUpdate
    public class TEA_BLEND_DATA
    {
        public T_TEA_BLEND T_TEA_BLEND { get; set; }
        public List<T_TEA_BLEND_DET> T_TEA_BLEND_DET { get; set; } = new List<T_TEA_BLEND_DET>();

        // Set by the client from the same #isEdit hidden field the rest of the page already
        // uses -- lets the controller's AEDV back-date check (Aday vs Eday) tell an Add from
        // an Edit without guessing from ID/DOCNO. Only read by this front end's Save action;
        // harmless if it rides along in the JSON forwarded to the API.
        public bool? IsNew { get; set; }
    }
}
