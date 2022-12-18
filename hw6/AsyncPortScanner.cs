using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TPL
{
	public class AsyncScanner : IPScanner
	{
		public async Task Scan(IPAddress[] ipAddrs, int[] ports)
		{
			foreach(var ipAddr in ipAddrs)
			{
				if(await PingAddrAsync(ipAddr) != IPStatus.Success)
					continue;

				await Task.WhenAll(ports.Select(port => CheckPortAsync(ipAddr, port)));
			}
		}

		private async Task<IPStatus> PingAddrAsync(IPAddress ipAddr, int timeout = 3000) 
		{
			using var ping = new Ping();

			Console.WriteLine($"Pinging {ipAddr}");
			var status = (await ping.SendPingAsync(ipAddr, timeout)).Status;
			Console.WriteLine($"Pinged {ipAddr}: {status}");
			
			return status;
		}

		private static async Task CheckPortAsync(IPAddress ipAddr, int port, int timeout = 3000)
		{
			using var tcpClient = new TcpClient();
			
			Console.WriteLine($"Checking {ipAddr}:{port}");
			var portStatus = await tcpClient.ConnectAsync(ipAddr, port, timeout); 
			Console.WriteLine($"Checked {ipAddr}:{port} - {portStatus}");
		}
	}
}