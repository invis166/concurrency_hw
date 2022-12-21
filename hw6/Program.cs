using System.Collections.Generic;
using System.Net;

namespace TPL
{
    public static class Program
	{
		public static void Main(string[] args)
		{
			// $ dig +short google.com
			// > 142.250.74.174
			var ipAddrs = new[] {IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.2")};
 			var ports = new[] {21, 25, 43, 80, 443, 5432, 3389, 8080, 8888};

			new AsyncScanner().Scan(ipAddrs, ports).Wait();
		}
	}
}
