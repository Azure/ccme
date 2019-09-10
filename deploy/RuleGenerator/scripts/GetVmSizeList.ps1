Param
(
    [string]
    $location
)

$skus = Get-AzureRmVMSize -Location $location

$whitelist = $skus | foreach {
    [pscustomobject]@{
        vmSize = $_.Name
    }
}

$doc = @{
    '$schema' = "https://aka.ms/CCME/schemas/2018-03-01/assessmentRuleListEvaluator.json"
    contentVersion = "1.0.0.0"
    whitelist = $whitelist
    whitelistNoHitMessage = "VM size {vmSize} is not supported in Azure China"
}

$doc | ConvertTo-Json -Depth 100 | Out-File "$($location)VirtualMachineSize.json"