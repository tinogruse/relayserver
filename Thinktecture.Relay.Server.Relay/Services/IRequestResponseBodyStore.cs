using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Relay.Services
{
	public interface IRequestResponseBodyStore
	{
		/// <summary>
		/// Stores a body stream to a temporary store.
		/// </summary>
		/// <param name="linkId">The id of the link for which this data is intended.</param>
		/// <param name="bodyId">The id for the data to be stored under.</param>
		/// <param name="data">The stream data to store.</param>
		/// <returns>A guid that identifies this body.</returns>
		Task StoreBodyAsync(Guid linkId, Guid bodyId, Stream data);

		/// <summary>
		/// Gets the body stream from the temporary store.
		/// </summary>
		/// <param name="linkId">The id of the link this data is intended for.</param>
		/// <param name="bodyId">The id of the stream in the backing store.</param>
		/// <returns>The data of the body stream.</returns>
		Task<Stream> GetBodyAsync(Guid linkId, Guid bodyId);
	}

	internal class InMemoryRequestResponseBodyStore : IRequestResponseBodyStore
	{
		private readonly IDictionary<Guid, Stream> _data = new ConcurrentDictionary<Guid, Stream>();

		public async Task StoreBodyAsync(Guid linkId, Guid bodyId, Stream data)
		{
			// this currently ignores the linkId

			// Use Memorystream to store the data in
			var ms = new MemoryStream();

			await data.CopyToAsync(ms).ConfigureAwait(false);
			ms.Position = 0;

			_data.Add(bodyId, ms);
		}

		public Task<Stream> GetBodyAsync(Guid linkId, Guid bodyId)
		{
			// this currently ignores the linkId
			if (_data.TryGetValue(bodyId, out var data))
			{
				_data.Remove(bodyId);
				return Task.FromResult(data);
			}

			return Task.FromResult((Stream) null);
		}
	}
}
