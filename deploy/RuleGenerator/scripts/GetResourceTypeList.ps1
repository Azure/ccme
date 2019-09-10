Param
(
    [string]
    $location
)

$whitelist = Get-AzureRmResourceProvider -Location $location -ListAvailable | foreach {
	$providerNamespace = $_.ProviderNamespace
	$_.ResourceTypes | foreach {
		[pscustomobject]@{
			resourceType = "$($ProviderNamespace)/$($_.ResourceTypeName)" } } }

$doc = @{
    '$schema' = "https://aka.ms/CCME/schemas/2018-03-01/assessmentRuleListEvaluator.json"
    contentVersion = "1.0.0.0"
    whitelist = $whitelist
    whitelistNoHitMessage = "Resource type {resourceType} is not supported in Azure China"
}

$doc | ConvertTo-Json -Depth 100 | Out-File "$($location)ResourceType.json"