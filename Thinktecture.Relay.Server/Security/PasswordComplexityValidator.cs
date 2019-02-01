using System;
using System.Collections.Generic;
using System.Linq;

namespace Thinktecture.Relay.Server.Security
{
	internal class NoopPasswordComplexityValidator : IPasswordComplexityValidator
	{
		public bool ValidatePassword(string userName, string password, out string errorMessage)
		{
			errorMessage = null;
			return true;
		}
	}

	internal class PasswordComplexityValidator : IPasswordComplexityValidator
	{
		public bool ValidatePassword(string userName, string password, out string errorMessages)
		{
			var result = new List<string>();

			// user must not use the username as password
			if (userName.Equals(password, StringComparison.InvariantCultureIgnoreCase))
			{
				result.Add("Username and password must not be the same.");
			}

			if (password.Length < 8)
			{
				result.Add("Password needs to be at least 8 characters long.");
			}

			if (!password.Any(Char.IsLower))
			{
				result.Add("Password must contain at least one lower case character.");
			}

			if (!password.Any(Char.IsUpper))
			{
				result.Add("Password must contain at least one upper case character.");
			}

			if (!password.Any(Char.IsDigit))
			{
				result.Add("Password must contain at least one number.");
			}

			if (password.All(Char.IsLetterOrDigit))
			{
				result.Add("Password must contain at least one special character.");
			}

			errorMessages = String.Join("\n", result);

			return result.Count == 0;
		}
	}
}
