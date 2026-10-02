// Unit + Packet (Blend / Packing) Type dropdowns of the list pages' inline "New" row.
// Root UI convention (see CLAUDE.md): a jquery.inputpicker dropdown whose local data is
// filtered on EVERY column shown (Code and Name), not a native <select>.
//
//   initNewRowPickers({ units: [{CODE,NAME}], types: { CODE: 'Name' }, onChange: fn, textField: 'NAME' })
//
// Call it right after the "New" row (containing <input id="newRowUnit"> and
// <input id="newRowPacketType">) has been added to the DOM. The box shows the picked row's
// Name (description); the plugin writes its Code to the original input, so
// $('#newRowUnit').val() is the Code, and '' until a row is actually picked. Pass
// textField: 'CODE' to show the Code instead (AWR Entry). A single permitted Unit is pre-selected.
function initNewRowPickers(cfg) {
    var units = (cfg.units || []).map(function (u) { return { CODE: u.CODE, NAME: u.NAME || u.CODE }; });
    var types = Object.keys(cfg.types || {}).map(function (k) { return { CODE: k, NAME: cfg.types[k] || k }; });
    var fields = [{ name: 'CODE', text: 'Code' }, { name: 'NAME', text: 'Name' }];
    var textField = cfg.textField || 'NAME';

    function init($input, data) {
        $input.inputpicker({
            data: data,
            fields: fields,
            fieldText: textField,
            fieldValue: 'CODE',
            headShow: true,
            filterOpen: true,   // no filterField => every field in `fields` is searched
            autoOpen: true,
            width: 'auto'
        });
        $input.off('change.newrow').on('change.newrow', function () { if (cfg.onChange) cfg.onChange(); });
    }

    if (units.length === 1) $('#newRowUnit').val(units[0].CODE);
    init($('#newRowUnit'), units);
    init($('#newRowPacketType'), types);

    // Picking a type takes its Unit from M_SALETYPE (CODE = type, TRN_TYPE = 'P'); a type with
    // no such row -- or whose Unit isn't linked to the user -- silently leaves the Unit as picked.
    // Opt-in (cfg.typeUnit): only Master Blend, Final Blend and Packing use it.
    var seq = 0;
    if (cfg.typeUnit) $('#newRowPacketType').off('change.typeunit').on('change.typeunit', function () {
        var type = $(this).val(), mine = ++seq;
        if (!type) return;
        $.getJSON(unitLookupUrl(), { type: type }, function (r) {
            if (mine !== seq || !r || !r.unit || !r.allowed) return;
            $('#newRowUnit').val(r.unit).trigger('change');
        });
    });
}

// App root = this script's own URL minus "Scripts/list-new-row-pickers.js" (works under a virtual directory).
function unitLookupUrl() {
    var src = $('script[src*="list-new-row-pickers.js"]').attr('src') || '/Scripts/list-new-row-pickers.js';
    var root = src.split('?')[0].replace(/Scripts\/list-new-row-pickers\.js$/i, '');
    return root + 'MasterBlendEntry/GetUnitForType';
}
