﻿using System;
using System.Collections.Generic;

namespace Expressium.TestExecution
{
    public class TestExecutionScenario
    {
        public int Index { get; set; }
        public string Id { get; set; }
        public List<TestExecutionTag> Tags { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Keyword { get; set; }
        public int Line { get; set; }
        public string Type { get; set; }

        public List<TestExecutionExample> Examples { get; set; }

        public TestExecutionScenario()
        {
            Index = 0;
            Tags = new List<TestExecutionTag>();
            Examples = new List<TestExecutionExample>();
        }
    }
}
