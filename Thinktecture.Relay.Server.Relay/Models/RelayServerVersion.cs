namespace Thinktecture.Relay.Server.Relay.Models
{
	/// <summary>
	/// Provides version information about the RelayServer application and parts.
	/// </summary>
	public class RelayServerVersion
	{
		/// <summary>
		/// Gets the version of the Thinktecture.Relay.Server.Relay assembly.
		/// </summary>
		public string RelayVersion { get; set; }

		/// <summary>
		/// Gets the version of the application assembly hosting the RelayServer Relay part.
		/// </summary>
		public string HostVersion { get; set; }
	}
}
