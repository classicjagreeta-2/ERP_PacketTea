/*
 * Global client-side error handling for ERP Packet Tea (loaded from _Layout).
 *
 * 1. AJAX: any failed jQuery request whose page code did NOT already show the
 *    user a message (alert / $.confirm / $.alert / $.dialog / toastr / swal)
 *    gets a popup with the server's message and error reference. Pages that
 *    handle their own errors are left alone, so there are no double popups.
 *    Opt a single request out with  $.ajax({ ..., globalError: false }).
 *
 * 2. JavaScript errors and unhandled promise rejections are sent to
 *    Error/LogClientError so they land in the same NLog file as server errors
 *    instead of only appearing in the browser console.
 *
 * Pages can reuse the message logic in their own .fail() handlers:
 *    .fail(function (xhr, status, err) { alert(ErpErrors.getAjaxErrorMessage(xhr, err)); })
 *
 * Configuration (set before this script): window.ErpErrorConfig = { logUrl, loginUrl }
 */
(function (window, document) {
    'use strict';

    if (window.ErpErrors) {
        return;
    }

    var config = window.ErpErrorConfig || {};
    var DIALOG_TITLE = 'Error';

    // =====================================================================
    // Track user-visible messages, so we know whether a page's own error
    // handler already told the user about a failed request.
    // =====================================================================

    var shownMessageCount = 0;

    function wrapCounter(owner, name) {
        if (!owner) {
            return;
        }
        var original = owner[name];
        if (typeof original !== 'function' || original.__erpCounted) {
            return;
        }
        var wrapped = function () {
            shownMessageCount++;
            return original.apply(this, arguments);
        };
        for (var key in original) {
            if (Object.prototype.hasOwnProperty.call(original, key)) {
                wrapped[key] = original[key];
            }
        }
        wrapped.__erpCounted = true;
        owner[name] = wrapped;
    }

    function wrapMessageFunctions() {
        wrapCounter(window, 'alert');
        var jq = window.jQuery;
        if (jq) {
            wrapCounter(jq, 'confirm');
            wrapCounter(jq, 'alert');
            wrapCounter(jq, 'dialog');
        }
        if (window.toastr) {
            wrapCounter(window.toastr, 'error');
            wrapCounter(window.toastr, 'warning');
            wrapCounter(window.toastr, 'info');
            wrapCounter(window.toastr, 'success');
        }
        wrapCounter(window, 'swal');
        if (window.Swal) {
            wrapCounter(window.Swal, 'fire');
        }
    }

    // =====================================================================
    // Message helpers
    // =====================================================================

    function escapeHtml(text) {
        return String(text == null ? '' : text)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function readJson(jqXHR) {
        if (jqXHR.responseJSON) {
            return jqXHR.responseJSON;
        }
        var text = jqXHR.responseText;
        if (text && /^\s*[{[]/.test(text)) {
            try {
                return JSON.parse(text);
            } catch (e) {
                return null;
            }
        }
        return null;
    }

    function looksLikeHtml(text) {
        return !!text && /^\s*</.test(text);
    }

    function isSessionProblem(jqXHR) {
        if (jqXHR.status === 401 || jqXHR.status === 403) {
            return true;
        }
        // Session expired: the server redirected to the login page, so the
        // request "succeeded" with HTML where the script expected JSON.
        return jqXHR.status >= 200 && jqXHR.status < 300 && looksLikeHtml(jqXHR.responseText);
    }

    function getAjaxErrorMessage(jqXHR, thrownError) {
        if (!jqXHR) {
            return 'The request failed.';
        }

        if (jqXHR.status === 0) {
            if (jqXHR.statusText === 'timeout') {
                return 'The request timed out. Please try again.';
            }
            return 'Unable to reach the server. Please check your network connection and try again.';
        }

        var json = readJson(jqXHR);
        if (json && (json.message || json.Message)) {
            return json.message || json.Message;
        }

        if (isSessionProblem(jqXHR)) {
            return 'Your session has expired or you are not authorised for this action. Please log in again.';
        }

        if (jqXHR.status >= 200 && jqXHR.status < 300) {
            return 'The server response could not be read.';
        }

        if (jqXHR.status === 404) {
            return 'The requested resource was not found (404).';
        }

        var detail = thrownError ? (thrownError.message || thrownError) : jqXHR.statusText;
        return 'Request failed (' + jqXHR.status + (detail ? ' ' + detail : '') + ').';
    }

    var lastShown = { text: null, at: 0 };

    function showError(message, offerLogin) {
        // Several parallel requests failing for the same reason (e.g. network
        // down) should produce one popup, not five.
        var now = Date.now();
        if (message === lastShown.text && now - lastShown.at < 3000) {
            return;
        }
        lastShown = { text: message, at: now };

        var jq = window.jQuery;
        var loginUrl = config.loginUrl;

        if (!(offerLogin && loginUrl) && window.AppMsg) {
            window.AppMsg.show(message, null, 'error');
        } else if (jq && typeof jq.confirm === 'function') {
            var buttons = { Okay: function () { } };
            if (offerLogin && loginUrl) {
                buttons = {
                    Login: function () { window.location.href = loginUrl; },
                    Close: function () { }
                };
            }
            jq.confirm({
                title: DIALOG_TITLE,
                type: 'red',
                content: escapeHtml(message),
                typeAnimated: true,
                buttons: buttons
            });
        } else {
            window.alert(message);
        }
    }

    // =====================================================================
    // Server-side logging of browser errors
    // =====================================================================

    var reportedKeys = {};
    var reportCount = 0;
    var MAX_REPORTS_PER_PAGE = 20;

    function report(kind, message, source, line, column, stack) {
        try {
            if (!config.logUrl) {
                return;
            }
            var key = kind + '|' + message + '|' + source + '|' + line;
            if (reportedKeys[key] || reportCount >= MAX_REPORTS_PER_PAGE) {
                return;
            }
            reportedKeys[key] = true;
            reportCount++;

            var fields = {
                kind: kind,
                message: String(message == null ? '' : message).slice(0, 2000),
                source: source || '',
                line: line == null ? '' : String(line),
                column: column == null ? '' : String(column),
                stack: String(stack || '').slice(0, 4000),
                pageUrl: window.location.href
            };

            var body = [];
            for (var name in fields) {
                if (Object.prototype.hasOwnProperty.call(fields, name)) {
                    body.push(encodeURIComponent(name) + '=' + encodeURIComponent(fields[name]));
                }
            }
            var payload = body.join('&');

            if (navigator.sendBeacon && window.Blob) {
                navigator.sendBeacon(config.logUrl, new Blob([payload], { type: 'application/x-www-form-urlencoded' }));
            } else {
                var xhr = new XMLHttpRequest();
                xhr.open('POST', config.logUrl, true);
                xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                xhr.send(payload);
            }
        } catch (e) {
            // Reporting must never throw.
        }
    }

    window.addEventListener('error', function (e) {
        // Resource load failures (img/script 404) don't bubble to window, so
        // only real script errors arrive here. "Script error." is the browser's
        // detail-less message for cross-origin (CDN) scripts - nothing to log.
        if (!e || e.message === 'Script error.' || !e.message) {
            return;
        }
        report('js-error', e.message, e.filename, e.lineno, e.colno, e.error && e.error.stack);
    });

    window.addEventListener('unhandledrejection', function (e) {
        var reason = e ? e.reason : null;
        var message = reason && reason.message ? reason.message : String(reason);
        report('unhandled-promise', message, '', null, null, reason && reason.stack);
    });

    // =====================================================================
    // jQuery AJAX
    // =====================================================================

    var pageIsUnloading = false;
    var unloadResetTimer = null;

    function markUnloading() {
        // Requests aborted because the user is navigating away fail with
        // status 0 - that's not a network problem worth a popup. Reset after a
        // few seconds in case a "leave this page?" prompt was cancelled.
        pageIsUnloading = true;
        clearTimeout(unloadResetTimer);
        unloadResetTimer = setTimeout(function () { pageIsUnloading = false; }, 3000);
    }

    window.addEventListener('beforeunload', markUnloading);
    window.addEventListener('pagehide', markUnloading);

    function installAjaxHandling(jq) {
        if (!jq || jq.__erpAjaxErrorHandling) {
            return;
        }
        jq.__erpAjaxErrorHandling = true;

        // Prefilters run before the request's own error/fail callbacks are
        // attached, so this runs first on failure and records how many
        // messages had been shown before the page's handlers get their turn.
        jq.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            jqXHR.fail(function () {
                jqXHR.__erpMessagesBeforeFail = shownMessageCount;
            });
        });

        jq(document).ajaxError(function (event, jqXHR, settings, thrownError) {
            if (!jqXHR || (settings && settings.globalError === false)) {
                return;
            }
            if (jqXHR.statusText === 'abort') {
                return;
            }

            var before = jqXHR.__erpMessagesBeforeFail;

            // Delay a little: .then()/.catch() handlers run asynchronously in
            // jQuery 3, after this event - give them the chance to report first.
            setTimeout(function () {
                if (jqXHR.status === 0 && pageIsUnloading) {
                    return;
                }
                if (typeof before === 'number' && shownMessageCount > before) {
                    return; // the page already told the user
                }
                showError(getAjaxErrorMessage(jqXHR, thrownError), isSessionProblem(jqXHR));
            }, jqXHR.status === 0 ? 300 : 50);
        });
    }

    function install() {
        wrapMessageFunctions();
        installAjaxHandling(window.jQuery);
    }

    install();

    // Views load their own plugins (and occasionally another jQuery) after
    // the layout - wrap/install again once everything has been parsed.
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', install);
    }

    window.ErpErrors = {
        getAjaxErrorMessage: getAjaxErrorMessage,
        showError: showError,
        report: report
    };
})(window, document);
