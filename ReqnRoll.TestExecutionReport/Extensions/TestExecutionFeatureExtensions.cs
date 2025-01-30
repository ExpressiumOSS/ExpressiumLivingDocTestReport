﻿using ReqnRoll.TestExecution;
using System.Linq;

namespace ReqnRoll.TestExecutionReport.Extensions
{
    public static class TestExecutionFeatureExtensions
    {
        public static bool IsTagged(this TestExecutionFeature feature)
        {
            return !string.IsNullOrEmpty(feature.Tags);
        }

        public static void OrderByTags(this TestExecutionFeature feature)
        {
            feature.Scenarios = feature.Scenarios.OrderBy(x => x.Tags).ToList();
        }

        public static string GetTags(this TestExecutionFeature feature)
        {
            return feature.Tags.FormatTags();
        }

        public static string GetStatus(this TestExecutionFeature feature)
        {
            foreach (var scenario in feature.Scenarios)
            {
                if (scenario.IsFailed())
                    return ReportStatuses.Failed.ToString();
            }

            foreach (var scenario in feature.Scenarios)
            {
                if (scenario.IsInconclusive())
                    return ReportStatuses.Inconclusive.ToString();
            }

            foreach (var scenario in feature.Scenarios)
            {
                if (scenario.IsSkipped())
                    return ReportStatuses.Skipped.ToString();
            }

            foreach (var scenario in feature.Scenarios)
            {
                if (scenario.IsPassed())
                    return ReportStatuses.Passed.ToString();
            }

            return ReportStatuses.Undefined.ToString();
        }

        public static int GetNumberOfPassed(this TestExecutionFeature feature)
        {
            return feature.Scenarios.Count(scenario => scenario.IsPassed());
        }

        public static int GetNumberOfFailed(this TestExecutionFeature feature)
        {
            return feature.Scenarios.Count(scenario => scenario.IsFailed());
        }

        public static int GetNumberOfInconclusive(this TestExecutionFeature feature)
        {
            return feature.Scenarios.Count(scenario => scenario.IsInconclusive());
        }

        public static int GetNumberOfSkipped(this TestExecutionFeature feature)
        {
            return feature.Scenarios.Count(scenario => scenario.IsSkipped());
        }

        public static int GetNumberOfTests(this TestExecutionFeature feature)
        {
            return feature.Scenarios.Count();
        }
    }
}
