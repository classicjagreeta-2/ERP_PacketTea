using Newtonsoft.Json;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Web;

namespace PacketTea.Utility
{
    // Screen-language support (i18n) -- currently wired into AWR Entry only (list + entry screen).
    // The user's pick lives in the "ui_lang" cookie; AWREntryController applies it to the request's
    // UI culture (CurrentUICulture only -- CurrentCulture, which drives date/number parsing, is left
    // alone). Texts come from Resources\AWR.resx (English, the fallback) and AWR.<lang>.resx.
    public static class UiLanguage
    {
        public const string CookieName = "ui_lang";

        // Code -> the language's own name, as shown in the dropdown. Add a language by adding a row
        // here and a Resources\AWR.<code>.resx (listed in PacketTea.csproj as EmbeddedResource).
        public static readonly string[][] Supported =
        {
            new[] { "en", "English" },
            new[] { "hi", "हिन्दी" },
            new[] { "bn", "বাংলা" },
            new[] { "fr", "Français" },
            new[] { "es", "Español" },
            new[] { "de", "Deutsch" }
        };

        public static bool IsSupported(string code) =>
            !string.IsNullOrEmpty(code) && (Supported.Any(s => s[0] == code) || CultureCatalog.Contains(code));

        public static string FromRequest(HttpRequestBase request)
        {
            var code = request?.Cookies[CookieName]?.Value;
            return IsSupported(code) ? code : "en";
        }

        public static void Apply(HttpRequestBase request)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(FromRequest(request));
        }

        public static HttpCookie MakeCookie(string code) =>
            new HttpCookie(CookieName, IsSupported(code) ? code : "en") { Expires = System.DateTime.Now.AddYears(1), HttpOnly = true };

        // Two-letter code of the current request's UI culture (for <html lang> / JS).
        // Full code of the current UI culture (e.g. "en-KE"), to preselect the dropdown.
        public static string CurrentCode => Thread.CurrentThread.CurrentUICulture.Name;

        public static string Current => Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
    }

    // AWR Entry's texts (Resources\AWR*.resx) for the current UI culture; a missing key falls back
    // to English, then to the key itself.
    public static class AwrText
    {
        private static readonly ResourceManager Rm =
            new ResourceManager("PacketTea.Resources.AWR", typeof(AwrText).Assembly);

        public static string T(string key) => Rm.GetString(key) ?? key;

        public static string F(string key, params object[] args) => string.Format(T(key), args);

        // { key: text } for the given keys, as JSON safe to drop inside a <script> block.
        public static string Json(params string[] keys) =>
            JsonConvert.SerializeObject(keys.Distinct().ToDictionary(k => k, T),
                new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });
    }
}
