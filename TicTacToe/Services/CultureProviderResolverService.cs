using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicTacToe.Services
{
    public class CultureProviderResolverService : RequestCultureProvider
    {
        private static readonly char[] _cookieSeparator = new[] { '|' };
        private static readonly string _culturePrefix = "c=";
        private static readonly string _uiCulturePrefix = "uic=";

        public override async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext) // ustala jaki jest wybrany język
        {
            if (GetCultureFromQueryString(httpContext, out string culture))
                return new ProviderCultureResult(culture, culture);

            else if (GetCultureFromCookie(httpContext, out culture))
                return new ProviderCultureResult(culture, culture);

            else if (GetCultureFromSession(httpContext, out culture))
                return new ProviderCultureResult(culture, culture);

            return await NullProviderCultureResult;
        }

        private bool GetCultureFromQueryString(HttpContext httpContext, out string culture) // pobiera język z url
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var request = httpContext.Request;
            if (!request.QueryString.HasValue)
            {
                culture = null;
                return false;
            }

            culture = request.Query["culture"];
            return true;
        }

        private bool GetCultureFromCookie(HttpContext httpContext, out string culture) // pobiera jezyk z Cookie
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var cookie = httpContext.Request.Cookies["culture"];
            if (string.IsNullOrEmpty(cookie))
            {
                culture = null;
                return false;
            }

            culture = ParseCookieValue(cookie);
            return !string.IsNullOrEmpty(culture);
        }

        public static string ParseCookieValue(string value) // parsuje ciasteczka aby odpowiednio je odczytywać
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var parts = value.Split(_cookieSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return null;
            }

            var potentialCultureName = parts[0];
            var potentialUICultureName = parts[1];

            if (!potentialCultureName.StartsWith(_culturePrefix) || !potentialUICultureName.StartsWith(_uiCulturePrefix))
            {
                return null;
            }

            var cultureName = potentialCultureName.Substring(_culturePrefix.Length);
            var uiCultureName = potentialUICultureName.Substring(_uiCulturePrefix.Length);
            if (cultureName == null && uiCultureName == null)
            {
                return null;
            }

            if (cultureName != null && uiCultureName == null)
            {
                uiCultureName = cultureName;
            }

            if (cultureName == null && uiCultureName != null)
            {
                cultureName = uiCultureName;
            }

            return cultureName;
        }

        private bool GetCultureFromSession(HttpContext httpContext, out string culture) // pobranie języka z sesji
        {
            culture = httpContext.Session.GetString("culture"); ;
            return !string.IsNullOrEmpty(culture);
        }
    }
}
