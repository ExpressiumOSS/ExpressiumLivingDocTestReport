﻿using AngleSharp.Html;
using AngleSharp.Html.Parser;
using ReqnRoll.TestExecution;
using ReqnRoll.TestExecutionReport.Extensions;
using ReqnRoll.TestExecutionReport.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReqnRoll.TestExecutionReport
{
    internal class TestExecutionReportGenerator
    {
        private string filePath;
        private string outputPath;

        public TestExecutionReportGenerator(string filePath, string outputPath)
        {
            this.filePath = filePath;
            this.outputPath = outputPath;
        }

        internal void Execute()
        {
            Console.WriteLine("");
            Console.WriteLine("Generating Test Execution Report...");
            Console.WriteLine("FilePath: " + filePath);
            Console.WriteLine("OutputPath: " + outputPath);
            Console.WriteLine("");

            Console.WriteLine("Parsing Test Execution JSON File...");
            var executionContext = TestExecutionUtilities.DeserializeAsJson<TestExecutionContext>(filePath);

            Console.WriteLine("Creating Test Execution Output Directories...");
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
            Directory.CreateDirectory(outputPath);
            Directory.CreateDirectory(Path.Combine(outputPath, "Attachments"));

            // Assign Unique Identifier to all Scenarios...
            foreach (var feature in executionContext.Features)
            {
                foreach (var scenario in feature.Scenarios)
                    scenario.Id = Guid.NewGuid().ToString();
            }

            // Copy Attachments to Output Directory...
            foreach (var feature in executionContext.Features)
            {
                foreach (var scenario in feature.Scenarios)
                {
                    foreach (var example in scenario.Examples)
                    {
                        foreach (var attachment in example.Attachments)
                        {
                            if (File.Exists(attachment))
                                File.Copy(attachment, Path.Combine(outputPath, "Attachments", Path.GetFileName(attachment)), true);
                        }
                    }
                }
            }

            Console.WriteLine("Generating Test Execution HTML Report...");
            GenerateTestExecutionReport(executionContext);

            Console.WriteLine("Generating Test Execution Report Completed");
            Console.WriteLine("");
        }

        private void GenerateTestExecutionReport(TestExecutionContext executionContext)
        {
            List<string> listOfLines = new List<string>();

            listOfLines.Add("<!DOCTYPE html>");
            listOfLines.Add("<html>");
            listOfLines.AddRange(GenerateHead());
            listOfLines.Add("<body>");
            listOfLines.AddRange(GenerateHeader());
            listOfLines.AddRange(GenerateContent(executionContext));
            listOfLines.AddRange(GenerateFooter());
            listOfLines.AddRange(GenerateData(executionContext));
            listOfLines.Add("</body>");
            listOfLines.Add("</html>");

            var htmlFilePath = Path.Combine(outputPath, "LivingDoc.html");
            SaveListOfLinesToFile(htmlFilePath, listOfLines);
        }

        private List<string> GenerateHead()
        {
            var listOfLines = new List<string>();

            listOfLines.Add($"<head>");

            listOfLines.Add("<meta charset='UTF-8'>");
            listOfLines.Add("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            listOfLines.Add("<title>ReqnRoll LivingDoc</title>");
            listOfLines.Add("<link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>");

            listOfLines.AddRange(GenerateStyles());
            listOfLines.AddRange(GenerateScripts());

            listOfLines.Add($"</head>");

            return listOfLines;
        }

        internal List<string> GenerateStyles()
        {
            return Resources.Styles.Split(Environment.NewLine).ToList();
        }

        internal List<string> GenerateScripts()
        {
            return Resources.Scripts.Split(Environment.NewLine).ToList();
        }

        private List<string> GenerateHeader()
        {
            List<string> listOfLines = new List<string>();

            listOfLines.Add("<!-- Header Section -->");
            listOfLines.Add("<header>");
            listOfLines.Add("ReqnRoll LivingDoc");
            listOfLines.Add("</header>");

            return listOfLines;
        }

        private List<string> GenerateContent(TestExecutionContext executionContext)
        {
            List<string> listOfLines = new List<string>();

            listOfLines.Add("<!-- Content Wrapper Section -->");
            listOfLines.Add("<div id='content-wrapper'>");

            listOfLines.Add("<!-- Left Content Section -->");
            listOfLines.Add("<div id='left-section' class='bg-light p-3'>");
            listOfLines.AddRange(GenerateContentOverview(executionContext));
            listOfLines.Add("</div>");

            listOfLines.Add("<!-- Splitter Content Section -->");
            listOfLines.Add("<div id='splitter'></div>");

            listOfLines.Add("<!-- Right Content Section -->");
            listOfLines.Add("<div id='right-section' class='bg-light p-3'>");
            listOfLines.Add("</div>");

            listOfLines.Add("</div>");

            listOfLines.Add("<!-- Content Splitter Script -->");
            listOfLines.AddRange(Resources.SplitterScript.Split(Environment.NewLine).ToList());

            return listOfLines;
        }

        private List<string> GenerateContentOverview(TestExecutionContext executionContext)
        {
            List<string> listOfLines = new List<string>();

            listOfLines.AddRange(GenerateProjectInformation(executionContext));

            listOfLines.Add("<!-- Content Overview Section -->");
            listOfLines.Add("<table width='100%'>");

            listOfLines.Add("<tr><td></td></tr>");
            listOfLines.Add("<tr><td></td></tr>");
            listOfLines.Add("<tr>");
            listOfLines.Add("<td align='center'>");
            listOfLines.AddRange(GenerateScenarioStatusChart(executionContext));
            listOfLines.Add("<br />");
            listOfLines.Add("</td>");
            listOfLines.Add("</tr>");

            //listOfLines.Add("<tr>");
            //listOfLines.Add("<td align='right'>");
            //listOfLines.AddRange(GenerateScenarioPreFilters(executionContext));
            //listOfLines.Add("</td>");
            //listOfLines.Add("</tr>");

            listOfLines.Add("<tr>");
            listOfLines.Add("<td>");
            listOfLines.AddRange(GenerateScenarioFilter(executionContext));
            listOfLines.Add("</td>");
            listOfLines.Add("</tr>");

            listOfLines.Add("<tr>");
            listOfLines.Add("<td>");
            listOfLines.AddRange(GenerateScenarioTableGrid(executionContext));
            listOfLines.Add("</td>");
            listOfLines.Add("</tr>");

            listOfLines.Add("</table>");

            return listOfLines;
        }

        private List<string> GenerateProjectInformation(TestExecutionContext executionContext)
        {
            List<string> listOfLines = new List<string>();

            listOfLines.Add("<!-- Project Information Section -->");
            listOfLines.Add("<div>");
            listOfLines.Add("<span class='project-name'>" + executionContext.Title + "</span><br />");
            listOfLines.Add("<span class='project-date'>generated " + executionContext.GetExecutionTime() + "</span>");
            listOfLines.Add("</div>");

            return listOfLines;
        }

        private List<string> GenerateScenarioStatusChart(TestExecutionContext executionContext)
        {
            List<string> listOfLines = new List<string>();

            var numberOfFailed = 0;
            var numberOfSkipped = 0;
            var numberOfInconclusive = 0;
            var numberOfPassed = 0;

            foreach (var feature in executionContext.Features)
            {
                foreach (var scenario in feature.Scenarios)
                {
                    if (scenario.IsFailed())
                        numberOfFailed++;
                    else if (scenario.IsInconclusive())
                        numberOfInconclusive++;
                    else if (scenario.IsSkipped())
                        numberOfSkipped++;
                    else if (scenario.IsPassed())
                        numberOfPassed++;
                    else
                    {
                        numberOfInconclusive++;
                    }
                }
            }

            var numberOfTests = numberOfPassed + numberOfSkipped + numberOfInconclusive + numberOfFailed;

            listOfLines.Add("<!-- Status Chart Section -->");
            listOfLines.Add("<div style='width: 80%; max-width: 500px;'>");

            listOfLines.AddRange(CreateScenarioStatusChartPercentage(numberOfTests, numberOfPassed));
            listOfLines.AddRange(CreateScenarioStatusChartGraphics(numberOfTests, numberOfPassed, numberOfSkipped, numberOfInconclusive, numberOfFailed));

            listOfLines.Add("</div>");

            return listOfLines;
        }

        internal static List<string> CreateScenarioStatusChartPercentage(int numberOfTests, int numberOfPassed)
        {
            var listOfLines = new List<string>();

            var procentOfPassed = numberOfPassed / (float)numberOfTests * 100;
            if (float.IsNaN(procentOfPassed))
                procentOfPassed = 0;

            listOfLines.Add("<div>");
            listOfLines.Add($"<span class='chart-percentage'>{procentOfPassed.ToString("0")}%<br></span>");
            listOfLines.Add("<span class='chart-status'>Passed</span>");
            listOfLines.Add("</div>");

            return listOfLines;
        }

        internal static List<string> CreateScenarioStatusChartGraphics(int numberOfTotal, int numberOfPassed, int numberOfSkipped, int numberOfInconclusive, int numberOfFailed)
        {
            var numberOfPassedPercent = 100.0f / numberOfTotal * numberOfPassed;
            var numberOfSkippedPercent = 100.0f / numberOfTotal * numberOfSkipped;
            var numberOfInconclusivePercent = 100.0f / numberOfTotal * numberOfInconclusive;
            var numberOfFailedPercent = 100.0f / numberOfTotal * numberOfFailed;

            if (numberOfSkippedPercent > 0.0 && numberOfSkippedPercent < 1.0f)
                numberOfSkippedPercent += 1.0f;

            if (numberOfInconclusivePercent > 0.0 && numberOfInconclusivePercent < 1.0f)
                numberOfInconclusivePercent += 1.0f;

            if (numberOfFailedPercent > 0.0 && numberOfFailedPercent < 1.0f)
                numberOfFailedPercent += 1.0f;

            var listOfLines = new List<string>();
            listOfLines.Add($"<p style='color: gray; font-style: italic; margin: 8px; '>{numberOfPassed} Passed, {numberOfInconclusive} Inconclusive, {numberOfFailed} Failed, {numberOfSkipped} Skipped Scenarios</p>");
            listOfLines.Add($"<div class='bgcolor-skipped' style='width: 100%; height: 1em;'>");
            listOfLines.Add($"<div class='bgcolor-passed' style='width: {(int)numberOfPassedPercent}%; height: 1em; float: left'></div>");
            listOfLines.Add($"<div class='bgcolor-inconclusive' style='width: {(int)numberOfInconclusivePercent}%; height: 1em; float: left'></div>");
            listOfLines.Add($"<div class='bgcolor-failed' style='width: {(int)numberOfFailedPercent}%; height: 1em; float: left'></div>");
            listOfLines.Add("</div>");

            return listOfLines;
        }

        private IEnumerable<string> GenerateScenarioPreFilters(TestExecutionContext executionContext)
        {
            List<string> listOfLines = new List<string>();

            listOfLines.Add("<!-- Features PreFilters Section -->");
            listOfLines.Add("<div>");
            listOfLines.Add("<button title='Passed' class='color-passed' onclick='presetScenarios(\"passed\")'>Passed</button>");
            listOfLines.Add("<button title='Inconclusive' class='color-inconclusive' onclick='presetScenarios(\"inconclusive\")'>Inconclusive</button>");
            listOfLines.Add("<button title='Failed' class='color-failed' onclick='presetScenarios(\"failed\")'>Failed</button>");
            listOfLines.Add("<button title='Skipped' class='color-skipped' onclick='presetScenarios(\"skipped\")'>Skipped</button>");
            listOfLines.Add("<button title='Clear' class='color-clear' onclick='presetScenarios(\"\")'>Clear</button>");
            listOfLines.Add("</div>");

            return listOfLines;
        }

        private IEnumerable<string> GenerateScenarioFilter(TestExecutionContext executionContext)
        {
            List<string> listOfLines = new List<string>();

            listOfLines.Add("<!-- Features Filter Section -->");
            listOfLines.Add("<div>");
            listOfLines.Add("<input class='filter' onkeyup='filterScenarios()' id='search-filter' type='text' placeholder='Filter by Text'>");
            listOfLines.Add("</div>");

            return listOfLines;
        }

        private List<string> GenerateScenarioTableGrid(TestExecutionContext executionContext)
        {
            List<string> listOfLines = new List<string>();

            listOfLines.Add("<!-- Scenarios Table Section -->");
            listOfLines.Add("<table class='grid'>");
            listOfLines.Add("<thead>");
            listOfLines.Add("<tr>");
            listOfLines.Add("<th>Feature</th>");
            listOfLines.Add("<th>Scenario</th>");
            listOfLines.Add("<th>Status</th>");
            //listOfLines.Add("<th></th>"); Status Dot
            listOfLines.Add("</tr>");
            listOfLines.Add("</thead>");
            listOfLines.Add("<tbody id='search-list'>");

            foreach (var feature in executionContext.Features)
            {
                foreach (var scenario in feature.Scenarios)
                {
                    var status = scenario.GetStatus().ToLower();

                    listOfLines.Add($"<tr tags='{scenario.GetTags()}' onclick=\"loadScenario('{scenario.Id}');\">");
                    listOfLines.Add($"<td>" + feature.Title + "</td>");
                    listOfLines.Add($"<td><a href='#'>" + scenario.Title + "</a></td>");
                    listOfLines.Add($"<td>" + scenario.GetStatus() + "</td>");
                    //listOfLines.Add($"<td><span class='status-dot bgcolor-{status}'></span></td>");
                    listOfLines.Add($"</tr>");
                }
            }

            listOfLines.Add("</tbody>");
            listOfLines.Add("</table>");

            return listOfLines;
        }

        private List<string> GenerateFooter()
        {
            List<string> listOfLines = new List<string>();

            listOfLines.Add("<!-- Footer Section -->");
            listOfLines.Add("<footer>");
            listOfLines.Add("©2025 Expressium All Rights Reserved");
            listOfLines.Add("</footer>");

            return listOfLines;
        }

        private List<string> GenerateData(TestExecutionContext executionContext)
        {
            var listOfLines = new List<string>();

            foreach (var feature in executionContext.Features)
            {
                foreach (var scenario in feature.Scenarios)
                {
                    listOfLines.Add("<!-- Scenario Data Section -->");
                    listOfLines.Add($"<div class='data-item' id='{scenario.Id}'>");

                    listOfLines.AddRange(GenerateDataFeatureInformation(feature));
                    listOfLines.AddRange(GenerateDataScenarioInformation(scenario));

                    listOfLines.Add("</div>");
                }
            }

            return listOfLines;
        }

        private List<string> GenerateDataFeatureInformation(TestExecutionFeature feature)
        {
            var listOfLines = new List<string>();

            listOfLines.Add("<div>");

            if (feature.IsTagged())
            {
                listOfLines.Add("<span class='tag-names'>" + feature.GetTags() + "</span>");
                listOfLines.Add("<br />");
            }

            listOfLines.Add("<span class='feature-name'>Feature: " + feature.Title + "</span>");
            listOfLines.Add("</div>");

            listOfLines.Add("<div class='userstory'>");
            var listOfDescription = feature.Description.Trim().Split("\n");
            foreach (var line in listOfDescription)
                listOfLines.Add("<span>" + line.Trim() + "</span><br />");
            listOfLines.Add("<br />");
            listOfLines.Add("</div>");

            return listOfLines;
        }

        private List<string> GenerateDataScenarioInformation(TestExecutionScenario scenario)
        {
            var listOfLines = new List<string>();

            bool scenarioSplitter = false;
            foreach (var example in scenario.Examples)
            {
                if (scenarioSplitter)
                    listOfLines.Add("<hr>");
                scenarioSplitter = true;

                listOfLines.Add("<div>");
                if (scenario.IsTagged())
                {
                    listOfLines.Add("<span class='tag-names'>" + scenario.GetTags() + "</span>");
                    listOfLines.Add("<br />");
                }
                listOfLines.Add("</div>");

                var status = example.GetStatus().ToLower();

                var scenarioKeyword = "Scenario:";
                if (scenario.Examples.Count > 1)
                    scenarioKeyword = "Scenario Outline:";

                listOfLines.Add("<table>");
                listOfLines.Add("<tbody>");

                listOfLines.Add("<tr>");
                listOfLines.Add("<td>");

                //if (example.IsPassed())
                //    listOfLines.Add($"<span class='color-{status}'>&check;</span>");
                //else
                //    listOfLines.Add($"<span class='color-{status}'>&#x2718;</span>");
                listOfLines.Add($"<span class='status-dot bgcolor-{status}'></span>");

                listOfLines.Add("</td>");
                listOfLines.Add("<td colspan='2'>");
                listOfLines.Add("<span class='scenario-name'>" + scenarioKeyword + " " + scenario.Title + " </span>");
                listOfLines.Add("<span class='duration'>&nbsp;" + example.GetDuration() + "</span>");
                listOfLines.Add("</td>");
                listOfLines.Add("</tr>");

                foreach (var step in example.Steps)
                {
                    listOfLines.Add($"<tr>");
                    listOfLines.Add($"<td></td>");
                    listOfLines.Add($"<td colspan='2'><span class='step-keyword'>" + step.Type + "</span> " + step.Text + "</td>");
                    listOfLines.Add($"</tr>");
                }

                if (scenario.Examples.Count > 1)
                {
                    listOfLines.Add($"<tr>");
                    listOfLines.Add($"<td></td>");
                    listOfLines.Add($"<td colspan='2' class='examples'>&nbsp;<b>Examples:</b></td>");
                    listOfLines.Add($"</tr>");

                    listOfLines.Add($"<tr>");
                    listOfLines.Add($"<td></td>");
                    listOfLines.Add($"<td colspan='2' class='examples'>");
                    listOfLines.Add("<table>");
                    listOfLines.Add("<tbody>");

                    listOfLines.Add($"<tr>");
                    foreach (var argument in example.Arguments)
                        listOfLines.Add($"<td><i>| " + argument.Name + "</i></td>");
                    listOfLines.Add($"<td>|</td>");
                    listOfLines.Add($"</tr>");

                    listOfLines.Add($"<tr>");
                    foreach (var argument in example.Arguments)
                        listOfLines.Add($"<td>| " + argument.Value + "</td>");
                    listOfLines.Add($"<td>|</td>");
                    listOfLines.Add($"</tr>");

                    listOfLines.Add("</tbody>");
                    listOfLines.Add("</table>");
                    listOfLines.Add($"</td>");
                    listOfLines.Add($"</tr>");
                }

                string message = null;
                if (example.Error != null)
                    message = example.Error;
                else if (example.IsStepPending())
                    message = "Pending Step Definition";
                else if (example.IsStepUndefined())
                    message = "Undefined Step Definition";
                else if (example.IsStepBindingError())
                    message = "Binding Error Step Definition";
                else
                {
                }

                if (message != null)
                {
                    listOfLines.Add("<tr><td></td></tr>");
                    listOfLines.Add("<tr>");
                    listOfLines.Add("<td colspan='3' class='step-failed'>" + message + "</td>");
                    listOfLines.Add("</tr>");
                }

                listOfLines.Add("</tbody>");
                listOfLines.Add("</table>");

                listOfLines.Add("<br />");
                listOfLines.AddRange(GenerateDataAttachments(example));
            }

            return listOfLines;
        }

        private List<string> GenerateDataAttachments(TestExecutionExample example)
        {
            var listOfLines = new List<string>();

            if (example.Attachments.Count > 0)
            {
                listOfLines.Add("<div class='attachments'>");
                listOfLines.Add("<span class='scenario-name'>Attachments:</span>");
                listOfLines.Add("<ul>");

                foreach (var attachment in example.Attachments)
                {
                    var filePath = Path.GetFileName(attachment);
                    listOfLines.Add($"<li><a target='_blank' href='./Attachments/{filePath}'>{filePath}</a></li>");
                }

                listOfLines.Add("</ul>");
                listOfLines.Add("</div>");
            }

            return listOfLines;
        }

        private static void SaveListOfLinesToFile(string filePath, List<string> listOfLines)
        {
            var content = string.Join(Environment.NewLine, listOfLines);

            var htmlParser = new HtmlParser();
            var htmlDocument = htmlParser.ParseDocument(content);

            using (var streamWriter = new StringWriter())
            {
                htmlDocument.ToHtml(streamWriter, new PrettyMarkupFormatter
                {
                    Indentation = "\t",
                    NewLine = "\n"
                });

                File.WriteAllText(filePath, streamWriter.ToString());
            }
        }
    }
}
