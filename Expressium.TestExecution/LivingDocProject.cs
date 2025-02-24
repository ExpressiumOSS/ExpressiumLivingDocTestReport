﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Expressium.TestExecution
{
    public class LivingDocProject
    {
        public string Title { get; set; }
        public DateTime ExecutionTime { get; set; }
        public TimeSpan Duration { get; set; }

        public List<LivingDocFeature> Features { get; set; }

        public LivingDocProject()
        {
            Duration = new TimeSpan();
            Features = new List<LivingDocFeature>();
        }

        public bool IsFeatureAdded(string title)
        {
            return Features.Any(m => m.Name == title);
        }

        public LivingDocFeature GetFeature(string title)
        {
            return Features.Find(x => x.Name == title);
        }

        public int GetNumberOfPassedFeatures()
        {
            return Features.Count(feature => feature.GetStatus() == LivingDocStatuses.Passed.ToString());
        }

        public int GetNumberOfIncompleteFeatures()
        {
            return Features.Count(feature => feature.GetStatus() == LivingDocStatuses.Incomplete.ToString());
        }

        public int GetNumberOfFailedFeatures()
        {
            return Features.Count(feature => feature.GetStatus() == LivingDocStatuses.Failed.ToString());
        }

        public int GetNumberOfSkippedFeatures()
        {
            return Features.Count(feature => feature.GetStatus() == LivingDocStatuses.Skipped.ToString());
        }

        public int GetNumberOfTestsFeature()
        {
            return Features.Count;
        }

        public int GetNumberOfPassed()
        {
            return Features
                .SelectMany(feature => feature.Scenarios)
                .Count(scenario => scenario.IsPassed());
        }

        public int GetNumberOfIncomplete()
        {
            return Features
                .SelectMany(feature => feature.Scenarios)
                .Count(scenario => scenario.IsIncomplete());
        }

        public int GetNumberOfFailed()
        {
            return Features
                .SelectMany(feature => feature.Scenarios)
                .Count(scenario => scenario.IsFailed());
        }

        public int GetNumberOfSkipped()
        {
            return Features
                .SelectMany(feature => feature.Scenarios)
                .Count(scenario => scenario.IsSkipped());
        }

        public int GetNumberOfTests()
        {
            return Features
                .SelectMany(feature => feature.Scenarios)
                .Count();
        }

        public string GetExecutionTime()
        {
            return ExecutionTime.ToString("ddd dd. MMM yyyy HH':'mm':'ss \"GMT\"z");
        }

        public string GetDuration()
        {
            if (Duration.Minutes > 0)
                return $"{Duration.Minutes}min {Duration.Seconds}s";

            return $"{Duration.Seconds}s {Duration.Milliseconds.ToString("D3")}ms";
        }

        public double GetPercentageOfPassed()
        {
            return Math.Round(100.0f / GetNumberOfTests() * GetNumberOfPassed());
        }

        public double GetPercentageOfIncomplete()
        {
            return Math.Round(100.0f / GetNumberOfTests() * GetNumberOfIncomplete());
        }

        public double GetPercentageOfFailed()
        {
            return Math.Round(100.0f / GetNumberOfTests() * GetNumberOfFailed());
        }

        public double GetPercentageOfSkipped()
        {
            return Math.Round(100.0f / GetNumberOfTests() * GetNumberOfSkipped());
        }

        public LivingDocFolder GetListOfFolderNodes()
        {
            var listOfFolders = new List<string>();

            foreach (var feature in Features)
            {
                if (!listOfFolders.Contains(feature.Uri))
                    listOfFolders.Add(feature.Uri);
            }

            return BuildTree(listOfFolders);
        }

        //public static List<string> GetListOfFolders(this LivingDocProject project)
        //{
        //    var listOfFolders = new List<string>();

        //    foreach (var feature in project.Features)
        //    {
        //        if (!listOfFolders.Contains(feature.Uri))
        //            listOfFolders.Add(feature.Uri);
        //    }

        //    return listOfFolders;
        //}

        public LivingDocFolder BuildTree(List<string> tokens)
        {
            var root = new LivingDocFolder("Root");

            foreach (var token in tokens)
            {
                var parts = token.Split('/');
                var current = root;

                foreach (var part in parts)
                {
                    if (!current.Children.ContainsKey(part))
                        current.Children[part] = new LivingDocFolder(part);

                    current = current.Children[part];
                }
            }

            return root;
        }
    }
}
