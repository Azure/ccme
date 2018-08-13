﻿// -----------------------------------------------------------------------
// <copyright file="PdfReportGenerator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Models;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Newtonsoft.Json;
using PdfSharp.Drawing;

namespace Microsoft.Azure.CCME.Assessment.Managers.ReportGenerators
{
    internal sealed class PdfReportGenerator : IReportGenerator
    {
        // TODO: split code file
        public async Task<AssessmentReport> ProcessAsync(
            IAssessmentContext context,
            ServiceParityResult serviceParityResult,
            CostEstimationResult costEstimationResult)
        {
            string reportFilePath =
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
            string targetRegion = costEstimationResult.TargetRegion;

            RegionInfo regionInfo = await GetRegionInfoAsync(targetRegion);

            // Create a MigraDoc document and initialize the page setting
            DocumentInfo documentInfo = new DocumentInfo();

            //Introduction Section
            Section introSection = documentInfo.CreateIntroSection();

            //Migration Summary Section
            CreateMigrationSummarySection(
                ref documentInfo,
                regionInfo,
                costEstimationResult,
                parityResult);

            //Service Parity Result
            CreateAppendixServiceParityResultSection(
                ref documentInfo,
                parityResult,
                costEstimationResult);

            // TODO: extract common parameters for creating section

            //Cost Estimation Section
            CreateCostEstimationSection(
                ref documentInfo,
                regionInfo,
                costEstimationResult);

            //Appendix Full Resource List
            CreateAppendixResouceListSection(
                ref documentInfo,
                regionInfo,
                costEstimationResult);

            //Conclusion Section
            documentInfo.CreateConclusionSection();

            //Making TOC
            AppendTOCToIntroductionSection(
                ref introSection,
                costEstimationResult.SubscriptionName,
                regionInfo.IsChinaRegion);

            return documentInfo.MakePDF();
        }

        private static void AppendTOCToIntroductionSection(
            ref Section introSection,
            string subscriptionName,
            bool isChinaRegion)
        {
            // TODO: move hard code string to resource file.
            introSection.AddParagraph("TABLE OF CONTENT", DocumentInfo.SubTitleStyleName);

            AddTOCParagraph(ref introSection, $"Assessment Summary for {subscriptionName}\t", $"MigrationSummary{subscriptionName}");
            AddTOCParagraph(ref introSection, $"Service Parity\t", "ServiceParity");
            AddTOCParagraph(ref introSection, $"Cost Estimation\t", $"CostEstimation{subscriptionName}");
            AddTOCParagraph(ref introSection, $"Full Resource List\t", $"AppendixResourceList{subscriptionName}");
            AddTOCParagraph(ref introSection, $"Conclusion\t", "Conclusion");
        }

        private static void AddTOCParagraph(ref Section section, string txt, string mark)
        {
            // TODO: extract to extension method
            var toc = section.AddParagraph();
            toc.Style = DocumentInfo.TocStyleName;
            var link = toc.AddHyperlink(mark);
            link.AddText(txt);
            link.AddPageRefField(mark);
            toc.AddLineBreak();
        }

        private static Section CreateAppendixResouceListSection(
            ref DocumentInfo documentInfo,
            RegionInfo regionInfo,
            CostEstimationResult costEstimationResult)
        {
            // TODO: extract to extension method
            // TODO: move hard code string to resource file.
            string subscriptionName = costEstimationResult.SubscriptionName;

            Section appendixResouceListSection = documentInfo.CreateSection();
            var appendixResourceListTitle =
                appendixResouceListSection.AddParagraph("Full Resource List For " + subscriptionName,
                DocumentInfo.TitleStyleName);
            appendixResourceListTitle.AddBookmark("AppendixResourceList" + subscriptionName);

            //Resource List by Resource Group
            appendixResouceListSection.AddParagraph(
                "Here is the full resource list in reference subscription related to this migration: ",
                DocumentInfo.TableDesStyleName);

            foreach (var kvp in costEstimationResult.DetailsByResourceGroup)
            {
                var resourceTable = DocumentInfo.CreateTable(ref appendixResouceListSection);
                resourceTable.AddColumn("5cm");
                resourceTable.AddColumn("7cm");
                resourceTable.AddColumn("4.5cm");

                var resourceGroupName = resourceTable.AddRow();
                resourceGroupName.Cells[0].AddParagraph("Resource Group Name");
                resourceGroupName.Cells[0].Format.Font.Bold = true;
                resourceGroupName.Cells[1].MergeRight = 1;
                documentInfo.AddParagraphWordWrap(resourceGroupName.Cells[1], kvp.Key);

                var resourceProperties = resourceTable.AddRow();
                resourceProperties.Format.Font.Bold = true;
                resourceProperties.Cells[0].AddParagraph("Name");
                resourceProperties.Cells[1].AddParagraph("Type");
                resourceProperties.Cells[2].AddParagraph("Location");

                foreach (var detail in kvp.Value)
                {
                    var resourceRow = resourceTable.AddRow();
                    documentInfo.AddParagraphWordWrap(resourceRow.Cells[0], detail.ResourceName);
                    documentInfo.AddParagraphWordWrap(resourceRow.Cells[1], detail.ResourceType);
                    documentInfo.AddParagraphWordWrap(resourceRow.Cells[2], detail.Location);
                }

                appendixResouceListSection.AddParagraph("");
            }

            return appendixResouceListSection;
        }

