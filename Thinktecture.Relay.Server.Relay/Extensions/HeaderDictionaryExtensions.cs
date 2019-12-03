using System.Linq;
using System.Net.Http.Headers;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Http
{
	public static class HeaderDictionaryExtensions
	{
		public static void CopyFrom(this IHeaderDictionary headers, HttpHeaders others)
		{
			if (others == null) return;

			foreach (var header in others)
			{
				headers[header.Key] = header.Value.ToArray();
			}
		}
	}
}
