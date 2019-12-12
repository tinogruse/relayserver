using System;
using System.Reactive.Subjects;
using Thinktecture.Relay.Server.Relay.Models;

namespace Thinktecture.Relay.Server.Relay.Services
{
	public interface IRequestResponseQueue
	{
		IObservable<OnPremiseRequest> Requests { get; }
		IObservable<RelayedResponse> Responses { get; }

		void SendRequest(OnPremiseRequest request);
		void SendResponse(RelayedResponse response);
	}

	public class InMemoryRequestResponseQueue : IRequestResponseQueue
	{
		private readonly Subject<OnPremiseRequest> _requests = new Subject<OnPremiseRequest>();
		private readonly Subject<RelayedResponse> _responses = new Subject<RelayedResponse>();

		public IObservable<OnPremiseRequest> Requests => _requests;
		public IObservable<RelayedResponse> Responses => _responses;

		public void SendRequest(OnPremiseRequest request)
		{
			_requests.OnNext(request);
		}

		public void SendResponse(RelayedResponse response)
		{
			_responses.OnNext(response);
		}
	}
}