        private static Section CreateCostEstimationSection(
            ref DocumentInfo documentInfo,
            RegionInfo regionInfo,
            CostEstimationResult costEstimationResult)
        {
            // TODO: extract to extension method
            // TODO: move hard code string to resource file.
            string subscriptionName = costEstimationResult.SubscriptionName;

            Section costEstimationSection = documentInfo.CreateSection();
            var costTitle = costEstimationSection.AddParagraph("Cost Estimation", DocumentInfo.TitleStyleName);
            costTitle.AddBookmark("CostEstimation" + subscriptionName);

            costEstimationSection.AddParagraph(
                "To best help customer plan and estimate the cost in destination environment, China Cloud Migration & Expansion tool set takes customer’s source service usage of one billing cycle as a reference to perform a cost estimation for the same service usage in the destination. Below is the cost estimation summary report. Please note that the cost only reflects the services that is available in destination environment. For services that is not available in destination, N/A are marked.",
                DocumentInfo.TableDesStyleName);

            //Make Cost Summary Table
            var costSummaryTalbe = DocumentInfo.CreateTable(ref costEstimationSection);
            var headCostSummaryColumn = costSummaryTalbe.AddColumn("8cm");
            headCostSummaryColumn.Format.Font.Bold = true;
            costSummaryTalbe.AddColumn("8cm");

            var costSummaryRow = costSummaryTalbe.AddRow();
            costSummaryRow.Cells[0].AddParagraph("Reference Environment");
            costSummaryRow.Cells[1].AddParagraph("Microsoft Azure");

            costSummaryRow = costSummaryTalbe.AddRow();
            costSummaryRow.Cells[0].AddParagraph("Reference Subscription");
            costSummaryRow.Cells[1].AddParagraph(subscriptionName);

            costSummaryRow = costSummaryTalbe.AddRow();
            costSummaryRow.Cells[0].AddParagraph("Target Environment");
            costSummaryRow.Cells[1].AddParagraph(regionInfo.TargetRegionName);

            costSummaryRow = costSummaryTalbe.AddRow();
            costSummaryRow.Cells[0].AddParagraph("Usage Collection Period");
            costSummaryRow.Cells[1].AddParagraph($"{costEstimationResult.StartTime:r} - {costEstimationResult.EndTime:r}");

            costSummaryRow = costSummaryTalbe.AddRow();
            costSummaryRow.Cells[0].AddParagraph("Target Price Base");
            costSummaryRow.Cells[1].AddParagraph("List Price");

            costSummaryRow = costSummaryTalbe.AddRow();
            costSummaryRow.Cells[0].AddParagraph("Currency");
            costSummaryRow.Cells[1].AddParagraph(regionInfo.CurrencyUnit);

            costSummaryRow = costSummaryTalbe.AddRow();
            costSummaryRow.Cells[0].AddParagraph("Currency Format");
            costSummaryRow.Cells[1].AddParagraph(regionInfo.CurrencyCulture.EnglishName);

            costSummaryRow = costSummaryTalbe.AddRow();
            costSummaryRow.Cells[0].AddParagraph("Total Cost in Period for Available Resources");

            if (!costEstimationResult.Details.Any())
            {
                costSummaryRow.Cells[1].AddParagraph("N/A");
            }
            else
            {
                costEstimationSection.AddParagraph(
                    "Please check the below cost list with price by Resource Group:",
                    DocumentInfo.TableDesStyleName);

                //Resource Details

                decimal totalCost = 0;
                decimal totalOriginalCost = 0;
                foreach (var kvp in costEstimationResult.DetailsByResourceGroup)
                {
                    if (!kvp.Value.Any(d => d.EstimatedCost.HasValue))
                    {
                        continue;
                    }

                    string resourceGroup = kvp.Key;
                    Table costTable = DocumentInfo.CreateTable(ref costEstimationSection);
                    costEstimationSection.AddParagraph("");
                    AddCostTableHeaders(ref costTable, documentInfo, resourceGroup);
                    foreach (CostEstimationDetail detail in kvp.Value.Where(d => d.EstimatedCost.HasValue))
                    {
                        AddCostTableRow(ref costTable, documentInfo, regionInfo, detail);
                        totalCost += detail.EstimatedCost.Value;
                    }

                    foreach (CostEstimationDetail detail in kvp.Value.Where(d => !d.EstimatedCost.HasValue))
                    {
                        AddCostTableRow(ref costTable, documentInfo, regionInfo, detail).Cells[0].Format.Font.Color = Colors.Red;

                        // ToDo: Is it necessary to put on total original costs?
                        if (!string.IsNullOrWhiteSpace(detail.SourceMeterId) && string.IsNullOrWhiteSpace(detail.TargetMeterId))
                        {
                            totalOriginalCost += detail.OriginalCost.Value;
                        }
                    }
                }

                // Add total cost to summary table
                costSummaryRow.Cells[1].AddParagraph(ToCurrencyString(totalCost, regionInfo.CurrencySymbol));
            }

            return costEstimationSection;
        }

