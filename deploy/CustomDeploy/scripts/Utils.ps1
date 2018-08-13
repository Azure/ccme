function RandomPassword
{
    param
    (
        [Parameter(Mandatory = $true)]
        [int] $length
    )

    $bytes = ((0..255) | Sort-Object {Get-Random})[0..$length]
    return [System.Convert]::ToBase64String($bytes)
}

function CreateAADApplication
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $aadApplicationDisplayName
    )

    $aadSessonInfo = Get-AzureADCurrentSessionInfo
    $user = Get-AzureADUser -ObjectId $aadSessonInfo.Account.Id

    # Build application property - application secret
    $now = Get-Date
    $aadApplicationPassword = RandomPassword -length 16

    $passwordCredential = New-Object Microsoft.Open.AzureAD.Model.PasswordCredential
    $passwordCredential.StartDate = $now
    $passwordCredential.EndDate = $now.AddYears(2)
    $passwordCredential.KeyId = New-Guid
    $passwordCredential.Value = $aadApplicationPassword

    # Build application property - required resource access
    $requiredResourceAccess = New-Object Microsoft.Open.AzureAD.Model.RequiredResourceAccess
    $requiredResourceAccess.ResourceAccess = New-Object Microsoft.Open.AzureAD.Model.ResourceAccess `
        -ArgumentList "41094075-9dad-400e-a0bd-54e686782033", "Scope"
    $requiredResourceAccess.ResourceAppId = "797f4846-ba00-4fd7-ba43-dac1f8f63013"

    $aadApplication = New-AzureADApplication `
        -AvailableToOtherTenants $true `
        -DisplayName $aadApplicationDisplayName `
        -IdentifierUris "http://$(New-Guid).$($aadSessonInfo.Tenant.Domain)" `
        -PasswordCredentials $passwordCredential `
        -RequiredResourceAccess $requiredResourceAccess

    # Assign current user as owner of the new application
    $owner = Get-AzureADApplicationOwner -ObjectId $aadApplication.ObjectId
    if (-not $owner)
    {
        Add-AzureADApplicationOwner `
            -ObjectId $aadApplication.ObjectId `
            -RefObjectId $user.ObjectId
    }

    return @{
        objectId = $aadApplication.ObjectId
        applicationId = $aadApplication.AppId
        applicationSecret = $aadApplicationPassword
    }
}

function CreateTemplateDeployment
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $location,

        [Parameter(Mandatory = $true)]
        [string] $resourceGroupName,

        [Parameter(Mandatory = $true)]
        [string] $templatePath,

        [Parameter(Mandatory = $true)]
        [PSObject] $parameters
    )

    New-AzureRmResourceGroup -Location $location -Name $resourceGroupName

    $deployment = New-AzureRmResourceGroupDeployment `
        -ResourceGroupName $resourceGroupName `
        -TemplateFile $templatePath `
        -TemplateParameterObject $parameters `
        -Verbose

    if($deployment.ProvisioningState -ne "Succeeded")
    {
        throw $deployment;
    }

    return $deployment
}

function CheckResourceGroup
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $resourceGroupName
    )

    Get-AzureRmResourceGroup `
        -Name $resourceGroupName `
        -ErrorAction SilentlyContinue `
        -ErrorVariable notPresent | Out-Null

    if (-not $notPresent)
    {
        throw "Resource group $($resourceGroupName) already exists"
    }
}

function CheckWebsiteUrl
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $websiteUrl
    )

    Resolve-DnsName `
        -Name $websiteUrl `
        -ErrorAction SilentlyContinue `
        -ErrorVariable notPresent | Out-Null

    if (-not $notPresent)
    {
        throw "Web site $($websiteUrl) already exists"
    }
}

function CreateDiagnosticSAS
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $resourceGroupName,

        [Parameter(Mandatory = $true)]
        [string] $storageAccountName,

        [Parameter(Mandatory = $true)]
        [string] $containerName
    )

    $now = Get-Date
    $start = $now.ToUniversalTime().ToString('o')
    $expiry = $now.AddYears(100).ToUniversalTime().ToString('o')
    $contentToSign = "rwdl`n$($start)`n$($expiry)`n/blob/$($storageAccountName)/$($containerName)`n`n`n`n2015-04-05`n`n`n`n`n"

    $storageAccountKey = Get-AzureRmStorageAccountKey `
        -Name $storageAccountName `
        -ResourceGroupName $resourceGroupName

    $hmac = New-Object System.Security.Cryptography.HMACSHA256
    $hmac.Key = [Convert]::FromBase64String($storageAccountKey[0].Value)
    $sign = $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($contentToSign))

    $storageAccount = Get-AzureRmStorageAccount `
        -Name $storageAccountName `
        -ResourceGroupName $resourceGroupName

    return "$($storageAccount.PrimaryEndpoints.Blob)$($containerName)?sv=2015-04-05&sr=c&sig=$([System.Web.HttpUtility]::UrlEncode([Convert]::ToBase64String($sign)))&st=$([System.Web.HttpUtility]::UrlEncode($start))&se=$([System.Web.HttpUtility]::UrlEncode($expiry))&sp=rwdl"
}

function EnableAppServiceDiagnostic
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $resourceGroupName,

        [Parameter(Mandatory = $true)]
        [string] $webAppName,

        [Parameter(Mandatory = $true)]
        [string] $sasUrl
    )

    $propertyObject = @{
        'applicationLogs' = @{
            'azureBlobStorage' = @{
                'level' = 'Verbose'
                'sasUrl' = $sasUrl
                'retentionInDays' = 14
            }
        }
    }

    Set-AzureRmResource `
        -resourceGroupName $resourceGroupName `
        -ResourceType "Microsoft.Web/sites/config" `
        -ResourceName "$($webAppName)/logs" `
        -PropertyObject $propertyObject `
        -ApiVersion 2016-08-01 `
        -Force | Out-Null
}