using System;
using System.Collections.Concurrent;
using RabbitMQ.Client;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	public class RabbitMqMessageDispatcherHandler : IMessageDispatcher
	{
		private const string _EXCHANGE_NAME = "RelayServer";
		private const string _REQUEST_QUEUE_PREFIX = "Request";
		private const string _RESPONSE_QUEUE_PREFIX = "Response";
		private const string _ACKNOWLEDGE_QUEUE_PREFIX = "Acknowledge";

		private readonly ILogger _logger;
		private readonly IConnection _connection;
		private readonly IConfiguration _configuration;
		private readonly Guid _originId;
		private readonly ConcurrentDictionary<string, RabbitMqRequestChannelBase> _rabbitMqRequestChannels = new ConcurrentDictionary<string, RabbitMqRequestChannelBase>();
		private readonly ConcurrentDictionary<string, RabbitMqResponseChannelBase> _rabbitMqResponseChannels = new ConcurrentDictionary<string, RabbitMqResponseChannelBase>();
		private readonly ConcurrentDictionary<string, RabbitMqAcknowledgeChannel> _rabbitMqAcknowledgeChannels = new ConcurrentDictionary<string, RabbitMqAcknowledgeChannel>();

		public RabbitMqMessageDispatcherHandler(ILogger logger, IConnection connection, IConfiguration configuration, IPersistedSettings persistedSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			_originId = persistedSettings?.OriginId ?? throw new ArgumentNullException(nameof(persistedSettings));
		}

		public IObservable<IOnPremiseConnectorRequest> OnRequestReceived(Guid linkId, String connectionId, Boolean autoAck)
		{
			return EnsureRequestChannel(linkId.ToString()).OnReceived(autoAck);
		}

		public IObservable<IOnPremiseConnectorResponse> OnResponseReceived()
		{
			return EnsureResponseChannel(_originId.ToString()).OnReceived();
		}

		public IObservable<String> OnAcknowledgeReceived()
		{
			return EnsureAcknowledgeChannel(_originId.ToString()).OnReceived();
		}

		public void AcknowledgeRequest(String acknowledgeId)
		{
			if (_rabbitMqRequestChannels.TryGetValue(_originId.ToString(), out var rabbitMqChannel))
			{
				rabbitMqChannel.Acknowledge(acknowledgeId);
			}
		}

		public void DispatchRequest(Guid linkId, IOnPremiseConnectorRequest request)
		{
			EnsureRequestChannel(linkId.ToString()).Dispatch(request);
		}

		public void DispatchResponse(Guid originId, IOnPremiseConnectorResponse response)
		{
			EnsureResponseChannel(originId.ToString()).Dispatch(response);
		}

		public void DispatchAcknowledge(Guid originId, String acknowledgeId)
		{
			EnsureAcknowledgeChannel(originId.ToString()).Dispatch(acknowledgeId);
		}

		private RabbitMqRequestChannelBase EnsureRequestChannel(string channelId)
		{
			if (_rabbitMqRequestChannels.TryGetValue(channelId, out var rabbitMqChannel))
				return rabbitMqChannel;

			rabbitMqChannel = new RabbitMqRequestChannelBase(_logger, _connection, _configuration, _EXCHANGE_NAME, channelId, _REQUEST_QUEUE_PREFIX);
			_rabbitMqRequestChannels[channelId] = rabbitMqChannel;

			return rabbitMqChannel;
		}

		private RabbitMqResponseChannelBase EnsureResponseChannel(string channelId)
		{
			if (_rabbitMqResponseChannels.TryGetValue(channelId, out var rabbitMqChannel))
				return rabbitMqChannel;

			rabbitMqChannel = new RabbitMqResponseChannelBase(_logger, _connection, _configuration, _EXCHANGE_NAME, channelId, _RESPONSE_QUEUE_PREFIX);
			_rabbitMqResponseChannels[channelId] = rabbitMqChannel;

			return rabbitMqChannel;
		}

		private RabbitMqAcknowledgeChannel EnsureAcknowledgeChannel(string channelId)
		{
			if (_rabbitMqAcknowledgeChannels.TryGetValue(channelId, out var rabbitMqChannel))
				return rabbitMqChannel;

			rabbitMqChannel = new RabbitMqAcknowledgeChannel(_logger, _connection, _configuration, _EXCHANGE_NAME, channelId, _ACKNOWLEDGE_QUEUE_PREFIX);
			_rabbitMqAcknowledgeChannels[channelId] = rabbitMqChannel;

			return rabbitMqChannel;
		}

		public void Dispose()
		{
			foreach (var rabbitMqChannel in _rabbitMqRequestChannels.Values)
			{
				rabbitMqChannel.Dispose();
			}

			foreach (var rabbitMqChannel in _rabbitMqResponseChannels.Values)
			{
				rabbitMqChannel.Dispose();
			}

			foreach (var rabbitMqChannel in _rabbitMqAcknowledgeChannels.Values)
			{
				rabbitMqChannel.Dispose();
			}
		}
	}
}
