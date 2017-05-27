######################################################################
# 
# 
# Automatically changes scriptpath and scriptargs in powershellstarter.exe.config
# and adds powershellstarter.exe as a service.
# 
# 
# 
#####################################################################
Param(

    [parameter(mandatory=$true)]
    [string]$ScriptPath,
    [parameter(mandatory=$true)]
    [string]$ScriptParameters,
    [parameter(mandatory=$true)]
    [string]$ServiceName,
    [parameter(mandatory=$true)]
    [ValidateSet("Automatic", "Manual", "Boot", "Disabled","System")]
    [string]$StartupType,
    [System.Management.Automation.PSCredential]$Credential,
    [string]$DependsOn,
    [string]$ServiceDisplayName,
    [switch]$Confirm,
    [switch]$StartService
)

Import-Module Microsoft.PowerShell.Management

<#
.Synopsis
   Gets a app.conf and sets the value of a specified key
.DESCRIPTION
   Gets a app.conf and sets the value of a specified key
.EXAMPLE
   Set-AppConfValue -Path $AppConfigFile -Key "ScriptParameters" -Value $ScriptParameters
#>
function Set-AppConfValue
{
    [CmdletBinding()]
    Param
    (
        # Path to app.conf
        [Parameter(Mandatory=$true)]
        $Path,
        # Key
        [Parameter(Mandatory=$true)]
        $Key,
        [Parameter(Mandatory=$true)]
        $Value

    )

    Begin
    {
        $AppConfig = New-Object XML

        if(Test-Path $Path){

           $AppConfig.Load($appConfigFile) 

        }else{
        
            Write-Error "file not found"
            break

        }

    }
    Process
    {
        
        ($AppConfig.configuration.appSettings.add | ? {$_.key -eq $Key}).value = $Value

    }
    End
    {

        $appConfig.Save($appConfigFile)

    }
}

# Get the directory of this script file
$CurrentDirectory = [IO.Path]::GetDirectoryName($MyInvocation.MyCommand.Path)

# Get the full path and file name of the App.config file in the same directory as this script
$AppConfigFile = [IO.Path]::Combine($currentDirectory, 'powershellstarter.exe.config')
$AppPath = [IO.Path]::Combine($currentDirectory, 'powershellstarter.exe')

Set-AppConfValue -Path $AppConfigFile -Key "ScriptPath" -Value $ScriptPath
Set-AppConfValue -Path $AppConfigFile -Key "ScriptParameters" -Value $ScriptParameters

if($ServiceDescription -eq $null){$ServiceDescription = $ServiceName}


New-Service -Name $ServiceName -BinaryPathName $AppPath -DisplayName $ServiceName -StartupType ([System.ServiceProcess.ServiceStartMode]$StartupType) -Credential $Credential -Confirm:$Confirm

if($StartService){

    Start-Service $ServiceName

}