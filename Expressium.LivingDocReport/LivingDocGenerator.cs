﻿using AngleSharp.Html;
using AngleSharp.Html.Parser;
using Expressium.LivingDoc;
using Expressium.LivingDocReport.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Expressium.LivingDocReport
{
    public class LivingDocGenerator
    {
        private string inputPath;
        private string outputPath;

        public LivingDocGenerator(string inputPath, string outputPath)
        {
            this.inputPath = inputPath;
            this.outputPath = outputPath;
        }

        public void Execute()
        {
            Console.WriteLine("");
            Console.WriteLine("Generating LivingDoc Report...");
            Console.WriteLine("InputPath: " + inputPath);
            Console.WriteLine("OutputPath: " + outputPath);
            Console.WriteLine("");

            var project = ParseJsonFile();
            CreateOutputDirectories();
            CopyOutputAttachments(project);
            AssignUniqueIdentifier(project);
            GenerateHtmlReport(project);

            Console.WriteLine("Generating LivingDoc Report Completed");
            Console.WriteLine("");
        }

        internal LivingDocProject ParseJsonFile()
        {
            Console.WriteLine("Parse JSON File...");
            return LivingDocUtilities.DeserializeAsJson<LivingDocProject>(inputPath);
        }

        internal void CreateOutputDirectories()
        {
            Console.WriteLine("Create Output Directories...");

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
            Directory.CreateDirectory(outputPath);
            Directory.CreateDirectory(Path.Combine(outputPath, "Attachments"));
        }

        internal void CopyOutputAttachments(LivingDocProject project)
        {
            Console.WriteLine("Copy Output Attachments...");

            foreach (var feature in project.Features)
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
        }

        internal void AssignUniqueIdentifier(LivingDocProject project)
        {
            Console.WriteLine("Assign Unique Identifier...");

            int indexId = 1;
            foreach (var feature in project.Features)
            {
                feature.Id = Guid.NewGuid().ToString();

                foreach (var scenario in feature.Scenarios)
                {
                    scenario.Id = Guid.NewGuid().ToString();
                    scenario.Index = indexId++;
                }
            }
        }

        internal void GenerateHtmlReport(LivingDocProject project)
        {
            Console.WriteLine("Generating  HTML Report...");

            var listOfLines = new List<string>();

            listOfLines.AddRange(GenerateHtmlHeader());
            listOfLines.AddRange(GenerateHead());
            listOfLines.AddRange(GenerateBody(project));
            listOfLines.AddRange(GenerateHtmlFooter());

            var htmlFilePath = Path.Combine(outputPath, "LivingDoc.html");
            SaveListOfLinesToFile(htmlFilePath, listOfLines);
        }

        internal List<string> GenerateHtmlHeader()
        {
            var listOfLines = new List<string>();

            listOfLines.Add("<!DOCTYPE html>");
            listOfLines.Add("<html>");

            return listOfLines;
        }

        internal List<string> GenerateHead()
        {
            var listOfLines = new List<string>();

            listOfLines.Add($"<head>");

            listOfLines.AddRange(GenerateHeads());
            listOfLines.AddRange(GenerateStyles());
            listOfLines.AddRange(GenerateScripts());

            listOfLines.Add($"</head>");

            return listOfLines;
        }

        internal List<string> GenerateHeads()
        {
            return Resources.Heads.Split(Environment.NewLine).ToList();
        }

        internal List<string> GenerateStyles()
        {
            return Resources.Styles.Split(Environment.NewLine).ToList();
        }

        internal List<string> GenerateScripts()
        {
            return Resources.Scripts.Split(Environment.NewLine).ToList();
        }

        internal List<string> GenerateBody(LivingDocProject project)
        {
            var listOfLines = new List<string>();

            var bodyGenerator = new LivingDocBodyGenerator();
            listOfLines.AddRange(bodyGenerator.GenerateBody(project));

            return listOfLines;
        }

        internal List<string> GenerateHtmlFooter()
        {
            var listOfLines = new List<string>();

            listOfLines.Add("</html>");

            return listOfLines;
        }

        internal static void SaveListOfLinesToFile(string filePath, List<string> listOfLines)
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
