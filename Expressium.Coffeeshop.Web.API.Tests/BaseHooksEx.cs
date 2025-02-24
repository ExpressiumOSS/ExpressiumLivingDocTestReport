﻿using Expressium.TestExecution;
using Reqnroll;
using System;
using System.IO;

namespace Expressium.Coffeeshop.Web.API.Tests
{
    public partial class BaseHooks
    {
        private static LivingDocProject livingDocProject;
        private static string outputFileName = "TestExecution.json";

        private LivingDocScenario livingDocScenario;

        private static void InitializeTestExecution()
        {
            livingDocProject = new LivingDocProject();
            livingDocProject.Title = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            livingDocProject.ExecutionTime = DateTime.UtcNow.ToLocalTime();
            livingDocProject.StartTime = DateTime.Now;
            livingDocProject.EndTime = DateTime.Now;

            System.Threading.Thread.Sleep(1000);
        }

        private static void FinalizeTestExecution()
        {
            livingDocProject.EndTime = DateTime.Now;

            LivingDocUtilities.SerializeAsJson(Path.Combine(Directory.GetCurrentDirectory(), outputFileName), livingDocProject);
        }

        private void AddTestExecutionBeforeScenario()
        {
            if (!livingDocProject.IsFeatureAdded(featureContext.FeatureInfo.Title))
            {
                var livingDocFeature = new LivingDocFeature();

                foreach (var tag in featureContext.FeatureInfo.Tags)
                    livingDocFeature.Tags.Add(new LivingDocTag() { Name = "@" + tag });

                livingDocFeature.Name = featureContext.FeatureInfo.Title;
                livingDocFeature.Description = featureContext.FeatureInfo.Description;
                livingDocFeature.Uri = featureContext.FeatureInfo.FolderPath;
                livingDocProject.Features.Add(livingDocFeature);
            }

            if (livingDocProject.IsFeatureAdded(featureContext.FeatureInfo.Title))
            {
                livingDocScenario = new LivingDocScenario();

                foreach (var tag in scenarioContext.ScenarioInfo.Tags)
                    livingDocScenario.Tags.Add(new LivingDocTag() { Name = "@" + tag });

                livingDocScenario.Name = scenarioContext.ScenarioInfo.Title;
                livingDocScenario.Description = scenarioContext.ScenarioInfo.Description;

                var livingDocExample = new LivingDocExample();
                livingDocExample.StartTime = DateTime.Now;
                livingDocExample.EndTime = DateTime.Now;
                livingDocScenario.Examples.Add(livingDocExample);

                if (scenarioContext.ScenarioInfo.Arguments.Count > 0)
                {
                    foreach (var key in scenarioContext.ScenarioInfo.Arguments.Keys)
                        livingDocScenario.Examples[0].TableHeader.Cells.Add(new LivingDocTableCell() { Value = key.ToString() });

                    var livingDocTableRow = new LivingDocTableRow();
                    foreach (var value in scenarioContext.ScenarioInfo.Arguments.Values)
                        livingDocTableRow.Cells.Add(new LivingDocTableCell() { Value = value.ToString() });
                    livingDocScenario.Examples[0].TableBody.Add(livingDocTableRow);
                }
            }
        }

        private void AddTestExecutionAfterScenario()
        {
            if (livingDocScenario != null)
            {
                livingDocScenario.Examples[0].Stacktrace = scenarioContext.TestError?.StackTrace;
                livingDocScenario.Examples[0].EndTime = DateTime.Now;

                if (livingDocProject.IsFeatureAdded(featureContext.FeatureInfo.Title))
                {
                    var livingDocFeature = livingDocProject.GetFeature(featureContext.FeatureInfo.Title);

                    if (livingDocFeature.IsScenarioAdded(scenarioContext.ScenarioInfo.Title))
                    {
                        var testExecutionScenarioParent = livingDocFeature.GetScenario(scenarioContext.ScenarioInfo.Title);
                        testExecutionScenarioParent.Examples.Add(livingDocScenario.Examples[0]);
                    }
                    else
                    {
                        livingDocFeature.Scenarios.Add(livingDocScenario);
                    }
                }
            }
        }

        private void AddTestExecutionScenarioAttachment(string filePath)
        {
            if (livingDocScenario != null)
            {
                livingDocScenario.Examples[0].Attachments.Add(filePath);
            }
        }

        private void AddTestExecutionAfterStep()
        {
            if (livingDocScenario != null)
            {
                var livingDocStep = new LivingDocStep();
                livingDocStep.Keyword = scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
                livingDocStep.Name = scenarioContext.StepContext.StepInfo.Text;
                livingDocStep.Status = scenarioContext.StepContext.Status.ToString();
                livingDocStep.Message = scenarioContext.StepContext.TestError?.ToString();

                if (livingDocStep.Message == null)
                    livingDocStep.Message = scenarioContext.TestError?.Message;

                // Mapping to LivingDoc status
                if (livingDocStep.Status == ScenarioExecutionStatus.OK.ToString())
                    livingDocStep.Status = TestExecutionStatuses.Passed.ToString();
                else if (livingDocStep.Status == ScenarioExecutionStatus.StepDefinitionPending.ToString())
                    livingDocStep.Status = TestExecutionStatuses.Pending.ToString();
                else if (livingDocStep.Status == ScenarioExecutionStatus.UndefinedStep.ToString())
                    livingDocStep.Status = TestExecutionStatuses.Undefined.ToString();
                else if (livingDocStep.Status == ScenarioExecutionStatus.BindingError.ToString())
                    livingDocStep.Status = TestExecutionStatuses.Ambiguous.ToString();
                else if (livingDocStep.Status == ScenarioExecutionStatus.TestError.ToString())
                    livingDocStep.Status = TestExecutionStatuses.Failed.ToString();
                else if (livingDocStep.Status == ScenarioExecutionStatus.Skipped.ToString())
                    livingDocStep.Status = TestExecutionStatuses.Skipped.ToString();
                else
                {
                }

                if (livingDocStep.Message == null)
                {
                    if (livingDocStep.IsPending())
                        livingDocStep.Message = "Pending Step Definition";
                    else if (livingDocStep.IsUndefined())
                        livingDocStep.Message = "Undefined Step Definition";
                    else if (livingDocStep.IsAmbiguous())
                        livingDocStep.Message = "Ambiguous Step Definition";
                    else
                    {
                    }
                }

                if (scenarioContext.StepContext.StepInfo.Table != null)
                {
                    var livingDocTableHeaderRow = new LivingDocTableRow();
                    foreach (var header in scenarioContext.StepContext.StepInfo.Table.Header)
                        livingDocTableHeaderRow.Cells.Add(new LivingDocTableCell() { Value = header });
                    livingDocStep.DataTable.Rows.Add(livingDocTableHeaderRow);

                    var testExecutionTableRow = new LivingDocTableRow();
                    foreach (var row in scenarioContext.StepContext.StepInfo.Table.Rows)
                    {
                        foreach (var value in row.Values)
                            testExecutionTableRow.Cells.Add(new LivingDocTableCell() { Value = value });
                    }
                    livingDocStep.DataTable.Rows.Add(testExecutionTableRow);
                }

                livingDocScenario.Examples[0].Steps.Add(livingDocStep);
            }
        }
    }
}