        private static void AddCostTableHeaders(
            ref Table costTable,
            DocumentInfo documentInfo,
            string tableName)
        {
            // TODO: extract to extension method
            // TODO: refine cost table columns
            costTable.AddColumn("4cm");
            costTable.AddColumn("4cm");
            costTable.AddColumn("6cm");
            costTable.AddColumn("2cm").Format.Alignment = ParagraphAlignment.Right;
            costTable.AddColumn("2cm").Format.Alignment = ParagraphAlignment.Right;

            Row costResourceGroup = costTable.AddRow();
            costResourceGroup.Cells[0].AddParagraph("Resource Group Name");
            costResourceGroup.Cells[0].Format.Font.Bold = true;
            costResourceGroup.Cells[1].MergeRight = 3;
            documentInfo.AddParagraphWordWrap(costResourceGroup.Cells[1], tableName);

            Row costProperties = costTable.AddRow();
            costProperties.Format.Font.Bold = true;
            costProperties.Cells[0].AddParagraph("Name");
            costProperties.Cells[1].AddParagraph("Category");
            costProperties.Cells[2].AddParagraph("Meter Name");
            costProperties.Cells[3].AddParagraph("Quantity").Format.Alignment = ParagraphAlignment.Left;
            costProperties.Cells[4].AddParagraph("Daily Cost").Format.Alignment = ParagraphAlignment.Left;
        }

        private static Row AddCostTableRow(
            ref Table costTable,
            DocumentInfo documentInfo,
            RegionInfo regionInfo,
            CostEstimationDetail costEstimationDetail)
        {
            // TODO: extract to extension method
            // TODO: refine cost table columns
            var allupTableCostItem = costTable.AddRow();
            documentInfo.AddParagraphWordWrap(allupTableCostItem.Cells[0], costEstimationDetail.ResourceName);

            var catergoryName = costEstimationDetail.MeterCategory;
            if (!string.IsNullOrEmpty(costEstimationDetail.MeterSubCategory))
            {
                catergoryName += " - " + costEstimationDetail.MeterSubCategory;
            }
            allupTableCostItem.Cells[1].AddParagraph(catergoryName ?? "N/A");

            allupTableCostItem.Cells[2].AddParagraph(costEstimationDetail?.MeterName ?? "N/A");
            string quantity = costEstimationDetail.UsagesQuantity?.ToString("0.####", CultureInfo.InvariantCulture);
            allupTableCostItem.Cells[3].AddParagraph(quantity ?? "N/A");
            allupTableCostItem.Cells[4].AddParagraph(
                ToCurrencyString(costEstimationDetail.EstimatedCost, regionInfo.CurrencySymbol));
            return allupTableCostItem;
        }

