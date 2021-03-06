using System;
using System.ComponentModel;
using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public class SecurityElement : ConfigurationElement
	{
		private static readonly ConfigurationProperty _authenticationType = new ConfigurationProperty("authenticationType", typeof(AuthenticationType), AuthenticationType.None, ConfigurationPropertyOptions.IsRequired);
		private static readonly ConfigurationProperty _identity = new ConfigurationProperty("identity", typeof(IdentityElement));
		private static readonly ConfigurationProperty _accessTokenRefreshWindow = new ConfigurationProperty("accessTokenRefreshWindow", typeof(TimeSpan), "00:01:00", new TimeSpanConverter(), new PositiveTimeSpanValidator(), ConfigurationPropertyOptions.None);

		private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection()
		{
			_authenticationType,
			_identity,
			_accessTokenRefreshWindow,
		};

		protected override ConfigurationPropertyCollection Properties => _properties;
		public AuthenticationType AuthenticationType => (AuthenticationType)this[_authenticationType];
		public IdentityElement Identity => (IdentityElement)this[_identity];

		[Obsolete("This property is obsolete. Please use the RelayServer - side link configuration from 2.1 onwards.")]
		public TimeSpan AccessTokenRefreshWindow => (TimeSpan)this[_accessTokenRefreshWindow];
	}
}
