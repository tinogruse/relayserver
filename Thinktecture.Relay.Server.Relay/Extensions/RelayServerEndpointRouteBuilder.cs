using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Thinktecture.Relay.Server.Relay.Extensions
{
	internal class RelayServerEndpointRouteBuilder : IRelayServerEndpointRouteBuilder
	{
		private readonly IEndpointRouteBuilder _endpoints;

		public ICollection<EndpointDataSource> DataSources => _endpoints.DataSources;
		public IServiceProvider ServiceProvider => _endpoints.ServiceProvider;
		public IApplicationBuilder CreateApplicationBuilder() => _endpoints.CreateApplicationBuilder();

		public RelayServerEndpointRouteBuilder(IEndpointRouteBuilder endpoints)
		{
			_endpoints = endpoints ?? throw new ArgumentNullException(nameof(endpoints));
		}
	}
}