        private static string ToCurrencyString(decimal? value, string currencySymbol)
        {
            // TODO: revisit here to check the decimal to string logic.
            return !value.HasValue ? "N/A"
                : Math.Round(value.Value, 2).ToString("C", CultureInfo.CurrentCulture)
                    .Replace(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, currencySymbol);
        }

        private static Section CreateMigrationSummarySection(
            ref DocumentInfo documentInfo,
            RegionInfo regionInfo,
            CostEstimationResult costEstimationResult,
            ServiceParityResult serviceParityResult)
        {
            // TODO: extract to extension method
            // TODO: move hard code string to resource file.

            string subscriptionName = costEstimationResult.SubscriptionName;
            int resourceGroupsCount = costEstimationResult.ResourceGroupsCount;
            int resourcesCount = costEstimationResult.ResourcesCount;

            Section migrationSummarySection = documentInfo.CreateSection();
            var summaryTitle =
                migrationSummarySection.AddParagraph(
                    "Assessment Summary for " + subscriptionName,
                    DocumentInfo.TitleStyleName);
            summaryTitle.AddBookmark("MigrationSummary" + subscriptionName);

            migrationSummarySection.AddParagraph(
                "Based on user selection and configuration, here is summary about subscription " + subscriptionName + ":",
                DocumentInfo.TableDesStyleName);

            var shortSummary = DocumentInfo.CreateTable(ref migrationSummarySection);

            var summaryTitleColumn = shortSummary.AddColumn("8cm");
            summaryTitleColumn.Format.Font = DocumentInfo.ContentFontBold.Clone();
            shortSummary.AddColumn("8cm");

            var shortSummaryRow = shortSummary.AddRow();
            shortSummaryRow.Cells[0].AddParagraph("Reference Environment");
            shortSummaryRow.Cells[1].AddParagraph("Microsoft Azure");

            shortSummaryRow = shortSummary.AddRow();
            shortSummaryRow.Cells[0].AddParagraph("Reference Subscription");
            documentInfo.AddParagraphWordWrap(shortSummaryRow.Cells[1], subscriptionName);

            shortSummaryRow = shortSummary.AddRow();
            shortSummaryRow.Cells[0].AddParagraph("Target Environment");
            shortSummaryRow.Cells[1].AddParagraph(regionInfo.TargetRegionName);

            shortSummaryRow = shortSummary.AddRow();
            shortSummaryRow.Cells[0].AddParagraph("Number of Resource Group");
            shortSummaryRow.Cells[1].AddParagraph(resourceGroupsCount.ToString());

            shortSummaryRow = shortSummary.AddRow();
            shortSummaryRow.Cells[0].AddParagraph("Number of Resources");
            shortSummaryRow.Cells[1].AddParagraph(resourcesCount.ToString());

            shortSummaryRow = shortSummary.AddRow();
            shortSummaryRow.Cells[0].AddParagraph("Number of Resources Failed in Parity Rule Check/Number of All Resources");

            var detailPassFailed = serviceParityResult.Details.Where(d => !d.Value.Pass);
            var resourceNotPassedCount = costEstimationResult.Details.Where(c => detailPassFailed.Any(d => d.Key == c.ResourceId)).Distinct().Count();
            shortSummaryRow.Cells[1].AddParagraph($"{resourceNotPassedCount}/{resourcesCount}");

            shortSummaryRow = shortSummary.AddRow();
            shortSummaryRow.Cells[0].AddParagraph("Target Region");
            shortSummaryRow.Cells[1].AddParagraph(costEstimationResult.TargetRegion);

            return migrationSummarySection;
        }

