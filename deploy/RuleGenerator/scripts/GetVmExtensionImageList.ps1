Param
(
    [string]
    $location
)

$imageTypes = Get-AzureRmVMImagePublisher -Location $location | 
    foreach { Get-AzureRmVMExtensionImageType -Location $location -PublisherName $_.PublisherName }

$whitelist = $imageTypes | foreach {
    [pscustomobject]@{
        publisher = $_.PublisherName
        type = $_.Type
        version = @(Get-AzureRmVMExtensionImage -Location $location -PublisherName $_.PublisherName -Type $_.Type | foreach {"$($_.Version)"}) + "latest"
    }
}

$doc = @{
    '$schema' = "https://aka.ms/CCME/schemas/2018-03-01/assessmentRuleListEvaluator.json"
    contentVersion = "1.0.0.0"
    whitelist = $whitelist
    whitelistNoHitMessage = "VM extension {type} (version {version}) from {publisher} is not supported in Azure China"
}

$doc | ConvertTo-Json -Depth 100 | Out-File "$($location)VirtualMachineExtensionImage.json"