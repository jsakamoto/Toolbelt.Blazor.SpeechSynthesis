#if NET5_0
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Components
{
    public static class NavigationManagerExtensions
    {
        public static string GetUriWithQueryParameters(this NavigationManager navigationManager, IReadOnlyDictionary<string, object?> parameters)
        {
            return navigationManager.Uri;
        }

        public static void NavigateTo(this NavigationManager navigationManager, string uri, bool replace)
        {
        }
    }
}
#endif