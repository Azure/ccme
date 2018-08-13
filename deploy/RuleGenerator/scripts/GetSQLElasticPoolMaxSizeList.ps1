#ToDo: Get SQL capability programmically (existing PPowerShall cmdlet `Get-AzureRmSqlCapability` hide most of details)
#Workaround: capture the response content to file directly, then pass the file as input of this script

Param
(
  [string]
  $inputFile
)

$totalMBytes = @{}
$totalMBytes['Megabytes'] = 1
$totalMBytes['Gigabytes'] = 1024
$totalMBytes['Terabytes'] = 1024 * 1024

$root = get-content $inputFile | ConvertFrom-Json
$whitelist = $root.supportedServerVersions |
	foreach { $_.supportedElasticPoolEditions } |
	foreach {
		$name = $_.name
		$_.supportedElasticPoolDtus |
		foreach {
			$dtu = $_.limit
			$_.supportedMaxSizes |
				foreach {
					[pscustomobject]@{
						edition = $name
						dtu = $dtu
						storageMB = $_.limit * $totalMBytes[$_.unit]
					}
				}
		 	}
		}

$doc = @{
    '$schema' = "https://aka.ms/CCME/schemas/2018-03-01/assessmentRuleListEvaluator.json"
    contentVersion = "1.0.0.0"
    whitelist = $whitelist
    whitelistNoHitMessage = "SQL elastic pool max size {storageMB}MB for SKU {edition} {dtu} is not supported in Azure China"
}

$doc | ConvertTo-Json -Depth 100 | Out-File "ChinaSQLElasticPoolMaxSize.json"