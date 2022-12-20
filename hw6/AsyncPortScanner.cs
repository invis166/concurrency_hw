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
			foreach(var ipAddr in ipAddrs)
			{
				ScanIpAddr(ipAddr, ports);
			}

			return Task.CompletedTask;
		}

		public void ScanIpAddr(IPAddress ipAddr, int[] ports)
		{
			var ipStatus = Task.Run(() => PingAddrAsync(ipAddr)).Result;
			if (ipStatus != IPStatus.Success)
			{
				return;
			}
			Task.WhenAll(ports.Select(port => CheckPortAsync(ipAddr, port))).Wait();
		}

		private IPStatus PingAddrAsync(IPAddress ipAddr, int timeout = 3000) 
		{
			using var ping = new Ping();

			Console.WriteLine($"Pinging {ipAddr}");
			var status = Task.Run(() => ping.SendPingAsync(ipAddr, timeout)).Result.Status;
			Console.WriteLine($"Pinged {ipAddr}: {status}");
			
			return status;
		}

		private static Task CheckPortAsync(IPAddress ipAddr, int port, int timeout = 3000)
		{
			using var tcpClient = new TcpClient();
			
			Console.WriteLine($"Checking {ipAddr}:{port}");
			var task = tcpClient.ConnectAsync(ipAddr, port, timeout).ContinueWith(t =>
				{
					Console.WriteLine($"Checked {ipAddr}:{port} - {t.Result}");
				}, TaskContinuationOptions.OnlyOnRanToCompletion);

			return task;
		}
	}
}