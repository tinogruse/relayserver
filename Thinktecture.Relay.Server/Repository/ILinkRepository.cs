using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.Dto;

namespace Thinktecture.Relay.Server.Repository
{
	public interface ILinkRepository
	{
		PageResult<Link> GetLinks(PageRequest paging);
		LinkDetails GetLinkDetails(Guid linkId);
		Link GetLink(string userName);
		CreateLinkResult CreateLink(string displayName, string userName);
		bool UpdateLink(LinkDetails link);
		void DeleteLink(Guid linkId);

		bool Authenticate(string userName, [Optional] string password, out Guid linkId);
		bool IsUserNameAvailable(string userName);
		Task AddOrRenewActiveConnectionAsync(Guid linkId, Guid originId, string connectionId, int connectorVersion, string assemblyVersion);
		Task RenewActiveConnectionAsync(string connectionId);
		Task RemoveActiveConnectionAsync(string connectionId);
		void DeleteAllConnectionsForOrigin(Guid originId);
		LinkConfiguration GetLinkConfiguration(Guid linkId);
		LinkRelayInfo GetLinkRelayInfo(string userName);
	}
}
