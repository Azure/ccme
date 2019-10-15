// -----------------------------------------------------------------------
// <copyright file="HtmlReportGenerator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.ReportGenerators
{
    internal sealed class HtmlReportGenerator : IReportGenerator
    {
        public async Task<AssessmentReport> ProcessAsync(
           IAssessmentContext context,
           ServiceParityResult serviceParityResult,
           CostEstimationResult costEstimationResult)
        {
            var reportFilePath =
                await this.CreateReportAsync(
                    serviceParityResult,
                    costEstimationResult);

            return new AssessmentReport
            {
                ReportFilePath = reportFilePath
            };
        }

        public async Task<string> CreateReportAsync(
            ServiceParityResult parityResult,
            CostEstimationResult costEstimationResult)
        {
            var targetRegion = costEstimationResult.TargetRegion;

            var regionInfo = await GetRegionInfoAsync(targetRegion);

            var htmlReport = MakePage(targetRegion,
                regionInfo,
                parityResult,
                costEstimationResult);

            var filename = "CCMEAssessmentReport" + DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + ".html";
            var path = Path.Combine(Path.GetTempPath(), filename);
            Debug.WriteLine(FormattableString.Invariant($"Save PDF document to: {path}"));

            File.WriteAllText(path, htmlReport);
            return path;
        }


        private static async Task<RegionInfo> GetRegionInfoAsync(string targetRegion)
        {
            string currencyUnit;
            string targetRegionName;
            CultureInfo currencyCulture;
            string currencySymbol = string.Empty;

            switch (targetRegion.ToLowerInvariant())
            {
                case "chinanorth":
                case "chinaeast":
                case "chinanorth2":
                case "chinaeast2":
                    currencyUnit = "RMB(CNY)";
                    targetRegionName = "Azure in China (21 Vianet)";
                    currencyCulture = CultureInfo.GetCultureInfo("zh-cn");
                    currencySymbol = "&yen;";
                    break;
                case "germanycentral":
                case "germanynortheast":
                    currencyUnit = "EUR";
                    targetRegionName = "Microsoft Azure in Germany";
                    currencyCulture = CultureInfo.GetCultureInfo("de-de");
                    currencySymbol = "&dollar;";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetRegion), targetRegion, null);
            }

            return await Task.FromResult(new RegionInfo
            {
                CurrencyCulture = currencyCulture,
                CurrencySymbol = currencySymbol,
                CurrencyUnit = currencyUnit,
                TargetRegionName = targetRegionName
            });
        }

        public static string MakePage(string targetRegion,
            RegionInfo regionInfo,
            ServiceParityResult parityResult,
            CostEstimationResult costEstimationResult)
        {
            StringBuilder sb = new StringBuilder();

            using (HtmlTextWriter w = new HtmlTextWriter(new StringWriter(sb, CultureInfo.CurrentCulture)))
            {
                #region Html
                w.AddAttribute("lang", "en");
                w.RenderBeginTag(HtmlTextWriterTag.Html);
                MakeHead(w);
                MakeBody(w, 
                    targetRegion,
                    regionInfo,
                    parityResult,
                    costEstimationResult);
                w.RenderEndTag();
                #endregion Html
            }

            string html = sb.ToString();
            return html;
        }

        private static void MakeHead(HtmlTextWriter w)
        {
            #region Head
            w.RenderBeginTag(HtmlTextWriterTag.Head);
            #region Title
            w.RenderBeginTag(HtmlTextWriterTag.Title);
            w.Write("China Cloud Migration & Expansion Assessment Report");
            w.RenderEndTag();
            #endregion
            #region Link
            w.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
            w.AddAttribute(HtmlTextWriterAttribute.Href, "http://cdn.static.runoob.com/libs/bootstrap/3.3.7/css/bootstrap.min.css");
            w.RenderBeginTag(HtmlTextWriterTag.Link);
            w.RenderEndTag();
            #endregion Link
            w.RenderEndTag();
            #endregion Head
        }

        private static void MakeBody(HtmlTextWriter w,
            string targetRegion,
            RegionInfo regionInfo,
            ServiceParityResult parityResult,
            CostEstimationResult costEstimationResult)
        {
            var needCostEstimation = !costEstimationResult.Details.All(d => d.EstimatedCost == null) || costEstimationResult.HasError;

            #region Body
            w.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "medium");
            w.RenderBeginTag(HtmlTextWriterTag.Body);
            w.AddAttribute(HtmlTextWriterAttribute.Class, "container");
            w.RenderBeginTag(HtmlTextWriterTag.Div);
            MakeIntroduction(w);
            MakeTableOfContent(w, 
                costEstimationResult.SubscriptionName, 
                needCostEstimation);
            MakeAssessmentSummary(w,
                regionInfo,
                costEstimationResult,
                parityResult);
            MakeServiceParity(w,
                parityResult,
                costEstimationResult);
            if (needCostEstimation)
            {
                MakeCostEstimation(w,
                    regionInfo, 
                    costEstimationResult);
            }

            MakeFullResourceList(w,
                regionInfo,
                costEstimationResult);

            MakeConclusion(w);
            w.RenderEndTag();
            w.RenderEndTag();
            #endregion Body
        }

        private static void MakeIntroduction(HtmlTextWriter w)
        {
            w.RenderBeginTag(HtmlTextWriterTag.Div);
            w.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            MakeHeader(w, @"China Cloud Migration & Expansion Assessment Report", null, HtmlTextWriterTag.H1);
            MakeParagraph(w, @"The aim of China Cloud Migration & Expansion tool set is to ease the migration process for Global Azure customers while migrating / expanding their global solutions to Azure China, and vice versa. This report is part of the tool set offering.");
            MakeParagraph(w, @"The purpose of this report is to help customers to decide whether to perform migration by providing following information:");
            w.RenderBeginTag(HtmlTextWriterTag.Ul);
            MakeList(w, @"Understand what to migrate and to comprehend the migration outcome");
            MakeList(w, @"Identify the service and configuration gaps between the source and the destination environment and minimize the unexpected migration risks");
            MakeList(w, @"Estimate the cost in the destination environment for budget planning.");
            w.RenderEndTag();
            w.RenderEndTag();
        }

        private static void MakeTableOfContent(HtmlTextWriter w,
            string subscriptionName,
            bool needCostEstimation)
        {
            w.RenderBeginTag(HtmlTextWriterTag.Div);
            MakeHeader(w, @"Table of Content");
            w.RenderBeginTag(HtmlTextWriterTag.Ul);
            MakeTocList(w, FormattableString.Invariant($"Assessment Summary for {subscriptionName}\t"), "#AssessmentSummary");
            MakeTocList(w, "Service Parity", "#ServiceParity");
            if (needCostEstimation)
            {
                MakeTocList(w, "Cost Estimation", "#CostEstimation");
            }
            MakeTocList(w, "Full Resource List", "#FullResourceList");
            MakeTocList(w, "Conclusion", "#Conclusion");
            w.RenderEndTag();
            w.RenderEndTag();
        }

        private static void MakeAssessmentSummary(HtmlTextWriter w,
            RegionInfo regionInfo,
            CostEstimationResult costEstimationResult,
            ServiceParityResult serviceParityResult)
        {
            MakeHeader(w, "AssessmentSummary for " + costEstimationResult.SubscriptionName, "AssessmentSummary");
            MakeParagraph(w, "Based on user selection and configuration, here is summary about subscription CHN;TST;SHARED;0002:");
            MakeAssessmentSummaryTable(w,
                regionInfo,
                costEstimationResult,
                serviceParityResult);
        }

        private static void MakeAssessmentSummaryTable(HtmlTextWriter w,
            RegionInfo regionInfo,
            CostEstimationResult costEstimationResult,
            ServiceParityResult serviceParityResult)
        {
            var subscriptionName = costEstimationResult.SubscriptionName;
            var resourceGroupsCount = costEstimationResult.ResourceGroupsCount;
            var resourcesCount = costEstimationResult.ResourcesCount;

            #region Table
            w.AddAttribute(HtmlTextWriterAttribute.Class, "table table-bordered");
            w.RenderBeginTag(HtmlTextWriterTag.Table);

            #region Tbody
            w.RenderBeginTag(HtmlTextWriterTag.Tbody);

            #region Reference Environment
            MakeKeyValueRow(w, "Reference Environment", "Microsoft Azure");
            #endregion Reference Environment

            #region Reference Subscription
            MakeKeyValueRow(w, "Reference Subscription", subscriptionName);
            #endregion Reference Subscription

            #region Target Environment
            MakeKeyValueRow(w, "Target Environment", regionInfo.TargetRegionName);
            #endregion Target Environment

            #region Number of Resource Group
            MakeKeyValueRow(w, "Number of Resource Group", resourceGroupsCount.ToString(CultureInfo.InvariantCulture));
            #endregion Number of Resource Group

            #region Number of Resources
            MakeKeyValueRow(w, "Number of Resources", resourcesCount.ToString(CultureInfo.InvariantCulture));
            #endregion Number of Resources

            #region Number of Resources Failed in Parity Rule Check/Number of All Resources
            var detailPassFailed = serviceParityResult.Details.Where(d => !d.Value.Pass);
            var resourceNotPassedCount = costEstimationResult.Details.Where(c => detailPassFailed.Any(d => d.Key == c.ResourceId)).GroupBy(g => g.ResourceId).Count();
            MakeKeyValueRow(w, "Number of Resources Failed in Parity Rule Check/Number of All Resources", FormattableString.Invariant($"{resourceNotPassedCount}/{resourcesCount}"));
            #endregion Number of Resources Failed in Parity Rule Check/Number of All Resources

            #region Target Region
            MakeKeyValueRow(w, "Target Region", costEstimationResult.TargetRegion);
            #endregion Target Region

            w.RenderEndTag();
            #endregion Tbody

            w.RenderEndTag();
            #endregion Table
        }

        private static void MakeServiceParity(HtmlTextWriter w,
            ServiceParityResult serviceParityResult,
            CostEstimationResult costEstimationResult)
        {
            MakeHeader(w, "Service Parity", "ServiceParity");
            if (serviceParityResult.Pass)
            {
                MakeParagraph(w, "There is no service failed in the parity rule check regarding this migration & expansion");
            }
            else
            {
                MakeParagraph(w, "Below are the summary of all services failed in the parity rule check regarding this migration & expansion:");
            }
            MakeSeviceParityTable(w,
                serviceParityResult,
                costEstimationResult);
        }

        private static void MakeSeviceParityTable(HtmlTextWriter w,
            ServiceParityResult serviceParityResult,
            CostEstimationResult costEstimationResult)
        {
            var detailsAll = serviceParityResult.Details.Where(d => d.Value.Pass == false).ToDictionary(i => i.Key, i => i.Value);

            // Group up parities by resource group
            foreach (var resourceGroup in costEstimationResult.DetailsByResourceGroup)
            {
                // ResourceGroup.Key is the name of the group
                if (resourceGroup.Value.Where(r => detailsAll.Keys.Contains(r.ResourceId)).Count() != 0)
                {
                    var resourceGroupName = resourceGroup.Key;
                    MakeParagraph(w, "Resource Group: " + resourceGroupName);
                }
                // Check each resource in the group
                foreach (var resource in resourceGroup.Value)
                {
                    if (detailsAll.Keys.Contains(resource.ResourceId) && detailsAll[resource.ResourceId].Pass == false)
                    {
                        #region Table
                        w.AddAttribute(HtmlTextWriterAttribute.Class, "table table-bordered");
                        w.RenderBeginTag(HtmlTextWriterTag.Table);

                        #region Tbody
                        w.RenderBeginTag(HtmlTextWriterTag.Tbody);

                        #region Resource Name 
                        w.RenderBeginTag(HtmlTextWriterTag.Tr);
                        w.AddAttribute(HtmlTextWriterAttribute.Scope, "row");
                        w.RenderBeginTag(HtmlTextWriterTag.Th);
                        w.Write("Resource Name");
                        w.RenderEndTag();
                        w.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
                        w.RenderBeginTag(HtmlTextWriterTag.Td);
                        w.Write(resource.ResourceName);
                        w.RenderEndTag();
                        w.RenderEndTag();
                        #endregion Resource Name 

                        #region Resource Id 
                        var cutMark = "providers";
                        var cut = resource.ResourceId.IndexOf(cutMark, StringComparison.OrdinalIgnoreCase) + cutMark.Length;
                        var resourceID = cut == -1 ? resource.ResourceId : resource.ResourceId.Substring(cut, resource.ResourceId.Length - cut);

                        w.RenderBeginTag(HtmlTextWriterTag.Tr);
                        w.AddAttribute(HtmlTextWriterAttribute.Scope, "row");
                        w.RenderBeginTag(HtmlTextWriterTag.Th);
                        w.Write("Resource Id");
                        w.RenderEndTag();
                        w.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
                        w.RenderBeginTag(HtmlTextWriterTag.Td);
                        w.Write(resourceID);
                        w.RenderEndTag();
                        w.RenderEndTag();
                        #endregion Resource Id

                        #region Head for RuleName Pass Message
                        w.RenderBeginTag(HtmlTextWriterTag.Tr);

                        w.AddAttribute(HtmlTextWriterAttribute.Style, "width:30%");
                        w.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                        w.RenderBeginTag(HtmlTextWriterTag.Th);
                        w.Write("RuleName");
                        w.RenderEndTag();

                        w.AddAttribute(HtmlTextWriterAttribute.Style, "width:15%");
                        w.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                        w.RenderBeginTag(HtmlTextWriterTag.Th);
                        w.Write("Pass");
                        w.RenderEndTag();

                        w.AddAttribute(HtmlTextWriterAttribute.Style, "width:55%");
                        w.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                        w.RenderBeginTag(HtmlTextWriterTag.Th);
                        w.Write("Message");
                        w.RenderEndTag();

                        w.RenderEndTag();
                        #endregion Head for RuleName Pass Message

                        #region Data for RuleName Pass Message
                        foreach (var ruleCheck in detailsAll[resource.ResourceId].Details.Where(d => d.Pass == false))
                        {
                            w.RenderBeginTag(HtmlTextWriterTag.Tr);
                            w.RenderBeginTag(HtmlTextWriterTag.Td);
                            w.Write(ruleCheck.Brief);
                            w.RenderEndTag();

                            w.RenderBeginTag(HtmlTextWriterTag.Td);
                            w.Write("N");
                            w.RenderEndTag();

                            w.RenderBeginTag(HtmlTextWriterTag.Td);
                            w.Write(ruleCheck.Message);
                            w.RenderEndTag();

                            w.RenderEndTag();
                        }  
                        #endregion Data for RuleName Pass Message

                        w.RenderEndTag();
                        #endregion Tbody

                        w.RenderEndTag();
                        #endregion Table
                    }
                }
            }
        }

        private static void MakeCostEstimation(HtmlTextWriter w,
            RegionInfo regionInfo,
            CostEstimationResult costEstimationResult)
        {
            MakeHeader(w, "Cost Estimation", "CostEstimation");
            if (costEstimationResult.HasError)
            {
                MakeParagraph(w,
                    "Failed to retrieve Azure resource usage to build the cost estimation due to insufficient permission. Please check your Azure RBAC role and ensure you have any of the role below for the whole subscription: Reader, Contributor and Owner.");
                return;
            }
            MakeParagraph(w, @"To best help customer plan and estimate the cost in destination environment, China Cloud Migration & Expansion tool set takes customer’s source service usage of one billing cycle as a reference to perform a cost estimation for the same service usage in the destination. Below is the cost estimation summary report. Please note that the cost only reflects the services that is available in destination environment. For services that is not available in destination, N/A are marked.");

            MakeCostEstimationSummaryTable(w,
                regionInfo,
                costEstimationResult);

            MakeCostEstimationResourceGroupTable(w,
                regionInfo,
                costEstimationResult);
        }

        private static void MakeCostEstimationSummaryTable(HtmlTextWriter w,
            RegionInfo regionInfo,
            CostEstimationResult costEstimationResult)
        {
            #region Table
            w.AddAttribute(HtmlTextWriterAttribute.Class, "table table-bordered");
            w.RenderBeginTag(HtmlTextWriterTag.Table);

            #region Tbody
            w.RenderBeginTag(HtmlTextWriterTag.Tbody);

            #region Reference Environment
            MakeKeyValueRow(w, "Reference Environment", "Microsoft Azure");
            #endregion Reference Environment

            #region Reference Subscription
            MakeKeyValueRow(w, "Reference Subscription", costEstimationResult.SubscriptionName);
            #endregion Reference Subscription

            #region Target Environment
            MakeKeyValueRow(w, "Target Environment", regionInfo.TargetRegionName);
            #endregion Target Environment

            #region Usage Collection Period
            MakeKeyValueRow(w, "Usage Collection Period", FormattableString.Invariant($"{costEstimationResult.StartTime:r} - {costEstimationResult.EndTime:r}"));
            #endregion Usage Collection Period

            #region Target Price Base
            MakeKeyValueRow(w, "Target Price Base", "List Price");
            #endregion Target Price Base

            #region Currency
            MakeKeyValueRow(w, "Currency", regionInfo.CurrencyUnit);
            #endregion Currency

            #region Currency Format
            MakeKeyValueRow(w, "Currency Format", regionInfo.CurrencyCulture.EnglishName);
            #endregion Currency Format

            #region Total Cost in Period for Available Resources
            if (!costEstimationResult.Details.Any())
            {
                MakeKeyValueRow(w, "Total Cost in Period for Available Resources", "N/A");
            }
            else
            {
                decimal totalCost = 0;
                foreach (var kvp in costEstimationResult.DetailsByResourceGroup)
                {
                    if (!kvp.Value.Any(d => d.EstimatedCost.HasValue))
                    {
                        continue;
                    }
                    foreach (var detail in kvp.Value.Where(d => d.EstimatedCost.HasValue))
                    {
                        totalCost += detail.EstimatedCost.Value;
                    }
                }
                MakeKeyValueRow(w, "Total Cost in Period for Available Resources", ToCurrencyString(totalCost, regionInfo.CurrencySymbol));
            }
            #endregion Total Cost in Period for Available Resources

            w.RenderEndTag();
            #endregion Tbody

            w.RenderEndTag();
            #endregion Table
        }

        private static void MakeCostEstimationResourceGroupTable(HtmlTextWriter w,
            RegionInfo regionInfo,
            CostEstimationResult costEstimationResult)
        {
            foreach (var kvp in costEstimationResult.DetailsByResourceGroup)
            {
                if (!kvp.Value.Any(d => d.EstimatedCost.HasValue))
                {
                    continue;
                }

                #region Table
                w.AddAttribute(HtmlTextWriterAttribute.Class, "table table-bordered");
                w.RenderBeginTag(HtmlTextWriterTag.Table);

                #region Tbody
                w.RenderBeginTag(HtmlTextWriterTag.Tbody);

                #region Resource Group Name 
                w.RenderBeginTag(HtmlTextWriterTag.Tr);
                w.AddAttribute(HtmlTextWriterAttribute.Scope, "row");
                w.RenderBeginTag(HtmlTextWriterTag.Th);
                w.Write("Resource Group Name");
                w.RenderEndTag();
                w.AddAttribute(HtmlTextWriterAttribute.Colspan, "4");
                w.RenderBeginTag(HtmlTextWriterTag.Td);
                w.Write(kvp.Key);
                w.RenderEndTag();
                w.RenderEndTag();
                #endregion Resource Group Name 

                #region Head for Name Category MeterName Quantity DailyCost
                w.RenderBeginTag(HtmlTextWriterTag.Tr);
                w.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                w.AddAttribute(HtmlTextWriterAttribute.Style, "width:20%");
                w.RenderBeginTag(HtmlTextWriterTag.Th);
                w.Write("Name");
                w.RenderEndTag();

                w.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                w.AddAttribute(HtmlTextWriterAttribute.Style, "width:20%");
                w.RenderBeginTag(HtmlTextWriterTag.Th);
                w.Write("Category");
                w.RenderEndTag();

                w.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                w.AddAttribute(HtmlTextWriterAttribute.Style, "width:20%");
                w.RenderBeginTag(HtmlTextWriterTag.Th);
                w.Write("Meter Name");
                w.RenderEndTag();

                w.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                w.AddAttribute(HtmlTextWriterAttribute.Style, "width:20%");
                w.RenderBeginTag(HtmlTextWriterTag.Th);
                w.Write("Quantity");
                w.RenderEndTag();

                w.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                w.AddAttribute(HtmlTextWriterAttribute.Style, "width:20%");
                w.RenderBeginTag(HtmlTextWriterTag.Th);
                w.Write("Daily Cost");
                w.RenderEndTag();

                w.RenderEndTag();
                #endregion Head for Name Category MeterName Quantity DailyCost

                #region Data for Name Category MeterName Quantity DailyCost
                foreach (var detail in kvp.Value.Where(d => d.EstimatedCost.HasValue))
                {
                    w.RenderBeginTag(HtmlTextWriterTag.Tr);

                    MakeCostEstimationResouceGroupTableRow(w, detail, regionInfo);

                    w.RenderEndTag();
                }

                foreach (var detail in kvp.Value.Where(d => !d.EstimatedCost.HasValue))
                {
                    w.RenderBeginTag(HtmlTextWriterTag.Tr);

                    MakeCostEstimationResouceGroupTableRow(w, detail, regionInfo, true);

                    w.RenderEndTag();
                }

                #endregion Data for Name Category MeterName Quantity DailyCost

                w.RenderEndTag();
                #endregion Tbody

                w.RenderEndTag();
                #endregion Table
            }
        }

        private static void MakeCostEstimationResouceGroupTableRow(HtmlTextWriter w,
            CostEstimationDetail detail,
            RegionInfo regionInfo,
            bool isError = false)
        {
            #region ResourceName
            if(isError)
            {
                w.AddStyleAttribute("color", "#C2403D");
            }
            w.RenderBeginTag(HtmlTextWriterTag.Td);
            w.Write(detail.ResourceName);
            w.RenderEndTag();
            #endregion ResourceName

            #region CategoryName
            var categoryName = detail.MeterCategory;
            if (!string.IsNullOrEmpty(detail.MeterSubCategory))
            {
                categoryName += " - " + detail.MeterSubCategory;
            }
            w.RenderBeginTag(HtmlTextWriterTag.Td);
            w.Write(categoryName ?? "N/A");
            w.RenderEndTag();
            #endregion CategoryName

            #region MeterName
            w.RenderBeginTag(HtmlTextWriterTag.Td);
            w.Write(detail?.MeterName ?? "N/A");
            w.RenderEndTag();
            #endregion MeterName

            #region Quantity
            var quantity = detail.UsagesQuantity?.ToString("0.####", CultureInfo.InvariantCulture);
            w.RenderBeginTag(HtmlTextWriterTag.Td);
            w.Write(quantity ?? "N/A");
            w.RenderEndTag();
            #endregion Quantity

            #region EstimatedCost
            w.RenderBeginTag(HtmlTextWriterTag.Td);
            var c = ToCurrencyString(detail.EstimatedCost, regionInfo.CurrencySymbol);
            w.Write(ToCurrencyString(detail.EstimatedCost, regionInfo.CurrencySymbol));
            w.RenderEndTag();
            #endregion EstimatedCost
        }

        private static string ToCurrencyString(decimal? value, string currencySymbol)
        {
            // TODO: revisit here to check the decimal to string logic.
            return !value.HasValue ? "N/A"
                : Math.Round(value.Value, 2).ToString("C", CultureInfo.CurrentCulture)
                    .Replace(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, currencySymbol);
        }

        private static void MakeFullResourceList(HtmlTextWriter w,
            RegionInfo regionInfo,
            CostEstimationResult costEstimationResult)
        {
            MakeHeader(w, "Full Resource List for " + costEstimationResult.SubscriptionName, "FullResourceList");
            MakeParagraph(w, @"Here is the full resource list in reference subscription related to this migration:");

            var locationMap = costEstimationResult.LocationMap[costEstimationResult.SubscriptionId];

            foreach (var kvp in costEstimationResult.DetailsByResourceGroup)
            {
                #region Table
                w.AddAttribute(HtmlTextWriterAttribute.Class, "table table-bordered");
                w.RenderBeginTag(HtmlTextWriterTag.Table);

                #region Tbody
                w.RenderBeginTag(HtmlTextWriterTag.Tbody);

                #region Resource Group Name 
                w.RenderBeginTag(HtmlTextWriterTag.Tr);
                w.AddAttribute(HtmlTextWriterAttribute.Scope, "row");
                w.RenderBeginTag(HtmlTextWriterTag.Th);
                w.Write("Resource Group Name");
                w.RenderEndTag();
                w.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
                w.RenderBeginTag(HtmlTextWriterTag.Td);
                w.Write(kvp.Key);
                w.RenderEndTag();
                w.RenderEndTag();
                #endregion Resource Group Name 

                #region Head for Name Type Location
                w.RenderBeginTag(HtmlTextWriterTag.Tr);
                w.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                w.AddAttribute(HtmlTextWriterAttribute.Style, "width:30%");
                w.RenderBeginTag(HtmlTextWriterTag.Th);
                w.Write("Name");
                w.RenderEndTag();

                w.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                w.AddAttribute(HtmlTextWriterAttribute.Style, "width:55%");
                w.RenderBeginTag(HtmlTextWriterTag.Th);
                w.Write("Type");
                w.RenderEndTag();

                w.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                w.AddAttribute(HtmlTextWriterAttribute.Style, "width:15%");
                w.RenderBeginTag(HtmlTextWriterTag.Th);
                w.Write("Location");
                w.RenderEndTag();

                w.RenderEndTag();
                #endregion Head for Name Type Location

                #region Data for Name Type Location
                foreach (var detail in kvp.Value)
                {
                    #region Row
                    w.RenderBeginTag(HtmlTextWriterTag.Tr);

                    #region Name
                    w.RenderBeginTag(HtmlTextWriterTag.Td);
                    w.Write(detail.ResourceName);
                    w.RenderEndTag();
                    #endregion Name

                    #region Type
                    w.RenderBeginTag(HtmlTextWriterTag.Td);
                    w.Write(detail.ResourceType);
                    w.RenderEndTag();
                    #endregion Type

                    #region Location
                    w.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (locationMap.TryGetValue(detail.Location, out var locationDisplayName))
                    {
                        w.Write(locationDisplayName);
                    }
                    else
                    {
                        w.Write(detail.Location);
                    }
                    w.RenderEndTag();
                    #endregion Location

                    w.RenderEndTag();
                    #endregion Row
                }
                #endregion Data for Name Type Location

                w.RenderEndTag();
                #endregion Tbody

                w.RenderEndTag();
                #endregion Table
            }
        }

        private static void MakeConclusion(HtmlTextWriter w)
        {
            MakeHeader(w, "Conclusion", "Conclusion");
            MakeParagraph(w, @"China Cloud Migration & Expansion tool set offers assistance for MNC customers to reduce the barriers, address MNC’s pain points on adopting Sovereign Azure Cloud, and also aims to provide a better experience for MNC migrating their diverse services and solutions to Azure around the world. This service will help accelerate MNC on-boarding Sovereign Azure Cloud with a clearer migration strategy, better migration planning, as well as tools that help minimizing the migration effort, shortening the time needed for migration, and lowering the migration risks.");
        }


        private static void MakeKeyValueRow(HtmlTextWriter w, string key, string value, string valueColSpan = "1")
        {
            w.RenderBeginTag(HtmlTextWriterTag.Tr);
            w.AddAttribute(HtmlTextWriterAttribute.Scope, "row");
            w.AddAttribute(HtmlTextWriterAttribute.Style, "width:50%");
            w.RenderBeginTag(HtmlTextWriterTag.Th);
            w.Write(key);
            w.RenderEndTag();
            if (valueColSpan != "1")
            {
                w.AddAttribute(HtmlTextWriterAttribute.Colspan, valueColSpan);
            }
            w.RenderBeginTag(HtmlTextWriterTag.Td);
            w.Write(value);
            w.RenderEndTag();
            w.RenderEndTag();
        }


        private static void MakeParagraph(HtmlTextWriter w, string content)
        {
            #region Paragraph
            w.RenderBeginTag(HtmlTextWriterTag.P);
            w.Write(content);
            w.RenderEndTag();
            #endregion Paragraph
        }

        private static void MakeList(HtmlTextWriter w, string content)
        {
            #region List
            w.RenderBeginTag(HtmlTextWriterTag.Li);
            w.Write(content);
            w.RenderEndTag();
            #endregion List
        }

        private static void MakeTocList(HtmlTextWriter w, string content, string url)
        {
            #region List
            w.RenderBeginTag(HtmlTextWriterTag.Li);
            #region A
            w.AddAttribute(HtmlTextWriterAttribute.Href, url);
            w.RenderBeginTag(HtmlTextWriterTag.A);
            w.Write(content);
            w.RenderEndTag();
            #endregion A
            w.RenderEndTag();
            #endregion List
        }

        private static void MakeHeader(HtmlTextWriter w, string content, string id = null, HtmlTextWriterTag headTag = HtmlTextWriterTag.H2)
        {
            #region Header
            if (id != null)
            {
                w.AddAttribute(HtmlTextWriterAttribute.Id, id);
            }
            w.RenderBeginTag(headTag);
            w.Write(content);
            w.RenderEndTag();
            #endregion Header
        }

        private static void MakeHref(HtmlTextWriter w, string url, string content)
        {
            #region A
            w.AddAttribute(HtmlTextWriterAttribute.Href, url);
            w.RenderBeginTag(HtmlTextWriterTag.A);
            w.Write(content);
            w.RenderEndTag();
            #endregion A
        }
    }
}
