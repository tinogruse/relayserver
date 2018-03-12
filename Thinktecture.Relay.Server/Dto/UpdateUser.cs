using System;

namespace Thinktecture.Relay.Server.Dto
{
	public class UpdateUser : CreateUser
	{
		public string CurrentPassword { get; set; }
	}
}
