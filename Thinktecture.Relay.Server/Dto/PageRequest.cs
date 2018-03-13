namespace Thinktecture.Relay.Server.Dto
{
	public class PageRequest
	{
		public string SearchText { get; set; }
		public SortDirection SortDirection { get; set; }
		public string SortField { get; set; }
		public int PageIndex { get; set; }
		public int PageSize { get; set; }
	}
}
