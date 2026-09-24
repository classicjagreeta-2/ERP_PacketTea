# PacketTea (MVC front end) — project rules

## Unit-wise permission rule for Blend screens (left-hand menu)

Users are granted access to specific Units via the `USER_SCHEMA_LINK` table
(`Models/user_schema_link.cs`). For the left-hand navigation's Blend-related
lists — **MasterBlend Entry**, **Final Blend Entry**, and **Packing entry
(from Blend)** — the list must only show/offer the Units the logged-in user
has permission for in `USER_SCHEMA_LINK`. Never list or default to a unit the
user isn't linked to.

Established pattern, now used identically by all three controllers —
`MasterBlendEntryController.cs`, `FinalBlendEntryController.cs`, and
`PackingController.cs`:

- Controller has a `GetUnitsForUserAsync()` helper that calls the API
  (`GET /api/TeaBlend/GetUnitsForUser`), which resolves the current user's
  allowed units from `USER_SCHEMA_LINK`.
- The result is assigned to `ViewBag.UnitList` in the `Index` action and
  rendered as a Unit dropdown on the list page (`UNIT_LIST` JS variable /
  `#unitFilter`, same pattern as `Views/TeaPurchaseNote/Index.cshtml` and
  `Views/PacketTeaNote/Index.cshtml`). Auto-selects the unit when the user
  only has one.
- `InsertOrUpdate` takes an optional `unit` query param — set when opened
  from the list's Unit picker — which is honored over the `BlendTypeUnit`
  stopgap guess and locks Blend Type read-only (`LockedFromList`). The
  chosen Unit is shown read-only (`#UNIT_disp`) on the entry screen and **is
  posted on Save** (`UNIT: $('#UNIT_disp').val()`), so the record is stored
  under the Unit picked on the list. `Save` rejects a NEW record whose Unit
  isn't one of the user's `GetUnitsForUser` units (`UnitScope.IsAllowed`,
  `Utility/Helpers.cs`); edits keep the record's own Unit. `UnitForBlendType`
  (the fixed Blend-Type -> Unit table) is now only the fallback when no Unit
  was posted.

**Unit + Packet Type (Blend Type) scoping** (added 2026-09-24):
- List `Index` actions take an optional `unit` and always send the API
  `UnitScope.ListFilter(unit, userUnits)` — the one unit asked for (if
  permitted) else **every unit the user is linked to** as a comma-separated
  list (API `TeaBlendController.ParseUnits`). `CurrentUnit` is always blank
  (`CurentUnit` is never set), so it must not be used as a filter.
- **List performance (2026-09-24):** Master Blend's `Index` sends every permitted
  Blend Type as ONE comma-separated `blendType` to `GetByPage` (API `ParseUnits`
  style) and pages server-side -- never one call per type. The three API
  `GetByPage` endpoints resolve Party/Allocation/Mark/Grade/Transporter/Sales
  Centre names only for the shown page (`TeaBlendController.LookupNames`);
  they read the whole master table only when sorting on that name column.
  Don't reintroduce whole-table `_query.ToList()` lookups in list endpoints.
- **"+" expand rows** load through the API's lean `GetRowLines` (`/api/TeaBlend|FinalBlend|BlendPacking/GetRowLines`,
  `{ details: [...] }` only) in ONE call -- not `GetRowDetail` + `GetByDocNo`, which
  ran the edit guards and read whole master tables. Keep new expand rows on that pattern.
- The "New" inline row's **Unit dropdown** (not the Unit column) is made wider by
  letting its cell span 3 columns (`colspan="3"`); Packet Type spans 2.
- **Packet Type dropdown** (list page "New" row) is the same on all three lists: only the
  types the user has rights to (`M_UNIT_USER_RIGHT`), via the shared static
  `MasterBlendEntryController.GetAllowedBlendTypesAsync()` (`GET /api/TeaBlend/GetAllowedBlendTypes`).
  Each list also sends that whole set as a comma-separated `blendType` to its `GetByPage`
  (`TeaBlend` / `FinalBlend` / `BlendPacking` all parse it with `TeaBlendController.ParseUnits`),
  not the single `?blendType=` from the URL. Falls back to all six types if the API call fails.
