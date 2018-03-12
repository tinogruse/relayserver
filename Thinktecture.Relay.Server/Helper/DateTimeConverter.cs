using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thinktecture.Relay.Server.Helper
{
	internal class DateTimeConverter : JsonConverter
	{
		// We don't like the default serialization of DateTime values.
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(DateTime) || Nullable.GetUnderlyingType(objectType) == typeof(DateTime);
		}

		public override bool CanRead => false;
		public override bool CanWrite => true;

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var dateTime = value as DateTime?;
			if (dateTime.HasValue)
			{
				if (dateTime.Value.Kind == DateTimeKind.Local)
					dateTime = dateTime.Value.ToUniversalTime();

				writer.WriteValue(dateTime.Value.ToString("s") + "Z");
			}
			else
			{
				writer.WriteNull();
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
