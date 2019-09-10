Manualll created rules
1. ChinaIoTHubSku.json
No official SKU list. Check Azure portal for the SKU list then update the rule manually.
Currently, there is no parity comparing to Global Azure

2. ChinaServerFarmSKU.json
No official SKU list. Check Azure portal for the SKU list then update the rule manually.
Currently, SKU `PxV2` and `Y` (consumption SKU for Function App) are not available in MC

3. ChinaStorageAccountKind.json
No official Kind list. Check Azure portal for the Kind list then update the rule manually.
Currently, `Storage V2` is not available in MC

3. ChinaVirtualMachineAvailabilityZone.json
No official SKU list. Check Azure portal for SKU list then update the rule manually.
Currently, there is no any region support availablility zone in MC

4. ChinaSQLDatabaseMaxSize.json
Semi-manually update via script. PowerShell cmdlet hide necessary infomation. Check `GetSQLDatabaseMaxSizeList.ps1` for details

5. ChinaSQLElasticPoolMaxSize.json
Semi-manually update via script. PowerShell cmdlet hide necessary infomation. Check `GetSQLElasticPoolMaxSizeList.ps1` for details

6. ChinaHDInsightVmSize.json/ChinaHDInsightExcludedVmSize.json
Manually update. PowerShell cmdlet `Get-AzureRmHDInsightProperties` could be used to retrieve the list of vmSize