        private static Section CreateAppendixServiceParityResultSection(
            ref DocumentInfo documentInfo,
            ServiceParityResult serviceParityResult,
            CostEstimationResult costEstimationResult)
        {
            // TODO: extract to extension method
            // TODO: move hard code string to resource file
            Section appendixServiceParityResultSection = documentInfo.CreateSection();
            var appendixServiceParityResultTitle =
                appendixServiceParityResultSection.AddParagraph("Service Parity", DocumentInfo.TitleStyleName);
            appendixServiceParityResultTitle.AddBookmark("ServiceParity");

            // All parities
            if (serviceParityResult.Pass)
            {
                appendixServiceParityResultSection.AddParagraph(
                   "There is no service failed in the parity rule check regarding this migration & expansion",
                   DocumentInfo.TableDesStyleName);
            }
            else
            {
                appendixServiceParityResultSection.AddParagraph(
                   "Below are the summary of all services failed in the parity rule check regarding this migration & expansion: ",
                   DocumentInfo.TableDesStyleName);

                var detailsAll = serviceParityResult.Details.Where(d => d.Value.Pass == false).ToDictionary(i => i.Key, i => i.Value);

                // Group up parities by resource group
                foreach (var resourceGroup in costEstimationResult.DetailsByResourceGroup)
                {
                    // ResourceGroup.Key is the name of the group
                    if (resourceGroup.Value.Where(r => detailsAll.Keys.Contains(r.ResourceId)).Count() != 0)
                    {
                        string resourceGroupName = resourceGroup.Key;
                        var groupNameTxt = appendixServiceParityResultSection.AddParagraph(
                            "Resource Group: " + resourceGroupName,
                            DocumentInfo.TableDesStyleName);
                    }
                    Table parityTable = DocumentInfo.CreateTable(ref appendixServiceParityResultSection);
                    parityTable.AddColumn("4cm"); // Rule name
                    parityTable.AddColumn("2cm"); // Pass
                    parityTable.AddColumn("12cm"); // Message

                    // Check each resource in the group
                    foreach (var resource in resourceGroup.Value)
                    {
                        if (detailsAll.Keys.Contains(resource.ResourceId) && detailsAll[resource.ResourceId].Pass == false)
                        {
                            // Resource name
                            var parityResourceName = parityTable.AddRow();
                            parityResourceName.Cells[0].AddParagraph("Resource Name");
                            parityResourceName.Cells[0].Format.Font.Bold = true;
                            parityResourceName.Cells[1].MergeRight = 1;
                            parityResourceName.Cells[1].Column.Width = "14.5cm";
                            documentInfo.AddParagraphWordWrap(parityResourceName.Cells[1], resource.ResourceName);

                            // Resource Id
                            var parityResourceID = parityTable.AddRow();
                            parityResourceID.Cells[0].AddParagraph("Resource ID");
                            parityResourceID.Cells[0].Format.Font.Bold = true;
                            parityResourceID.Cells[1].MergeRight = 1;
                            parityResourceID.Cells[1].Column.Width = "14.5cm";
                            string cutMark = "providers";
                            int cut = resource.ResourceId.IndexOf(cutMark) + cutMark.Length;
                            string resourceID = cut == -1 ? resource.ResourceId : resource.ResourceId.Substring(cut, resource.ResourceId.Length - cut);
                            documentInfo.AddParagraphWordWrap(parityResourceID.Cells[1], resourceID);

                            var parityProperties = parityTable.AddRow();
                            parityResourceID.Cells[1].Column.Width = "2cm";

                            parityProperties.Format.Font.Bold = true;
                            parityProperties.Cells[0].AddParagraph("RuleName");
                            parityProperties.Cells[1].AddParagraph("Pass");
                            parityProperties.Cells[2].AddParagraph("Message");

                            // Check each parity result for the resource
                            foreach (var ruleCheck in detailsAll[resource.ResourceId].Details.Where(d => d.Pass == false))
                            {
                                var ruleCheckRow = parityTable.AddRow();
                                documentInfo.AddParagraphWordWrap(ruleCheckRow.Cells[0], ruleCheck.Brief);
                                documentInfo.AddParagraphWordWrap(ruleCheckRow.Cells[1], ("N"));
                                documentInfo.AddParagraphWordWrap(ruleCheckRow.Cells[2], ruleCheck.Message);
                            }
                        }
                    }
                }
            }

            return appendixServiceParityResultSection;
        }

