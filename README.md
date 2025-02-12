# alf - Account Lockout Finder
## About
This application was created to ease the problem of users getting locked out by persistent logon sessions on Windows hosts. The application queries the event logs 
on a target domain controller and parses the results based on the supplied username. Event log query is performed via powershell call to the Get-Event commandlet. 
This was chosen becuase C# does not have an efficient way to query a large quantity of event logs. The Get-Event powershell commandlet is already optimized to pull 
relevant logs in seconds rather than minutes. A C# console app was chosen over a powershell script becuase powershell is a messy solution to parse the resultant logs.

## How to Use
alf must be run from an account that has permission to access event logs on the target host.  
`alf -u <username> -d <hosts>`  

For example...  
`alf -u jsmith -d dc1,dc2`  

- **username**: the user account experiencing lockouts  
- **hosts**: the target host containing event logs for Security Event ID 4740; can be a single host or a comma separated list

## Compatibility
alf has only been tested in Windows 10 x86-64 environments with PowerShell version 5.0+.

## Build Procedure
### Prerequisites
- Visual Studio 2022 installed
- .Net Core 9 SDK installed

### Procedure
1. Open alf.sln in Visual Studio.
2. From the Main Menu Bar select Tools > NuGet Package Manager > Package Manager Console
3. Run `dotnet publish -r win-x64`

## Install Procedure
### Dependencies
The project is configured as a single file application and therefore has no dependencies other than PowerShell v5.0+ being installed.

### Procedure
1. Navigate to alf\bin\Release\net9.0\publish
2. Copy alf.exe
3. Paste to desired destination; could be system path or non system path folder
4. OPTIONAL: If alf.exe is maintained in non system path location, add this folder to system path to run from anywhere on the filesystem.
