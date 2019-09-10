#ToDo: Get SQL capability programmically (existing PPowerShall cmdlet `Get-AzureRmSqlCapability` hide most of details)
#Workaround: capture the response content to file directly, then pass the file as input of this script

Param
(
  [string]
  $inputFile
)

$totalBytes = @{}
$totalBytes['Megabytes'] = 1024 * 1024
$totalBytes['Gigabytes'] = 1024 * 1024 * 1024
$totalBytes['Terabytes'] = 1024 * 1024 * 1024 * 1024

$root = get-content $inputFile | ConvertFrom-Json
$whitelist = $root.supportedServerVersions |
    foreach { $_.supportedEditions } |
    foreach { $_.supportedServiceLevelObjectives } |
    foreach { 
        $name = $_.name
        $_.supportedMaxSizes |
            foreach {
                [pscustomobject]@{
                    serviceLevelObjective = $name
                    maxSizeBytes = ($_.limit * $totalBytes[$_.unit]).ToString()
                }
            }
    }

$doc = @{
    '$schema' = "https://aka.ms/CCME/schemas/2018-03-01/assessmentRuleListEvaluator.json"
    contentVersion = "1.0.0.0"
    whitelist = $whitelist
    whitelistNoHitMessage = "SQL database max size {maxSizeBytes}Bytes for SKU {serviceLevelObjective} is not supported in Azure China"
}

$doc | ConvertTo-Json -Depth 100 | Out-File "ChinaSQLDatabaseMaxSize.json"