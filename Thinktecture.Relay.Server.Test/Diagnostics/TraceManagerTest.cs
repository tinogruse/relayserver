using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Diagnostics
{
	// ReSharper disable ConvertToConstant.Local
	// ReSharper disable JoinDeclarationAndInitializer
	[TestClass]
	public class TraceManagerTest
	{
		private class OnPremiseConnectorResponse : IOnPremiseConnectorResponse
		{
			public string RequestId { get; set; }
			public Guid OriginId { get; set; }
			public DateTime RequestStarted { get; set; }
			public DateTime RequestFinished { get; set; }
			public HttpStatusCode StatusCode { get; set; }
			public IReadOnlyDictionary<string, string> HttpHeaders { get; set; }
			public byte[] Body { get; set; }
			public Stream Stream { get; set; }
			public long ContentLength { get; set; }
		}

		[TestMethod]
		public void IsCurrentlyTraced_correctly_reports_tracing_enabled_state_from_repository()
		{
			var traceRepositoryMock = new Mock<ITraceRepository>();
			var sut = new TraceManager(traceRepositoryMock.Object, null, null, null, null);
			var linkId = Guid.NewGuid();
			var traceConfigurationId = Guid.NewGuid();
			Guid? result;

			traceRepositoryMock.Setup(t => t.GetCurrentTraceConfigurationId(linkId)).Returns(traceConfigurationId);

			result = sut.GetCurrentTraceConfigurationId(linkId);

			result.Should().Be(traceConfigurationId);
		}

		[TestMethod]
		public void IsCurrentlyTraced_correctly_reports_tracing_disabled_state_from_repository()
		{
			var traceRepositoryMock = new Mock<ITraceRepository>();
			var sut = new TraceManager(traceRepositoryMock.Object, null, null, null, null);
			var linkId = Guid.NewGuid();
			Guid? result;

			traceRepositoryMock.Setup(t => t.GetCurrentTraceConfigurationId(linkId)).Returns((Guid?)null);

			result = sut.GetCurrentTraceConfigurationId(linkId);

			result.Should().NotHaveValue();
		}

		[TestMethod]
		public void Trace_creates_tracefiles_directory()
		{
			var traceFileWriterMock = new Mock<ITraceFileWriter>();
			var loggerMock = new Mock<ILogger>();
			var traceConfigurationId = Guid.NewGuid();
			var clientRequest = new OnPremiseConnectorRequest
			{
				HttpHeaders = new Dictionary<string, string>
				{
					["Content-Type"] = "text/plain",
					["Content-Length"] = "700"
				},
				Body = new byte[] { 65, 66, 67 }
			};
			var onPremiseConnectorResponse = new OnPremiseConnectorResponse()
			{
				HttpHeaders = new Dictionary<string, string>
				{
					["Content-Type"] = "image/png",
					["Content-Length"] = "7500"
				},
				Body = new byte[] { 66, 67, 68 }
			};
			ITraceManager sut = new TraceManager(null, traceFileWriterMock.Object, null, CreateDummyConfiguration(), loggerMock.Object);

			Directory.CreateDirectory("tracefiles");
			Directory.Delete("tracefiles");

			traceFileWriterMock.Setup(t => t.WriteContentFileAsync(It.IsAny<string>(), clientRequest.Body));
			traceFileWriterMock.Setup(t => t.WriteContentFileAsync(It.IsAny<string>(), onPremiseConnectorResponse.Body));
			traceFileWriterMock.Setup(t => t.WriteHeaderFileAsync(It.IsAny<string>(), clientRequest.HttpHeaders));
			traceFileWriterMock.Setup(t => t.WriteHeaderFileAsync(It.IsAny<string>(), onPremiseConnectorResponse.HttpHeaders));

			sut.Trace(clientRequest, onPremiseConnectorResponse, traceConfigurationId);

			traceFileWriterMock.VerifyAll();
			Directory.Exists("tracefiles").Should().BeTrue();

			Directory.Delete("tracefiles");
		}

		[TestMethod]
		public void Trace_uses_correct_file_names_build_from_Trace_ID_and_DateTime_ticks()
		{
			var traceFileWriterMock = new Mock<ITraceFileWriter>();
			var loggerMock = new Mock<ILogger>();
			var traceConfigurationId = Guid.NewGuid();
			var clientRequest = new OnPremiseConnectorRequest
			{
				HttpHeaders = new Dictionary<string, string>
				{
					["Content-Type"] = "text/plain",
					["Content-Length"] = "700"
				},
				Body = new byte[] { 65, 66, 67 }
			};
			var onPremiseConnectorResponse = new OnPremiseConnectorResponse()
			{
				HttpHeaders = new Dictionary<string, string>
				{
					["Content-Type"] = "image/png",
					["Content-Length"] = "7500"
				},
				Body = new byte[] { 66, 67, 68 }
			};
			string clientRequestBodyFileName = null;
			string clientRequestHeadersFileName = null;
			string onPremiseTargetResponseBodyFileName = null;
			string onPremiseTargetResponseHeadersFileName = null;
			DateTime startTime;
			ITraceManager sut = new TraceManager(null, traceFileWriterMock.Object, null, CreateDummyConfiguration(), loggerMock.Object);

			startTime = DateTime.Now;

			traceFileWriterMock.Setup(t => t.WriteContentFileAsync(It.IsAny<string>(), clientRequest.Body)).Callback((string f, byte[] c) => clientRequestBodyFileName = f);
			traceFileWriterMock.Setup(t => t.WriteContentFileAsync(It.IsAny<string>(), onPremiseConnectorResponse.Body)).Callback((string f, byte[] c) => onPremiseTargetResponseBodyFileName = f);
			traceFileWriterMock.Setup(t => t.WriteHeaderFileAsync(It.IsAny<string>(), clientRequest.HttpHeaders)).Callback((string f, IReadOnlyDictionary<string, string> c) => clientRequestHeadersFileName = f);
			traceFileWriterMock.Setup(t => t.WriteHeaderFileAsync(It.IsAny<string>(), onPremiseConnectorResponse.HttpHeaders)).Callback((string f, IReadOnlyDictionary<string, string> c) => onPremiseTargetResponseHeadersFileName = f);

			sut.Trace(clientRequest, onPremiseConnectorResponse, traceConfigurationId);

			var ticks = new DateTime(Int64.Parse(clientRequestBodyFileName.Split('-').Skip(5).First().Split('.').First()));
			var expectedFileNamePrefix = Path.Combine("tracefiles", traceConfigurationId + "-" + ticks.Ticks);

			traceFileWriterMock.VerifyAll();
			ticks.Should().BeOnOrAfter(startTime).And.BeOnOrBefore(DateTime.Now);
			clientRequestBodyFileName.Should().Be(expectedFileNamePrefix + ".ct.content");
			onPremiseTargetResponseBodyFileName.Should().Be(expectedFileNamePrefix + ".optt.content");
			clientRequestHeadersFileName.Should().Be(expectedFileNamePrefix + ".ct.headers");
			onPremiseTargetResponseHeadersFileName.Should().Be(expectedFileNamePrefix + ".optt.headers");
		}

		[TestMethod]
		public void Trace_logs_exception()
		{
			var traceFileWriterMock = new Mock<ITraceFileWriter>();
			var loggerMock = new Mock<ILogger>();
			var traceConfigurationId = Guid.NewGuid();
			var clientRequest = new OnPremiseConnectorRequest
			{
				HttpHeaders = new Dictionary<string, string>
				{
					["Content-Type"] = "text/plain",
					["Content-Length"] = "700"
				},
				Body = new byte[] { 65, 66, 67 }
			};
			var onPremiseConnectorResponse = new OnPremiseConnectorResponse()
			{
				HttpHeaders = new Dictionary<string, string>
				{
					["Content-Type"] = "image/png",
					["Content-Length"] = "7500"
				},
				Body = new byte[] { 66, 67, 68 }
			};
			ITraceManager sut = new TraceManager(null, traceFileWriterMock.Object, null, CreateDummyConfiguration(), loggerMock.Object);

			var exception = new Exception();

			traceFileWriterMock.Setup(t => t.WriteContentFileAsync(It.IsAny<string>(), clientRequest.Body)).Throws(exception);
			loggerMock.Setup(l => l.Warning(exception, It.IsAny<string>()));

			sut.Trace(clientRequest, onPremiseConnectorResponse, traceConfigurationId);

			traceFileWriterMock.VerifyAll();
		}

		[TestMethod]
		public async Task GetTraceFilesAsync_returns_file_info_objects_for_all_trace_files_of_a_given_prefix()
		{
			var traceFileReaderMock = new Mock<TraceFileReader>() { CallBase = true };
			var sut = new TraceManager(null, null, traceFileReaderMock.Object, CreateDummyConfiguration(), null);
			const string filePrefix1 = "7975999f-54d9-4b21-a093-4502ea372723-635497418466831637";
			const string filePrefix2 = "7975999f-54d9-4b21-a093-4502ea372723-635497418466831700";
			IEnumerable<Trace> result;

			var clientHeaders = new Dictionary<string, string>()
			{
				["Content-Type"] = "text/plain",
				["Content-Length"] = "0"
			};

			var onPremiseTargetHeaders = new Dictionary<string, string>()
			{
				["Content-Type"] = "image/png",
				["Content-Length"] = "500"
			};

			Directory.CreateDirectory("tracefiles");

			var traceFileWriter = new TraceFileWriter();
			await traceFileWriter.WriteHeaderFileAsync("tracefiles/" + filePrefix1 + ".ct.headers", clientHeaders);
			await traceFileWriter.WriteHeaderFileAsync("tracefiles/" + filePrefix1 + ".optt.headers", onPremiseTargetHeaders);
			await traceFileWriter.WriteHeaderFileAsync("tracefiles/" + filePrefix2 + ".ct.headers", clientHeaders);
			await traceFileWriter.WriteHeaderFileAsync("tracefiles/" + filePrefix2 + ".optt.headers", onPremiseTargetHeaders);

			result = await sut.GetTracesAsync(Guid.Parse("7975999f-54d9-4b21-a093-4502ea372723"));

			Directory.Delete("tracefiles", true);

			result.Count().Should().Be(2);
		}

		[TestMethod]
		public async Task GetTraceFilesAsync_catches_errors_when_reading_a_file_and_logs_them()
		{
			var loggerMock = new Mock<ILogger>();
			var traceFileReaderMock = new Mock<TraceFileReader>() { CallBase = true };
			var sut = new TraceManager(null, null, traceFileReaderMock.Object, CreateDummyConfiguration(), loggerMock.Object);
			const string filePrefix1 = "7975999f-54d9-4b21-a093-4502ea372723-635497418466831637";
			const string filePrefix2 = "7975999f-54d9-4b21-a093-4502ea372723-635497418466831700";
			IEnumerable<Trace> result;

			var clientHeaders = new Dictionary<string, string>()
			{
				["Content-Type"] = "text/plain",
				["Content-Length"] = "0"
			};

			var onPremiseTargetHeaders = new Dictionary<string, string>()
			{
				["Content-Type"] = "image/png",
				["Content-Length"] = "500"
			};

			Directory.CreateDirectory("tracefiles");

			var traceFileWriter = new TraceFileWriter();
			await traceFileWriter.WriteHeaderFileAsync("tracefiles/" + filePrefix1 + ".ct.headers", clientHeaders);
			await traceFileWriter.WriteHeaderFileAsync("tracefiles/" + filePrefix1 + ".optt.headers", onPremiseTargetHeaders);
			await traceFileWriter.WriteHeaderFileAsync("tracefiles/" + filePrefix2 + ".crxxxxxxx.headers", clientHeaders);
			await traceFileWriter.WriteHeaderFileAsync("tracefiles/" + filePrefix2 + ".ltrxxxxxxx.headers", onPremiseTargetHeaders);

			loggerMock.Setup(l => l.Warning<string>(It.IsAny<FileNotFoundException>(), It.IsAny<string>(), It.IsAny<string>()));

			result = await sut.GetTracesAsync(Guid.Parse("7975999f-54d9-4b21-a093-4502ea372723"));

			Directory.Delete("tracefiles", true);

			loggerMock.Verify(m => m.Warning(It.IsAny<FileNotFoundException>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
			result.Count().Should().Be(1);
		}

		private IConfiguration CreateDummyConfiguration()
		{
			return new ConfigurationDummy()
			{
				TraceFileDirectory = "tracefiles",
			};
		}
	}
}
