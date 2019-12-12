using System.Text.Json;

namespace Thinktecture.Relay.Server.Relay.Services
{
	public interface IJsonSerializer
	{
		string Serialize(object value);
		T Deserialize<T>(string data);
	}

	public class CustomJsonSerializer : IJsonSerializer
	{
		private static readonly JsonSerializerOptions Options = new JsonSerializerOptions()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};

		public string Serialize(object value)
		{
			return JsonSerializer.Serialize(value, Options);
		}

		public T Deserialize<T>(string json)
		{
			return JsonSerializer.Deserialize<T>(json, Options);
		}
	}
}
