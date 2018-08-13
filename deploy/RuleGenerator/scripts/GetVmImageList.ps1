Param
(
    [string]
    $location
)

$skus = Get-AzureRmVmImagePublisher -Location $location |
    foreach { Get-AzureRmVMImageOffer -Location $location -PublisherName $_.PublisherName } |
    foreach { Get-AzureRmVMImageSku -Location $location -PublisherName $_.PublisherName -Offer $_.Offer }

$whitelist = $skus | foreach {
    [pscustomobject]@{
        publisher = $_.PublisherName
        offer = $_.Offer
        sku = $_.Skus
        version = @(Get-AzureRmVMImage -Location $location -PublisherName $_.PublisherName -Offer $_.Offer -Skus $_.Skus | foreach {"$($_.Version)"}) + "latest"
    }
}

$doc = @{
    '$schema' = "https://aka.ms/CCME/schemas/2018-03-01/assessmentRuleListEvaluator.json"
    contentVersion = "1.0.0.0"
    whitelist = $whitelist
    whitelistNoHitMessage = "VM image {offer}.{sku} (version {version}) from {publisher} is not supported in Azure China"
}

$doc | ConvertTo-Json -Depth 100 | Out-File "$($location)VirtualMachineImage.json"