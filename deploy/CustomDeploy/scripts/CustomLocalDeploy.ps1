param
(
    [Alias('n')]
    [Parameter(Mandatory = $true)]
    [string] $displayName,

    [Parameter(Mandatory = $false)]
    [string] $aadApplicationDisplayName,

    [Parameter(Mandatory = $false)]
    [string] $resourceGroupName,

    [Alias('l')]
    [Parameter(Mandatory = $true)]
    [string] $location
)

function ValidateParameters
{
    $azureContext = Get-AzureRmContext
    if (-not $azureContext.Account)
    {
        throw "Please login Azure by Login-AzureRmAccount"
    }

    Get-AzureADCurrentSessionInfo `
        -ErrorAction SilentlyContinue `
        -ErrorVariable notPresent | Out-Null
    if ($notPresent)
    {
        throw "Please login Graph API by Connect-AzureAD"
    }

    if (-not $script:aadApplicationDisplayName)
    {
        $script:aadApplicationDisplayName = $script:displayName
    }

    if (-not $script:resourceGroupName)
    {
        $script:resourceGroupName = $script:displayName -replace "[^0-9a-zA-Z]"
    }

    Write-Host "Deployment parameters:"
    Write-Host "AAD application display name = $($script:aadApplicationDisplayName)"
    Write-Host "Resource group name          = $($script:resourceGroupName)"
    Write-Host "Target location              = $($script:location)"
    Write-Host "Target subscription          = $($azureContext.Subscription.Name)"
    $confirm = Read-Host "`nIs it OK to continue with above parameter? (yes/no)"
    if (($confirm -cne "yes") -and ($confirm -cne "y"))
    {
        exit
    }

    CheckResourceGroup -resourceGroupName $script:resourceGroupName
}

function Deploy
{
    Write-Host "Creating AAD application..."
    $applicationCredentials = CreateAADApplication `
        -aadApplicationDisplayName $script:aadApplicationDisplayName
    Write-Host "Done`n"

    Write-Host "Creating template deployment..."
    $parameters = @{
        sqlServerAdminName = "ccmesqladmin"
        sqlServerAdminPassword = RandomPassword -length 16
    }

    $deployment = CreateTemplateDeployment `
        -location $script:location `
        -resourceGroupName $script:resourceGroupName `
        -templatePath "$($script:root)\..\arm_templates\localdeploy.json" `
        -parameters $parameters
    Write-Host "Done`n"

    Write-Host "Updating reply URL of AAD application..."
    Set-AzureADApplication `
        -ObjectId $applicationCredentials.objectId `
        -ReplyUrls "http://localhost:50080/"
    Write-Host "Done`n"

    $config = @(
        @{
            key = "CloudEnvironment"
            value = (Get-AzureRmContext).Environment.Name
        },
        @{
            key = "ApplicationId"
            value = $applicationCredentials.applicationId
        },
        @{
            key = "ApplicationSecret"
            value = $applicationCredentials.applicationSecret
        },
        @{
            key = "CCMEDB"
            value = $deployment.Outputs.sqlDatabaseConnectionString.Value
        },
        @{
            key = "StorageAccountConnectionString"
            value = $deployment.Outputs.storageAccountConnectionString.Value
        }
    )

    $outputPath = "$($script:root)\$($script:displayName).local.config"
    $config | ConvertTo-Json | Out-File $outputPath
    Write-Host "Deployment completed. Please copy $($outputPath) to root folder of the solution and rename it as local.config"
}

# import utils functions
$root = Split-Path $MyInvocation.MyCommand.Path
. "$($root)\Utils.ps1"

ValidateParameters

Deploy