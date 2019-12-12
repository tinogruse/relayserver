using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Thinktecture.Relay.Server.Authentication.Extensions
{
	public static class EndpointRouteBuilderExtensions
	{
		public static IRelayServerEndpointRouteBuilder WithRelayServerAuthentication(this IRelayServerEndpointRouteBuilder routeBuilder)
		{
			routeBuilder.Map("/token", context => {
				context.Response.ContentType = "application/json";
				return context.Response.WriteAsync(@"{
	""access_token"": ""eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1bmlxdWVfbmFtZSI6ImFkbWluIiwicm9sZSI6IkFkbWluIiwiVXNlcklkIjoiZDQ2MzU0ZjItYTRjOC00MTAyLTkxYjItODQwMzcxZGEwYmU2IiwiaXNzIjoiaHR0cDovL3RoaW5rdGVjdHVyZS5jb20vcmVsYXlzZXJ2ZXIvc3RzIiwiYXVkIjoiaHR0cDovL3RoaW5rdGVjdHVyZS5jb20vcmVsYXlzZXJ2ZXIvY29uc3VtZXJzIiwiZXhwIjoxNjA3Njg1MjIzLCJuYmYiOjE1NzYxNDkyMjN9.T4bxSmPaEEMYdHuzLKSEGLN1CBJEYZ2T81tSHmpWWwM"",
	""token_type"": ""bearer"",
	""expires_in"": 31535999
}", Encoding.UTF8, context.RequestAborted);
			});

			return routeBuilder;
		}
	}
}
