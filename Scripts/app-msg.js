/*
 * Packet Tea validation / information messages -> one styled dialog (red top bar, warning icon,
 * bold values, OKAY button), instead of toastr toasts or the browser's native alert.
 * Loaded from _Layout.
 *
 *   AppMsg.show(message, onOk, kind, title)   kind: 'error' | 'warning' (default) | 'success'
 *
 * Also takes over native window.alert() and jquery-confirm's $.alert() so every validation
 * message on every screen looks the same. Confirmations ($.confirm, showConfirm) are untouched.
 * A message about a date gets the title "Invalid Date !".
 */
(function (window) {
    'use strict';
    if (window.AppMsg) { return; }

    var ICONS = {
        warning: '<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M12 2.5c.7 0 1.3.4 1.7 1l9 15.6c.8 1.3-.2 2.9-1.7 2.9H3c-1.5 0-2.5-1.6-1.7-2.9l9-15.6c.4-.6 1-1 1.7-1z"/><rect x="10.9" y="8.2" width="2.2" height="7" rx="1.1" fill="#fff"/><circle cx="12" cy="18" r="1.3" fill="#fff"/></svg>',
        success: '<svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="11" fill="currentColor"/><path d="M6.8 12.4l3.3 3.3 7.1-7.2" fill="none" stroke="#fff" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"/></svg>'
    };

    var css = document.createElement('style');
    css.textContent =
        '.am-overlay{position:fixed;inset:0;background:rgba(0,0,0,.4);z-index:100003;display:flex;align-items:center;justify-content:center;padding:16px;opacity:0;transition:opacity .15s}' +
        '.am-overlay.am-show{opacity:1}' +
        '.am-box{--am:#e8574a;--am-dark:#d64536;width:420px;max-width:100%;background:#fff;border-radius:8px;border-top:5px solid var(--am);box-shadow:0 8px 24px rgba(0,0,0,.25);padding:14px 18px;font-family:inherit;transform:scale(.96);transition:transform .15s}' +
        '.am-overlay.am-show .am-box{transform:none}' +
        '.am-box.am-success{--am:#2e9e5b;--am-dark:#25824a}' +
        '.am-head{display:flex;align-items:center;gap:8px;margin-bottom:10px}' +
        '.am-head svg{flex:0 0 22px;width:22px;height:22px;color:var(--am)}' +
        '.am-title{margin:0;font-size:17px;font-weight:600;color:#111;line-height:1.2}' +
        '.am-msg{color:#111;font-size:13px;line-height:1.45;white-space:pre-wrap;word-break:break-word;max-height:50vh;overflow-y:auto}' +
        '.am-msg b{font-weight:700}' +
        '.am-user{margin-top:10px;font-size:12px;color:#666}' +
        '.am-actions{display:flex;justify-content:flex-end;margin-top:14px}' +
        '.am-btn{border:0;border-radius:4px;padding:6px 16px;min-width:70px;font-size:13px;font-weight:700;letter-spacing:.5px;text-transform:uppercase;cursor:pointer;background:var(--am);color:#fff}' +
        '.am-btn:hover,.am-btn:focus{background:var(--am-dark);outline:none;box-shadow:0 0 0 3px rgba(232,87,74,.3)}';
    (document.head || document.documentElement).appendChild(css);

    function plain(text) {
        var d = document.createElement('div');
        d.innerHTML = String(text == null ? '' : text).replace(/<br\s*\/?>/gi, '\n');
        return (d.textContent || d.innerText || '').trim();
    }
    function esc(s) {
        return String(s).replace(/[&<>"']/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
        });
    }
    // Escape first, then bold dates and quoted values so the user can spot them.
    function format(text) {
        return esc(text)
            .replace(/\b(\d{4}-\d{2}-\d{2}|\d{2}[\/-]\d{2}[\/-]\d{4})\b/g, '<b>$1</b>')
            .replace(/&quot;([^&]{1,60}?)&quot;/g, '&quot;<b>$1</b>&quot;');
    }
    function kindOf(text) {
        return /\b(saved|deleted|success(fully)?)\b/i.test(text) ? 'success' : 'warning';
    }
    function titleOf(text, kind) {
        if (kind === 'success') { return 'Success !'; }
        if (/\bdate\b/i.test(text) && /(outside|invalid|future|back|range|cannot be|blank|select)/i.test(text)) { return 'Invalid Date !'; }
        return kind === 'error' ? 'Error !' : 'Warning !';
    }

    function show(message, onOk, kind, title) {
        var text = plain(message);
        if (!text) { if (onOk) { onOk(); } return; }
        kind = kind || kindOf(text);

        var ov = document.createElement('div');
        ov.className = 'am-overlay';
        ov.innerHTML = '<div class="am-box' + (kind === 'success' ? ' am-success' : '') + '" role="alertdialog" aria-modal="true">' +
            '<div class="am-head">' + (kind === 'success' ? ICONS.success : ICONS.warning) + '<h3 class="am-title"></h3></div>' +
            '<div class="am-msg"></div><div class="am-actions"><button type="button" class="am-btn">Okay</button></div></div>';
        ov.querySelector('.am-title').textContent = title || titleOf(text, kind);
        ov.querySelector('.am-msg').innerHTML = format(text);
        if (kind !== 'success' && window.AppUserName) {
            var who = document.createElement('div');
            who.className = 'am-user';
            who.innerHTML = 'User: <b></b>';
            who.querySelector('b').textContent = window.AppUserName;
            ov.querySelector('.am-msg').parentNode.insertBefore(who, ov.querySelector('.am-actions'));
        }
        var ok = ov.querySelector('.am-btn');

        function close() {
            document.removeEventListener('keydown', onKey, true);
            ov.classList.remove('am-show');
            setTimeout(function () { if (ov.parentNode) { ov.parentNode.removeChild(ov); } }, 150);
            if (onOk) { onOk(); }
        }
        function onKey(e) {
            if (e.key === 'Escape' || e.key === 'Enter') { e.preventDefault(); e.stopPropagation(); close(); }
        }
        ok.addEventListener('click', close);
        document.body.appendChild(ov);
        document.addEventListener('keydown', onKey, true);
        requestAnimationFrame(function () { ov.classList.add('am-show'); ok.focus(); });
    }

    window.AppMsg = { show: show };

    window.alert = function (message) { show(message); };

    // jquery-confirm's $.alert('text') / $.alert({ title, content, buttons:{ ok:{ action } } }).
    function patchJqueryAlert() {
        var jq = window.jQuery;
        if (!jq || jq.__appMsgAlert) { return; }
        jq.__appMsgAlert = true;
        jq.alert = function (arg) {
            if (typeof arg === 'string') { show(arg); return; }
            arg = arg || {};
            var kind = arg.type === 'green' ? 'success' : undefined;
            var action = null;
            if (arg.buttons) {
                for (var k in arg.buttons) {
                    if (arg.buttons[k] && typeof arg.buttons[k].action === 'function') { action = arg.buttons[k].action; break; }
                    if (typeof arg.buttons[k] === 'function') { action = arg.buttons[k]; break; }
                }
            }
            show(arg.content || arg.title || '', action, kind);
        };
    }
    patchJqueryAlert();
    if (document.readyState === 'loading') { document.addEventListener('DOMContentLoaded', patchJqueryAlert); }

    // Direct toastr.error(msg) / toastr.warning(msg) calls (PacketTeaInvoice, Reference, ...) show
    // the same dialog, so they carry the user name too. toastr.success / info stay toasts.
    function patchToastr() {
        var t = window.toastr;
        if (!t || t.__appMsg) { return; }
        t.__appMsg = true;
        t.error = function (message, title) { show(message || title, null, 'warning'); };
        t.warning = function (message, title) { show(message || title, null, 'warning'); };
    }
    patchToastr();
    if (document.readyState === 'loading') { document.addEventListener('DOMContentLoaded', patchToastr); }
    window.addEventListener('load', patchToastr);
})(window);
