Param(
    [Parameter(Mandatory=$True)]
    [string]$outputFilePath
);

# Get subscription id and accessToken of the current session
# Error occurs if there is no valid session. Please log in first
function GetSubscriptionIdAndAccessToken()
{
  $ErrorActionPreference = 'Stop'
  
  if(-not (Get-Module AzureRm.Profile)) {
    Import-Module AzureRm.Profile
  }

  $azureRmProfileModuleVersion = (Get-Module AzureRm.Profile).Version

  # refactoring performed in AzureRm.Profile v3.0 or later
  if($azureRmProfileModuleVersion.Major -ge 3) {
    $azureRmProfile = [Microsoft.Azure.Commands.Common.Authentication.Abstractions.AzureRmProfileProvider]::Instance.Profile
    if(-not $azureRmProfile.Accounts.Count) {
      Write-Error "Ensure you have logged in before calling this function.";    
    }
  } 
  else {
    # AzureRm.Profile < v3.0
    $azureRmProfile = [Microsoft.WindowsAzure.Commands.Common.AzureRmProfileProvider]::Instance.Profile
    if(-not $azureRmProfile.Context.Account.Count) {
      Write-Error "Ensure you have logged in before calling this function."    
    }
  }
  
  $currentAzureContext = Get-AzureRmContext;
  $profileClient = New-Object Microsoft.Azure.Commands.ResourceManager.Common.RMProfileClient($azureRmProfile);
  $sid = $currentAzureContext.Subscription.Id;
  $token = $profileClient.AcquireAccessToken($currentAzureContext.Subscription.TenantId).AccessToken;
  $result = [PSCustomObject]@{
    accessToken = $token
    subscriptionId = $sid
    };
  return $result;
}

# Get meter list for Azure China
function GetPrice{
    $info = GetSubscriptionIdAndAccessToken;
    $subscriptionId = $info.subscriptionId;
    $accessToken = $info.accessToken;
    $apiVersion = "2016-08-31-preview";
    $filterPara ="`$filter"
    $filter = "MS-MC-AZR-0033P";
    $currency = "CNY";
    $locale = "en-US";
    $regionInfo = "CN";

    $httpUri =  [string]::Format("https://management.chinacloudapi.cn/subscriptions/{0}/providers/Microsoft.Commerce/RateCard?api-version={1}&{2}=OfferDurableId eq ’{3}’ and Currency eq ’{4}’ and Locale eq ’{5}’ and RegionInfo eq ’{6}’",$subscriptionId,$apiVersion,$filterPara,$filter,$currency,$locale,$regionInfo)
    $httpUri = $httpUri.Replace(" ","%20").Replace("’","'")

    $basicAuthValue = "Bearer $accessToken";
    $headers = @{ Authorization = $basicAuthValue }

    $res = Invoke-WebRequest -Uri $httpUri -Headers $headers -Method Get -ContentType "application/json" -OutFile $outputFilePath
}

GetPrice