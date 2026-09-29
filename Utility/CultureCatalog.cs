using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace PacketTea.Utility
{
    // One row of appsettings.json -> Localization:SupportedCultures.
    public class CultureConfig
    {
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public string Continent { get; set; }
        // true = a language with no .resx yet: accepted anyway, and its texts show in English
        // until an AWR.<lang>.resx is added.
        public bool AllowEnglishFallback { get; set; }
    }

    // English-variant cultures (en-KE, en-NG, ...) for the screen-language dropdown, read from
    // appsettings.json and grouped by continent. They need no .resx of their own: the resource
    // lookup falls back en-XX -> en -> Resources\AWR.resx. Add a country by adding one JSON line.
    public static class CultureCatalog
    {
        private static List<CultureConfig> _all;

        public static List<CultureConfig> All
        {
            get
            {
                if (_all != null) return _all;
                var list = new List<CultureConfig>();
                try
                {
                    var path = HostingEnvironment.MapPath("~/appsettings.json");
                    var rows = JObject.Parse(File.ReadAllText(path))["Localization"]?["SupportedCultures"] as JArray;
                    if (rows != null)
                        foreach (var r in rows.ToObject<List<CultureConfig>>())
                            if (IsValid(r)) list.Add(r);
                }
                catch { /* no / bad config -> no extra cultures; the fixed language list still works */ }
                return _all = list;
            }
        }

        // Only regional variants of a language that already has a .resx are accepted: English (the
        // neutral AWR.resx), Swahili (sw) and the fixed UiLanguage.Supported languages. Any other base
        // language would need its own .resx, so it is ignored.
        private static bool IsValid(CultureConfig c)
        {
            if (c == null || string.IsNullOrWhiteSpace(c.Code)) return false;
            try
            {
                var lang = CultureInfo.GetCultureInfo(c.Code).TwoLetterISOLanguageName;
                return c.AllowEnglishFallback || lang == "en" || lang == "sw" || UiLanguage.Supported.Any(s => s[0] == lang);
            }
            catch (CultureNotFoundException) { return false; }
        }

        public static bool Contains(string code) =>
            !string.IsNullOrEmpty(code) && All.Any(c => c.Code == code);

        public static IEnumerable<IGrouping<string, CultureConfig>> ByContinent() =>
            All.GroupBy(c => string.IsNullOrWhiteSpace(c.Continent) ? "Other" : c.Continent);
    }
}
