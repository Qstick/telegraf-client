using System;
using System.Collections.Generic;

namespace Telegraf {

	public static class Metrics {
		static ISender _statsD = new NullSender();
		static TextUDP _textUdp;
		static readonly SamplerFunc Sampler = SamplerDefault.ShouldSend;
		static bool _configured;

		/// <summary>
		/// Configures the Metric class with a configuration. Call this once at application startup (Main(), Global.asax, etc).
		/// </summary>
		/// <param name="config">Configuration settings.</param>
		public static void Configure(MetricsConfig config) {
			CheckValidity(config);

			_textUdp = new TextUDP(
				config.ServerName, 
				config.ServerPort,
				config.MaxUDPPacketSize);

			_statsD = new SyncSender(_textUdp, config.Tags);
			_configured = true;
		}

		public static AsyncSender ConfigureAsync(MetricsConfig config) {
			CheckValidity(config);

			_textUdp = new TextUDP(
				config.ServerName,
				config.ServerPort,
				config.MaxUDPPacketSize);

			var asyncSender = new AsyncSender(
				_textUdp, 
				config.Tags, 
				config.AsyncMaxNumberOfPointsInQueue,
				config.MaxUDPPacketSize);
			_statsD = asyncSender;
			_configured = true;

			return asyncSender;
		}

		private static void CheckValidity(MetricsConfig config) {
			if (_configured) {
				throw new InvalidOperationException("Metrics could be configured only once");
			}
			if (config == null) {
				throw new ArgumentNullException("config");
			}

			if (string.IsNullOrEmpty(config.ServerName)) {
				throw new ArgumentException("Statsd server name wasn't provided");
			}
		}


		/// <summary>
		/// Record a given value as single 'value' property.
		/// </summary>
		/// <param name="measurement">Name of the metric.</param>
		/// <param name="value">Value to set as 'value' property.</param>
		/// <param name="tags">Dictionary of key/values to store.</param>
		/// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
		public static void RecordValue(string measurement, object value, Dictionary<string,string> tags = null, int sampleRate = 1) {
			if (!Sampler(sampleRate)) {
				return;
			}

			if (tags == null) {
				tags = new Dictionary<string, string>();
			}

			// Ensure the entry is always unique
			tags.Add("__guid", Guid.NewGuid().ToString());

			var values = new Dictionary<string, object> { { "value", value } };
			var point = new InfluxPoint(measurement, values, tags);
			_statsD.Send(point);
		}

		/// <summary>
		/// Record the given values
		/// </summary>
		/// <param name="measurement">Name of the metric.</param>
		/// <param name="values">Dictionary of key/values to set.</param>
		/// <param name="tags">Dictionary of key/values to store with entry.</param>
		/// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
		public static void Record(string measurement, Dictionary<string,object> values, Dictionary<string, string> tags = null, int sampleRate = 1) {
			if (!Sampler(sampleRate))
			{
				return;
			}

			if (tags == null) {
				tags = new Dictionary<string, string>();
			}

			// Ensure the entry is always unique
			tags.Add("__guid", Guid.NewGuid().ToString());

			var point = new InfluxPoint(measurement, values, tags);
			_statsD.Send(point);
		}

		/// <summary>
		/// Send a counter value.
		/// </summary>
		/// <param name="measurement">Name of the metric.</param>
		/// <param name="tags">Dictionary of key/values to store with entry.</param>
		/// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
		/// <param name="count">The integer value to store as 'count'.</param>
		public static void RecordCount(string measurement, long count = 1, Dictionary<string,string> tags = null, int sampleRate = 1) {
			if (!Sampler(sampleRate))
			{
				return;
			}

			if (tags == null) {
				tags = new Dictionary<string, string>();
			}

			// Ensure the entry is always unique
			tags.Add("__guid", Guid.NewGuid().ToString());

			var values = new Dictionary<string, object> { { "count", count } };
			var point = new InfluxPoint(measurement, values, tags);
			_statsD.Send(point);
		}
	}

}