- Final Blend's Master Blend picker (`GetMasterBlendList` /
  `GetMasterBlendDetail`) and Packing's Final Blend picker (`GetFinalBlendList`
  / `GetFinalBlendRowValues`) take the entry screen's `unit` (`#UNIT_disp`) +
  `blendType`; the controller returns nothing unless `unit` is permitted. The API
  already drops closed / fully-issued (Remaining Qty <= 0) masters and fully
  packed final blends.
- Master Blend's "Select Data" (`GetAvailableStock`) stays scoped by Blend Type
  only — stock rows carry the purchase's own unit, which need not equal the
  Unit picked for the blend.

When adding a new Blend-related list screen, wire it up the same way
(`GetUnitsForUserAsync()` / `GetUnitsForUser`) instead of inventing a new
mechanism.

The actual `USER_SCHEMA_LINK` lookup/filtering logic lives in the API repo,
not here — see the two-repo layout (MVC front end + .NET 8 API); most changes
to these screens need edits in both repos.

## AEDV (Add/Edit/Delete/View) permission model + back-date policy

Ported from `D:\GIT\ERP_Payroll`'s `Utility\Helpers.cs` (`AEDV` class) and
`Controllers\BaseController.cs` (`IsDateValid`) — same shape here, adapted
since this project has no `BaseController`. Currently wired into **Master
Blend Entry**, **Final Blend Entry**, and **Packing Entry**
(`Controllers/TEA/{MasterBlendEntry,FinalBlendEntry,Packing}Controller.cs`).
Apply the same pattern to any other list screen that needs Add/Edit/Delete/
View gating or a back-date restriction — don't invent a new mechanism.

**The model** (`Utility/Helpers.cs`, `PacketTea.Helpers.AEDV`):
- `Aedv` is a string like `"AEDV"`; `Add`/`Edit`/`Delete`/`View` each check
  their own letter via the null-safe `Can(char)` (each property has its own
  backing field — a prior version of this class had all four share `_add`,
  silently corrupting whichever was read last; fixed 2026-09-23).
- `Aday`/`Eday`/`Dday` are the allowed back-date window (in days) for
  Add/Edit/Delete respectively.
- `MinDocDate(bool isNew)` → `Today.AddDays(-(isNew ? Aday : Eday))`.
- `CheckAddEdit(bool isNew, DateTime docDate)` (instance) / the null-safe
  static overload `CheckAddEdit(AEDV permission, bool isNew, DateTime docDate)`
  → `null` if allowed, else the exact message to surface to the user (missing
  right, future-dated, or outside the back-date window).
- Permission rows load into `Session["User_AEDV"]` as `List<AEDV>`, keyed by
  `Controller`. These three screens all share the pre-existing
  `"PacketTeaPurchaseEntry"` bucket (see `MasterBlendEntryController`'s own
  comment) rather than a per-screen key, since `MasterBlendEntry`/
  `FinalBlendEntry`/`Packing` aren't provisioned as separate permission rows —
  match that convention, don't invent new keys, unless the menu/rights setup
  (outside this repo) actually provisions one.

**Wire-up pattern per screen** (see the three controllers for the concrete
diff):
1. `Index` action: `ViewBag.Permission = sdsd?.FirstOrDefault(l => l.Controller == "<bucket>")`.
2. `InsertOrUpdate` GET:
   - Set `ViewBag.Permission` the same way.
   - Redirect to `Index` with `TempData["toastrError"]` if opening **New**
     without the `Add` right, or opening **Edit** on a record whose Doc Date
     (or the Add-right/back-date check) fails `AEDV.CheckAddEdit(...)` —
     block entry to the form outright, don't just disable fields.
   - Compute `ViewBag.MinDocDate` = the *later* of the financial-year start
     and `(permission ?? new AEDV()).MinDocDate(isNew)`, and bind it to the
     Doc Date `<input type="date">`'s `min` attribute (replacing the old
     `ViewBag.FyStart`-only bound) so the picker itself steers users away
     from a rejected back-date, in addition to FY bounds.
