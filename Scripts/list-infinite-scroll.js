/* Chunked ("infinite scroll") loading for the Master Blend / Final Blend / Packing list pages,
   the same behaviour as ERP_Inventory's Purchase Order list: the first chunk is rendered with
   the page, then every time the scroll box is scrolled to its bottom the next chunk of rows is
   fetched and appended, and the footer shows "Showing <loaded> of <total>".

   The list's Index action must return ONLY the <tr> rows (PartialView "_ListRows") when
   Request.IsAjaxRequest(), and every data row must carry class="data-row" (the "New" inline row
   and the "+" detail rows are not counted).

   initInfiniteList({
       scroll:   '#tableScroll',   // the box that scrolls (fixed max-height, overflow:auto)
       body:     '#listBody',      // <tbody> the chunks are appended to
       loading:  '#listLoading',   // "Loading..." element shown while a chunk is in flight
       count:    '#shownCount',    // element showing how many rows are loaded so far
       url:      '/Controller/Index',
       search:   'current search text',
       pageSize: 15,
       total:    2098,             // total rows for the current search (server rowCount)
       startPage: 1,               // page already rendered with the page itself
       extra:    { sortBy: 'DOCNO', sortDir: 'asc' }   // optional: more query params sent with every chunk
   });
*/
(function (w) {
    w.initInfiniteList = function (o) {
        var page = o.startPage || 1, loading = false, done = false;
        var $scroll = $(o.scroll), $body = $(o.body);

        function loaded() { return $body.children('tr.data-row').length; }
        function hasMore() { return !done && loaded() < o.total; }
        function updateCount() { $(o.count).text(Math.min(loaded(), o.total)); }

        // First chunk too short to make the box scroll (tall screen / small chunk) -> keep
        // loading until it overflows or the list is exhausted, otherwise the user could
        // never trigger the scroll event.
        function fillIfShort() {
            var el = $scroll[0];
            if (el && hasMore() && !loading && el.scrollHeight <= el.clientHeight + 10) loadNext();
        }

        function loadNext() {
            if (loading || !hasMore()) return;
            loading = true;
            $(o.loading).show();
            $.get(o.url, $.extend({ searchString: o.search, page: page + 1, pageSize: o.pageSize }, o.extra || {}))
                .done(function (html) {
                    var $rows = $('<tbody>').html(html).children('tr');
                    if ($rows.filter('.data-row').length === 0) { done = true; return; }
                    $body.append($rows);
                    page++;
                })
                .fail(function () { done = true; })   // don't loop on a failing endpoint
                .always(function () {
                    loading = false;
                    $(o.loading).hide();
                    updateCount();
                    fillIfShort();
                });
        }

        $scroll.on('scroll', function () {
            var el = this;
            if (el.scrollTop + el.clientHeight >= el.scrollHeight - 40) loadNext();
        });
        $(w).on('resize', fillIfShort);

        updateCount();
        fillIfShort();
    };
})(window);
