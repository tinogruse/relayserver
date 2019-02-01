using System.Collections.Generic;
using Thinktecture.Relay.Server.Security;

namespace Thinktecture.Relay.CustomCodeDemo
{
	internal class NoopPasswordComplexityValidator : IPasswordComplexityValidator
	{
		public bool ValidatePassword(string userName, string password, out IEnumerable<string> errorMessages)
		{
			errorMessages = null;
			return true;
		}
	}
}
