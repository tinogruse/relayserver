using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Thinktecture.Relay.Server.Relay.Options
{
	public class RelayServerOptions
	{
		
		/// <summary>
		/// The buffer size to use when copying the response stream to the response.
		/// </summary>
		public int StreamCopyBufferSize { get; set; } = 81920;

		/// <summary>
		/// The absolute path where the relay endpoint will listen to requests.
		/// </summary>
		/// <remarks>Must start with a slash.</remarks>
		public string AbsoluteRelayPath { get; set; } = "/relay";

		/// <summary>
		/// The absolute path where the SignalR OnPremises Hub will listen to connections.
		/// </summary>
		/// <remarks>Must start with a slash.</remarks>
		public string AbsoluteConnectorPath { get; set; } = "/onPremisesConnection";

		/// <summary>
		/// The absolute path where the OnPremises will talk to the relay server.
		/// </summary>
		/// <remarks>Must start with a slash.</remarks>
		public string AbsoluteOnPremisesPath { get; set; } = "/onpremises";
	}

	public class RelayServerOptionsValidator : IValidateOptions<RelayServerOptions>
	{
		public ValidateOptionsResult Validate(string name, RelayServerOptions options)
		{
			var failures = new List<string>();

			ValidatePath(options.AbsoluteRelayPath, nameof(RelayServerOptions.AbsoluteRelayPath), failures);
			ValidatePath(options.AbsoluteConnectorPath, nameof(RelayServerOptions.AbsoluteConnectorPath), failures);
			ValidatePath(options.AbsoluteOnPremisesPath, nameof(RelayServerOptions.AbsoluteOnPremisesPath), failures);
			
			return (failures.Count > 0) ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
		}

		private void ValidatePath(string path, string argumentName, List<string> failures)
		{
			if (String.IsNullOrEmpty(path))
			{
				failures.Add($"{argumentName} must be specified.");
			}
			else if (!path.StartsWith("/"))
			{
				failures.Add($"{argumentName} must start with a slash (/)");
			}
		}
	}
}