3. `Save` POST: re-fetch `permission` from `Session["User_AEDV"]` (a fresh
   POST has no `ViewBag` from the GET), determine `isNew` from the `IsNew`
   flag the client now sends (see below), and call
   `AEDV.CheckAddEdit(permission, isNew, docDate)` **before** touching the
   API — return `{ success = false, message = err }` on failure. This is the
   real enforcement point; the `min` attribute only steers well-behaved
   clients.
4. `Delete` POST: re-fetch `permission`, reject with `TempData["toastrError"]`
   when `!(permission?.Delete ?? false)`, same redirect-to-Index shape as the
   other guards.
5. Client (`InsertOrUpdate.cshtml`): the Save payload must include
   `IsNew: !IS_EDIT` (the page's existing `#isEdit` hidden field / `IS_EDIT`
   var) as a sibling of the header object, and the DTO posted to `Save` needs
   a matching `public bool? IsNew { get; set; }` — the header models here
   (`TEA_BLEND_DATA`, `BLEND_PACKING_DATA`) don't reliably expose new-vs-edit
   any other way (`Packing`'s header carries no ID at all).
6. List page toolbar buttons (`Index.cshtml`): gate New/Edit/Delete/View with
   inline style, exactly the convention already used in `BlendEntry/Index.cshtml`
   and (commented out, so not live) `PacketTeaPurchaseEntry/Index.cshtml`:
   `style="@(!((ViewBag.Permission as PacketTea.Helpers.AEDV)?.Add ?? false) ? "pointer-events:none; opacity:0.5;" : "")"`
   (swap `.Add` for `.Edit`/`.Delete`/`.View` per button). This is UX only —
   the real gate is the controller-side checks above; never rely on the
   button being disabled as the actual security boundary.

## List-page columns (Master / Final Blend Entry, Packing Entry)

Every list starts with **Unit** and **Blend Type** (Packing: **Packing Type**)
and (Final only) ends with an **AEDV** column (`td.aedv-cell`): per-row Edit /
View / Delete icons, each rendered only when `ViewBag.Permission` holds that
right. The icons share the same `goEdit` / `goView` / `doDelete` JS helpers as
the toolbar buttons — UX only, the controller-side checks above remain the
real gate. Unit/Type come straight off each row (`UNIT`, `BLEND_TYPE`);
Packing rows pass their own `BLEND_TYPE` (not the toolbar filter's) to
Edit/Delete.

- Master: Unit, Blend Type, Doc No, Date, Blend No, Blend Date, Warehouse,
  Blend Mark, Blend Grade, Allocation, Party, No of Chest, Net Wt/Chest,
  Closed (reordered 2026-09-24; Blend Details and Gross WT/Chest dropped,
  Blend Date added from `BLEND_DATE`). (**No AEDV column on Master Blend's list** -- removed
  at the user's request 2026-09-24; use the toolbar buttons. **Packing's list has none either**
  -- removed 2026-09-24, same reason. Only Final keeps it.)
- Final: Unit, Blend Type, Doc No, Date, Final Blend No, Date, Master Blend
  No, Date, Warehouse, Blend Mark, Blend Grade, Allocation, Party, No of Chest,
  Net Wt/Chest, Closed, AEDV.
- Packing: Unit, Packing Type, Doc No, Date, Blend No, Date, Location,
  Packing Qty., Short/Excess, Party. **Blend No is a link** into Edit mode (like Master's Blend No),
  rendered only when the user holds the Edit right (plain text otherwise). Rows carry their `UNIT`
  (`data-unit`, link `unit=`) into Edit / Delete, since a Doc No repeats across units;
  `InsertOrUpdate` / `Delete` check that Unit against `GetUnitsForUser`.

All three lists share the same layout: a checkbox column and a "+" expand
column in front (the "+" row lazily loads the document's lines via the
controller's `GetRowDetail`), no Unit / Packet Type dropdowns in the toolbar —
Unit + Packet (Packing) Type are picked in the "New" inline row (pencil icon
opens the entry screen with both locked). All three entry screens (Master, Final,
Packing) show a separate **"Unit / Blend Type"** section above Header containing just
the read-only Unit and the Blend Type dropdown (locked from the list / in Edit). There
is **no Packet Type field** -- it is the same value as Blend Type (changed 2026-09-24).

When a list's column count changes, update the `colspan`s on the "No records"
row, the "+" expand row and the "New" inline row to match.

## List-page toolbar (standard, added 2026-09-24)

Every list screen — **Master Blend Entry, Final Blend Entry, Packing Entry**, and any
new list — uses the same toolbar as `D:\GIT\ERP_Inventory`'s Purchase Order list, one
`<div class="action-btns"><ul>` row above the page title, in this exact order:

**Back · New · Edit · Delete · View · Lines**, then a right-aligned **LIST** badge
(`<li style="margin-left:auto">`, dark rounded label, like Inventory's).

- Icons: Back `fa-arrow-left`, New `fa-plus`, Edit `fa-pencil`, Delete `fa-trash-o`,
  View `fa-eye`, Lines `fa-list` (bullet-list icon).
- **Back** goes to `Menu/Index`. **New / Edit / Delete / View** are gated with the AEDV
  inline style from the AEDV section above (`.Add` / `.Edit` / `.Delete` / `.View`);
  Edit / Delete / View / Lines act on the ticked row (`goEdit` / `doDelete` / `goView`
  helpers). **Lines** opens the ticked row's line details (the same content the "+"
  expand row shows, via `GetRowLines`) — it needs a ticked row, like Edit/View.
- Button order is the same on all three screens — don't reorder per screen, and don't
  drop a button on one screen. UX only: the controller-side AEDV checks remain the real gate.
- Implemented 2026-09-24 on all three lists: `#linesBtn` ticks-row → opens that row's "+"
  expand panel and scrolls it into view; the LIST badge is `<span class="list-badge">`.
  **Packing now has a View mode** like Master/Final: `PackingController.InsertOrUpdate`
  takes `view` (needs the View right, bypasses the Edit / back-date check, sets
  `ViewBag.IsView`), and `Packing/InsertOrUpdate.cshtml` disables the form (`IS_VIEW`).

## List pagination = chunked infinite scroll (added 2026-09-24)

Every list page pages the way `D:\GIT\ERP_Inventory`'s Purchase Order list does:
**no Prev/Next buttons and no "Page x of y"** — the first chunk is rendered with
the page, then each time the list's scroll box is scrolled to the bottom the next
chunk of rows is fetched and appended, and the footer reads
`Showing <loaded> of <total>` (e.g. "Showing 90 of 2098"). Applies to **Master
Blend Entry, Final Blend Entry and Packing Entry** (`Views/*/Index.cshtml`) and to
every new list screen — don't build a Prev/Next pager.

How it is wired (copy it for a new list):
- **Rows partial:** the `<tr>` rows live in `Views/<Screen>/_ListRows.cshtml`
  (model = the list, plus `ViewBag.RowOffset` so per-row ids like `rowDetail<n>`
  stay unique across chunks). Every data row carries `class="data-row"` — the
  loader counts those (the inline "New" row and the "+" detail rows aren't
  counted). `Index.cshtml` renders `@Html.Partial("_ListRows", Model)` inside
  `<tbody id="listBody">` plus a "No records found" row when the list is empty.
- **Controller `Index`:** `var isChunkRequest = Request.IsAjaxRequest();` — a full
  page view always loads page 1 (`pageNo = isChunkRequest ? page : 1`); an AJAX
  request returns `PartialView("_ListRows", list)` (an **empty** list on API
  failure, and no `TempData` toast) so the client stops. Set `ViewBag.Page` /
  `ViewBag.RowOffset = (pageNo - 1) * pageSize` and use `pageNo` (not the raw
  `page`) in the API call. Default `pageSize` stays 15.
- **View:** the table sits in `<div class="table-responsive" id="tableScroll">`
  (CSS: fixed `max-height: calc(100vh - 380px)`, `overflow:auto`, sticky
  `thead th`), footer is `<div id="listLoading">` + `Showing <span id="shownCount">`
  of `@rowCount`, and the page script includes
  `~/Scripts/list-infinite-scroll.js` then calls
  `initInfiniteList({ scroll:'#tableScroll', body:'#listBody', loading:'#listLoading', count:'#shownCount', url, search, pageSize, total, startPage:1 })`.
  The loader also keeps fetching while the first chunk is too short to make the box
  scroll. Search still reloads the page (`?searchString=`), which restarts from the
  first chunk.
- New files must be added to `PacketTea.csproj` as `<Content Include=...>` (old-style
  project — views/scripts not listed there are missed on publish).
- Row event handlers must be **delegated** (`$('#blendTable').on('click', '.x', ...)`)
  so rows appended later work; insert the "New" row with `$('#listBody').prepend(...)`
  (not `#table tbody`, which also matches the nested "+" detail tables).

## Lookup pickers ("Root UI" convention)

Any simple code/name lookup field (single-select: Party, Warehouse,
Allocation, Grade, Mark, Transporter, Sales Centre, Chest Size, etc.) must
use the `jquery.inputpicker` plugin — the same widget most other screens in
this app already use (`PacketTeaPurchaseEntry`, `PacketTeaInvoice`,
`PurchaseContractEntry`, `BlendEntry`'s list page, `TransferMemo`,
`FullsStockActual`, `StockReport`) and the one `D:\GIT\ERP_Inventory`'s own
Purchase Order Entry uses for its Supplier Code field. It renders an
anchored, paginated dropdown directly under the field itself — not a
centered modal dialog with a backdrop.

- Include `~/Controllers/Test/assets/css/jquery.inputpicker.css` and
  `~/Controllers/Test/assets/js/jquery.inputpicker.js` — the exact path
  `PacketTeaPurchaseEntry` already uses and the one proven to work
  end-to-end in this app. (Several other copies of this same plugin exist
  under `libraries/` — don't use those; stick to the one path already in
  production use here.)
- Controller action signature: `(string q = "", int limit = 0, string
  fieldValue = "", string fieldText = "", string value = "", int p = 1)` —
  these exact parameter names are what the plugin's AJAX call sends (`q` for
  the typed search text, `limit`/`p` for paging), not `search`/`page`.
- Response shape: `{ data: [...], count: N }` (plain `Json`/`JsonExact`,
  matching how `PacketTeaPurchaseEntryController.GetMark` already does it).
  The plugin reads `data` for rows and `count` (via `pageCountField`) for
  the page/total count.
- When forwarding to an existing "top-N search" API endpoint that doesn't
  itself support real paging (true of most `/api/TeaBlend/Get*` /
  `/api/BlendPacking/Get*` lookup endpoints here), wrap the returned array
  as `{ data = arr, count = arr.Count }` rather than inventing paging the
  API can't provide — inputpicker still renders correctly with this shape,
  it just never has a second page to fetch. See `MasterBlendEntryController
  .PickerJson` (and the identical helper in `FinalBlendEntryController` /
  `PackingController`) for the exact pattern.
- View-side init, once per input, guarded by `$input.data('inputpicker')`:
  ```js
  $input.inputpicker({
      url: cfg.url,
      fields: cfg.fields,        // [{ name: 'CODE', text: 'Code' }, ...]
      fieldText: cfg.fields[0].name,
      fieldValue: cfg.fields[0].name,
      headShow: true, filterOpen: true, autoOpen: true,
      pagination: true, pageField: 'p', pageLimitField: 'limit', pageLimit: 10, pageCountField: 'count',
      width: 'auto', urlDelay: 1
  });
  $input.off('inputpicker.select').on('inputpicker.select', function (e, row) { /* ...; */ $(this).inputpicker('hide'); });
  ```
  The `select` event does **not** write the picked value into the input for
  you — the handler must call `.val(...)` itself (see how
  `PacketTeaPurchaseEntry`'s own vendor/mark pickers do it).

Implemented (2026-09-23) in **Master Blend Entry**, **Final Blend Entry**,
and **Packing Entry**'s `InsertOrUpdate` screens, replacing their earlier
bespoke centered-modal `#genericPicker`: Party/Warehouse/Allocation/Blend
Grade/Mark/Transporter (Master + Final) and Mark/Grade/Chest
Size/Allocation/Sales Centre (Packing) all now use `jquery.inputpicker`.
Row-level lookups (Mark/Grade/Chest Size/Allocation cells in Packing's grid,
Trans Code in Master/Final Blend's grid) anchor the picker to that row's own
text input instead of a shared modal. In Packing's grid (2026-09-24) the code box
itself is the dropdown (click/focus opens it -- no "…" button) with the picked name
shown beside it (Mark widest), and Alloc / Item / MRP are hidden (`.col-hide`, kept
in the DOM so they still Save). Packing's **Final Blend** picker (header Blend No + each
row's Blend) is now an inputpicker dropdown too (2026-09-24, at the user's request): MVC
`PackingController.GetFinalBlendList` is a picker endpoint (`q`/`limit`/`p` → `{ data, count }`)
that also takes `blendType` + `unit` (sent via the picker's `urlParam`) and re-checks the unit
against `GetUnitsForUser`. This plugin copy never fires `inputpicker.select` — it writes the
picked value to the input and fires `change` — so `openPicker` reads the picked row back from
`$input.inputpicker('data')` on `change` (`pickedRow`); don't rely on the select event alone.

**Every Root UI dropdown is searchable by ALL its columns (added 2026-09-24).** Typing in
an inputpicker (`filterOpen: true`) sends `q` to the picker endpoint, so the filtering
happens there: the endpoint (MVC controller and the API `Get*` action behind it) must
match `q` (case-insensitive, "contains") against **every column the dropdown shows**
— e.g. Code *and* Name, or Blend No *and* Date *and* Party *and* Mark — not just the
first/code column. Every picker's `fields` list is the set of searchable columns, so
when a column is added to `fields`, add it to the server-side `q` match too. Applies to
all Root UI dropdowns on every screen, including the list page's "New" row Unit /
Packet Type pickers and the per-row grid pickers (Mark/Grade/Chest Size/etc.).
Where the list is small and fully client-side (e.g. Unit / Blend Type), filter across all
columns in the browser instead. Don't ship a picker that only searches its code column.
- The list pages' "New" row **Unit / Packet Type** are inputpickers with local data (Code +
  Name, both searchable), not native `<select>`s — `Scripts/list-new-row-pickers.js`
  `initNewRowPickers({ units, types })`, called right after the New row is inserted (it also
  pre-selects a single permitted Unit). `#newRowUnit` / `#newRowPacketType` are text inputs;
  `.val()` is only set once a row is really picked.
- API (2026-09-24): `TeaBlend/GetWarehouse` now also matches Destination; Packing's
  `BlendPacking/GetFinalBlendList` matches Blend No, Doc No (SQL, case-insensitive) and —
  for a numeric search — Blend Qty / Packed / Remaining / Rate (on the computed rows).

**Deliberately NOT converted** — these are composite/bulk selection
screens, not simple code/name lookups, and don't fit `jquery.inputpicker`'s
single-row-picks-one-value model:
- Master Blend Entry's "Select Data" (`#stockPicker` /
  `GetAvailableStock`) — bulk multi-row selection with its own Garden/Mark/
  Category filters.
- Final Blend Entry's "Master Blend" picker (`#masterPicker` /
  `GetMasterBlendList`) — picking a master to raise a final against, with its own
  multi-column, non-code/name layout. (Packing's "Final Blend" picker was in this
  list but has since been converted — see above.)

When adding a new simple lookup field anywhere in this app, use
`jquery.inputpicker` from the start — don't build another bespoke modal
picker.
