using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegraf;

namespace TesterTool {
	class Program
	{
		static void Main(string[] args) {
			var config = new MetricsConfig();

			var sender = Metrics.ConfigureAsync(config);
			Task.Factory.StartNew(() => sender.KeepDelivering(CancellationToken.None));
		

			while (true) {
				Metrics.RecordCount("test", 1, new Dictionary<string, string>() {
					{ "tag","bingo"}
				});
				Metrics.RecordValue("test", 1, new Dictionary<string, string>() {
					{"tag","bongo" }
				});

				Metrics.Record("test", new Dictionary<string, object>() {
					{"value1", 2 },
					{"value2", 3 },
					{"tag", "bingo2" }
				});

				Console.WriteLine("Write to continue");
				Console.ReadLine();
			}
		}
	}
}