        private static async Task<RegionInfo> GetRegionInfoAsync(string targetRegion)
        {
            string resourceTypeUrl;
            string currencyUnit;
            string targetRegionName;
            CultureInfo currencyCulture;
            bool isChinaRegion = false;
            switch (targetRegion.ToLower())
            {
                case "chinanorth":
                case "chinaeast":
                case "chinanorth2":
                case "chinaeast2":
                    resourceTypeUrl = @"https://globalconnectioncenter.blob.core.windows.net/resourcetypelist/resourceTypeInChina.json";
                    currencyUnit = "RMB(CNY)";
                    targetRegionName = "Azure in China (21 Vianet)";
                    currencyCulture = CultureInfo.GetCultureInfo("zh-cn");
                    isChinaRegion = true;
                    break;
                case "germanycentral":
                case "germanynortheast":
                    resourceTypeUrl = @"https://globalconnectioncenter.blob.core.windows.net/resourcetypelist/resourceTypeInGermany.json";
                    currencyUnit = "EUR";
                    targetRegionName = "Microsoft Azure in Germany";
                    currencyCulture = CultureInfo.GetCultureInfo("de-de");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetRegion), targetRegion, null);
            }

            string currencySymbol = currencyCulture.NumberFormat.CurrencySymbol;

            List<string> targetResoureTypes;
            using (WebClient wc = new WebClient { Encoding = Encoding.UTF8 })
            {
                string resourceTypeContent =
                    await wc.DownloadStringTaskAsync(resourceTypeUrl);
                targetResoureTypes = JsonConvert.DeserializeObject<List<string>>(resourceTypeContent);
            }

