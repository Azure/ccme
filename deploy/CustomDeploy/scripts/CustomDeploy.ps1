param
(
    [Alias('n')]
    [Parameter(Mandatory = $true)]
    [string] $displayName,

    [Parameter(Mandatory = $false)]
    [string] $aadApplicationDisplayName,

    [Parameter(Mandatory = $false)]
    [string] $websiteName,

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

    if (-not $script:websiteName)
    {
        $script:websiteName = $script:displayName -replace "[^0-9a-zA-Z]"
    }

    if (-not $script:resourceGroupName)
    {
        $script:resourceGroupName = $script:displayName -replace "[^0-9a-zA-Z]"
    }

    switch ($azureContext.Environment)
    {
        "AzureCloud" { $websiteUrl = "$($script:websiteName).azurewebsites.net" }
        "AzureChinaCloud" { $websiteUrl = "$($script:websiteName).chinacloudsites.cn" }
        default { throw "Not supported cloud environment" }
    }

    Write-Host "Deployment parameters:"
    Write-Host "AAD application display name = $($script:aadApplicationDisplayName)"
    Write-Host "Website URL                  = $($websiteUrl)"
    Write-Host "Resource group name          = $($script:resourceGroupName)"
    Write-Host "Target location              = $($script:location)"
    Write-Host "Target subscription          = $($azureContext.Subscription.Name)"
    $confirm = Read-Host "`nIs it OK to continue with above parameter? (yes/no)"
    if (($confirm -cne "yes") -and ($confirm -cne "y"))
    {
        exit
    }

    CheckResourceGroup -resourceGroupName $script:resourceGroupName

    CheckWebsiteUrl -websiteUrl $websiteUrl
}

function Deploy
{
    Write-Host "Creating AAD application..."
    $applicationCredentials = CreateAADApplication `
        -aadApplicationDisplayName $script:aadApplicationDisplayName
    Write-Host "Done`n"

    Write-Host "Creating template deployment..."
    $parameters = @{
        aadApplicationId = $applicationCredentials.applicationId
        aadApplicationSecret = $applicationCredentials.applicationSecret
        sqlServerAdminName = "ccmesqladmin"
        sqlServerAdminPassword = RandomPassword -length 16
        websiteName = $script:websiteName
    }

    $deployment = CreateTemplateDeployment `
        -location $script:location `
        -resourceGroupName $script:resourceGroupName `
        -templatePath "$($script:root)\..\arm_templates\azuredeploy.json" `
        -parameters $parameters
    Write-Host "Done`n"

    Write-Host "Enabling application diagnostic for web application..."
    $storageAccount = Get-AzureRmStorageAccount `
        -Name $deployment.Outputs.storageAccountName.Value `
        -ResourceGroupName $script:resourceGroupName

    New-AzureStorageContainer `
        -Name log `
        -Context $storageAccount.Context | Out-Null

    $sasUrl = CreateDiagnosticSAS `
        -resourceGroupName $script:resourceGroupName `
        -storageAccountName $deployment.Outputs.storageAccountName.Value `
        -containerName log

    EnableAppServiceDiagnostic `
        -resourceGroupName $script:resourceGroupName `
        -webAppName $deployment.Outputs.webAppName.Value `
        -sasUrl $sasUrl
    Write-Host "Done`n"

    Write-Host "Updating reply URL of AAD application..."
    Set-AzureADApplication `
        -ObjectId $applicationCredentials.objectId `
        -ReplyUrls $deployment.Outputs.webAppLink.Value
    Write-Host "Done`n"

    Write-Host "Deployment completed. Please store properties below in secure place:"
    Write-Host "Application ID             = $($applicationCredentials.applicationId)"
    Write-Host "Application Secret         = $($applicationCredentials.applicationSecret)"
    Write-Host "SQL server admin name      = $($parameters.sqlServerAdminName)"
    Write-Host "SQL server admin password  = $($parameters.sqlServerAdminPassword)"

    Start-Process $deployment.Outputs.webAppLink.Value
}

# import utils functions
$root = Split-Path $MyInvocation.MyCommand.Path
. "$($root)\Utils.ps1"

ValidateParameters

Deploy
