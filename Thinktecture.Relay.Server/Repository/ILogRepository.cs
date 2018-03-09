using System;
using System.Collections.Generic;
using Thinktecture.Relay.Server.Dto;

namespace Thinktecture.Relay.Server.Repository
{
	public interface ILogRepository
	{
		void LogRequest(RequestLogEntry requestLogEntry);
		IEnumerable<RequestLogEntry> GetRecentLogEntriesForLink(Guid linkId, int numberOfEntries);
		IEnumerable<ContentBytesChartDataItem> GetContentBytesChartDataItemsForLink(Guid id, TimeFrame timeFrame);
		IEnumerable<ContentBytesChartDataItem> GetContentBytesChartDataItems(int numberOfDays);
		IEnumerable<RequestLogEntry> GetRecentLogEntries(int numberOfEntries);
	}
}
