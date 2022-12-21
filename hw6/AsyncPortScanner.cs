using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TPL
{
	public class AsyncScanner : IPScanner
	{
		public Task Scan(IPAddress[] ipAddrs, int[] ports)
		{
			return Task.WhenAll(ipAddrs.Select(ipAddr => ScanIpAddr(ipAddr, ports)));
		}

		public Task ScanIpAddr(IPAddress ipAddr, int[] ports, int timeout = 3000)
		{
			var ping = new Ping();

			Console.WriteLine($"Pinging {ipAddr}");

			var task = ping.SendPingAsync(ipAddr, timeout)
			.ContinueWith(t => 
			{
				Console.WriteLine($"Pinged {ipAddr}: {t.Result.Status}");
				return t.Result.Status;
			}, TaskContinuationOptions.OnlyOnRanToCompletion)
			.ContinueWith(t => {
				ping.Dispose();
				if (t.Result == IPStatus.Success)
				{
					Task.WhenAll(ports.Select(port => CheckPortAsync(ipAddr, port))).Wait();
				}

			}, TaskContinuationOptions.OnlyOnRanToCompletion);

			return task;
		}

		private static Task CheckPortAsync(IPAddress ipAddr, int port, int timeout = 3000)
		{
			var tcpClient = new TcpClient();
			
			Console.WriteLine($"Checking {ipAddr}:{port}");
			var task = tcpClient.ConnectAsync(ipAddr, port, timeout).ContinueWith(t =>
				{
					Console.WriteLine($"Checked {ipAddr}:{port} - {t.Result}");
				}, TaskContinuationOptions.OnlyOnRanToCompletion
				).ContinueWith(t => {
					tcpClient.Dispose();
				});

			return task;
		}
	}
}