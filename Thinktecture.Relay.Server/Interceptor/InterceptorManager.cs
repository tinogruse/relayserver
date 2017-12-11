using System;
using System.Net;
using System.Net.Http;
using Serilog;
using Thinktecture.Relay.Server.Http;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal class InterceptorManager : IInterceptorManager
	{
		private readonly ILogger _logger;
		private readonly IOnPremiseRequestInterceptor _requestInceptor;
		private readonly IOnPremiseResponseInterceptor _responseInterceptor;

		public InterceptorManager(ILogger logger, IOnPremiseRequestInterceptor requestInceptor = null, IOnPremiseResponseInterceptor responseInterceptor = null)
		{
			_logger = logger;
			_requestInceptor = requestInceptor;
			_responseInterceptor = responseInterceptor;
		}

		public IOnPremiseConnectorRequest HandleRequest(IOnPremiseConnectorRequest request, HttpRequestMessage message, out HttpResponseMessage immediateResponse)
		{
			immediateResponse = null;
			if (_requestInceptor == null)
			{
				return request;
			}

			_logger?.Verbose("Handling request. request-id={RequestId}", request.RequestId);

			IPAddress ipAddress = null;
			try
			{
				ipAddress = message.GetRemoteIpAddress();
			}
			catch (Exception ex)
			{
				_logger?.Warning(ex, "Could not fetch remote IP address for request {RequestId}", request.RequestId);
			}

			try
			{
				var replacedRequest = new InterceptedRequest(request) { ClientIpAddress = ipAddress };
				immediateResponse = _requestInceptor.OnRequestReceived(replacedRequest);

				return replacedRequest;
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error while executing the request interceptor. type-name={InterceptorType}, request-id={RequestId}", _requestInceptor?.GetType().Name, request.RequestId);
			}

			return request;
		}

		public HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response)
		{
			if (_responseInterceptor == null)
			{
				return null;
			}

			_logger?.Verbose("Handling response. request-id={RequestId}", request.RequestId);

			try
			{
				if (response == null)
				{
					return _responseInterceptor.OnResponseReceived(new InterceptedRequest(request));
				}

				return _responseInterceptor.OnResponseReceived(new InterceptedRequest(request), new InterceptedResponse(response));
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error while executing the response interceptor. type-name={InterceptorType}, request-id={RequestId}", _requestInceptor?.GetType().Name, request.RequestId);
				return null;
			}
		}
	}
}