            return new RegionInfo
            {
                IsChinaRegion = isChinaRegion,
                CurrencyCulture = currencyCulture,
                CurrencySymbol = currencySymbol,
                CurrencyUnit = currencyUnit,
                TargetRegionName = targetRegionName,
                TargetResoureTypes = targetResoureTypes
            };
        }

        private class DocumentInfo
        {
            public const string TitleStyleName = "Title";
            public const string SubTitleStyleName = "SubTitle";
            public const string ContentStyleName = "Content";
            public const string TableStyleName = "Table";
            public const string TableDesStyleName = "TableDes";
            public const string TocStyleName = "Toc";
            public const string BulletStyleName = "Bullet";

            public static readonly Font TitleFont = new Font
            {
                Bold = true,
                Color = Color.FromCmyk(92, 59, 0, 4),
                Size = 18,
                Name = "Calibri"
            };

            public static readonly Font ContentFont = new Font
            {
                Color = Color.FromCmyk(0, 0, 0, 70),
                Size = 12,
                Name = "Segoe UI"
            };

            public static readonly Font ContentFontBold = new Font
            {
                Color = Colors.Black,
                Size = 12,
                Name = "Segoe UI",
                Bold = true
            };

            public static readonly Font LinkFont = new Font
            {
                Color = Colors.Blue,
                Size = 12,
                Name = "Segoe UI",
                Underline = Underline.Single
            };

            private static readonly XGraphics GraphicsMeasure =
                XGraphics.CreateMeasureContext(
                    new XSize(21, 29.7), XGraphicsUnit.Centimeter, XPageDirection.Downwards);

            private static readonly char[] Whitespaces = { ' ', '\r', '\n', '\t' };

            private static readonly char[] Separators = { '\\', '/', '.', '_' };

            public DocumentInfo()
            {
                Document document = new Document { UseCmykColor = true };
                document.DefaultPageSetup.HeaderDistance = Unit.FromCentimeter(0.8);
                document.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(1.5);
                document.DefaultPageSetup.RightMargin = Unit.FromCentimeter(1.5);
                document.DefaultPageSetup.TopMargin = Unit.FromCentimeter(3);
                document.DefaultPageSetup.BottomMargin = Unit.FromCentimeter(3);

                this.Document = document;

                //Setup Global Style
                Style titleStyle = document.Styles.AddStyle(TitleStyleName, "Normal");
                titleStyle.Font = TitleFont.Clone();
                titleStyle.ParagraphFormat.SpaceAfter = 15;

                Style subTitleStyle = document.Styles.AddStyle(SubTitleStyleName, "Normal");
                subTitleStyle.Font = TitleFont.Clone();
                subTitleStyle.Font.Size = 16;
                subTitleStyle.Font.Bold = false;
                subTitleStyle.ParagraphFormat.SpaceBefore = 20;
                subTitleStyle.ParagraphFormat.SpaceAfter = 15;

                Style contentStyle = document.Styles.AddStyle(ContentStyleName, "Normal");
                contentStyle.Font = ContentFont.Clone();
                contentStyle.ParagraphFormat.LineSpacing = 4;
                contentStyle.ParagraphFormat.SpaceBefore = 5;

                Style tableStyle = document.Styles.AddStyle(TableStyleName, "Normal");
                tableStyle.Font = ContentFont.Clone();
                tableStyle.Font.Color = Colors.Black;
                tableStyle.Font.Size = 10;
                tableStyle.ParagraphFormat.LineSpacing = 4;
                tableStyle.ParagraphFormat.SpaceBefore = 0;

                // TODO: change to static read-only field
                this.TableMeasureFront = new XFont(tableStyle.Font.Name, tableStyle.Font.Size);

                Style tableDesStyle = document.Styles.AddStyle(TableDesStyleName, "Normal");
                tableDesStyle.Font = ContentFont.Clone();
                tableDesStyle.ParagraphFormat.LineSpacing = 4;
                tableDesStyle.ParagraphFormat.SpaceBefore = 5;
                tableDesStyle.ParagraphFormat.SpaceAfter = 10;

                Style bulletStyle = document.Styles.AddStyle(BulletStyleName, "Normal");
                bulletStyle.Font = ContentFont.Clone();
                bulletStyle.ParagraphFormat.LineSpacing = 4;
                bulletStyle.ParagraphFormat.LeftIndent = 20;
                bulletStyle.ParagraphFormat.SpaceBefore = 15;
                bulletStyle.ParagraphFormat.SpaceAfter = 10;

                Style tocStyle = document.Styles.AddStyle(TocStyleName, "Normal");
                tocStyle.Font = ContentFont.Clone();
                tocStyle.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right, TabLeader.Dots);
                tocStyle.ParagraphFormat.SpaceAfter = 15;
            }

            public Document Document { get; private set; }

            public XFont TableMeasureFront { get; set; }

            public static Table CreateTable(ref Section section)
            {
                var table = new Table
                {
                    Style = TableStyleName,
                    Borders =
                {
                    Color = Colors.Gray,
                    Width = 0.25,
                    Left = {Width = 0.5},
                    Right = {Width = 0.5}
                },
                    LeftPadding = 3,
                    RightPadding = 3,
                    Rows =
                {
                    LeftIndent = 0,
                    VerticalAlignment = VerticalAlignment.Center
                },
                    Format =
                {
                    Alignment = ParagraphAlignment.Left,
                    LeftIndent = 0,
                    RightIndent = 0
                }
                };

                section?.Add(table);

                return table;
            }

            public void AddParagraphWordWrap(Cell cell, string text)
            {
                if (text == null)
                {
                    text = string.Empty;
                }
                XSize size = GraphicsMeasure.MeasureString(text, this.TableMeasureFront);
                Column column = cell.Column;

                // We use padding properties from Table instead of Column because the ones from Column
                // are 0 even there is a small default padding
                Unit maxWidth = column.Width - column.Table.LeftPadding - column.Table.RightPadding;

                if (size.Width > maxWidth)
                {
                    var words = text.Split(Whitespaces, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 1)
                    {
                        bool isSplit = false;
                        var processed = new List<string>();
                        foreach (string word in words)
                        {
                            string fit = this.SplitTextWithHyphen(word, maxWidth);
                            if (!ReferenceEquals(fit, word))
                            {
                                processed.Add(fit);
                                isSplit = true;
                            }
                        }

                        if (isSplit)
                        {
                            text = string.Join("\n", processed);
                        }
                    }
                    else
                    {
                        text = this.SplitTextWithHyphen(text, maxWidth);
                    }
                }

                cell.AddParagraph(text);
            }

            public Section CreateSection()
            {
                const string LogoPath = "Images\\logo.png";

                Section section = this.Document.AddSection();

                //setup header
                if (File.Exists(LogoPath))
                {
                    Image image = section.Headers.Primary.AddImage(LogoPath);
                    image.Height = Unit.FromCentimeter(1.5);
                    image.LockAspectRatio = true;
                    image.RelativeVertical = RelativeVertical.Line;
                    image.RelativeHorizontal = RelativeHorizontal.Margin;
                    image.Top = ShapePosition.Top;
                    image.Left = ShapePosition.Left;
                    image.WrapFormat.Style = WrapStyle.TopBottom;
                }

                //setup footer
                Paragraph footer = section.Footers.Primary.AddParagraph();
                footer.AddPageField();
                footer.Format.Alignment = ParagraphAlignment.Right;

                return section;
            }

            public Section CreateIntroSection()
            {
                Section introSection = this.CreateSection();
                //Introduction Title
                introSection.AddParagraph(PdfReportMessages.IntroductionTitle, TitleStyleName);

                //Introduction Content
                introSection.AddParagraph(PdfReportMessages.IntroductionContent_P1, ContentStyleName);
                introSection.AddParagraph(PdfReportMessages.IntroductionContent_P2, ContentStyleName);

                Paragraph introBullets = introSection.AddParagraph();
                introBullets.Style = BulletStyleName;
                introBullets.AddFormattedText(PdfReportMessages.IntroductionContent_ListItem1);
                introBullets.AddLineBreak();
                introBullets.AddFormattedText(PdfReportMessages.IntroductionContent_ListItem2);
                introBullets.AddLineBreak();
                introBullets.AddFormattedText(PdfReportMessages.IntroductionContent_ListItem3);

                return introSection;
            }

            public Section CreateConclusionSection()
            {
                // TODO: move hard code string to resource file.

                Section conclusionSection = this.CreateSection();
                var conclusionTitle = conclusionSection.AddParagraph("Conclusion", TitleStyleName);
                conclusionTitle.AddBookmark("Conclusion");
                conclusionSection.AddParagraph(
                    "China Cloud Migration & Expansion tool set offers assistance for MNC customers to reduce the barriers, address MNC’s pain points on adopting Sovereign Azure Cloud, and also aims to provide a better experience for MNC migrating their diverse services and solutions to Azure around the world. This service will help accelerate MNC on-boarding Sovereign Azure Cloud with a clearer migration strategy, better migration planning, as well as tools that help minimizing the migration effort, shortening the time needed for migration, and lowering the migration risks.",
                    ContentStyleName);

                return conclusionSection;
            }

            public string MakePDF()
            {
                PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer();
                //Generate PDF file

                // Associate the MigraDoc document with a renderer
                pdfRenderer.Document = this.Document;

                // Layout and render document to PDF
                pdfRenderer.RenderDocument();

                // Save the document...
                string filename = "CCMEAssessmentReport" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                string path = Path.Combine(Path.GetTempPath(), filename);

                // TODO: upload pdf file to storage account.
                pdfRenderer.PdfDocument.Save(path);

                Debug.WriteLine($"Save pdf document to: {path}");

                return path;
            }

            private string SplitTextWithHyphen(string word, Unit maxWidth)
            {
                if (GraphicsMeasure.MeasureString(word, this.TableMeasureFront).Width <= maxWidth)
                {
                    return word;
                }

                var parts = word.Split('-');
                if (parts.Length == 1)
                {
                    return this.EnsureFit(word, maxWidth);
                }

                bool isSplite = false;
                var processed = new List<string>();
                foreach (string part in parts)
                {
                    string fit = this.EnsureFit(part, maxWidth);
                    if (!ReferenceEquals(fit, part))
                    {
                        processed.Add(fit);
                        isSplite = true;
                    }
                }

                return isSplite ? string.Join("\n", processed) : word;
            }

            private string EnsureFit(string word, Unit maxWidth)
            {
                int length = word.Length;
                if (length < 2 || GraphicsMeasure.MeasureString(word, this.TableMeasureFront).Width <= maxWidth)
                {
                    return word;
                }

                int splitIndex = length / 2;
                int candidate1 = word.IndexOfAny(Separators, splitIndex);
                int candidate2 = word.LastIndexOfAny(Separators, splitIndex);
                if (candidate1 >= splitIndex && candidate1 < length - 1 &&
                   candidate1 - splitIndex < splitIndex - candidate2)
                {
                    splitIndex = candidate1 + 1;
                }
                else if (candidate2 > 0)
                {
                    splitIndex = candidate2 + 1;
                }

                string firstHalf = this.EnsureFit(word.Substring(0, splitIndex), maxWidth);
                string secondHalf = this.EnsureFit(word.Substring(splitIndex), maxWidth);
                return string.Join("\n", firstHalf, secondHalf);
            }
        }

        private class RegionInfo
        {
            public bool IsChinaRegion { get; set; }

            public CultureInfo CurrencyCulture { get; set; }

            public string CurrencySymbol { get; set; }

            public string CurrencyUnit { get; set; }

            public string TargetRegionName { get; set; }

            public List<string> TargetResoureTypes { get; set; }
        }
    }
}
