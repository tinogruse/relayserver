using System;

namespace Thinktecture.Relay.Server.EntityFramework.Entities
{
	/// <summary>
	/// Represents a client secret which a Connector for a <see cref="Tenant"/> can use.
	/// </summary>
	public class ClientSecret
	{
		/// <summary>
		/// The unique Id of this client secret.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The Id of the <see cref="Tenant"/> this secret is for.
		/// </summary>
		public int TenantId { get; set; }

		/// <summary>
		/// A Sha256 or Sha512 of the secret.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Defines when this secret will become invalid.
		/// </summary>
		public DateTime? Expiration { get; set; }

		/// <summary>
		/// Indicates when this secret was created.
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// The <see cref="Tenant"/> for which this secret is valid.
		/// </summary>
		public Tenant Tenant { get;  set; }
	}
}
