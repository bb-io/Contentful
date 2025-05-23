﻿using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class SearchWorkflowsRequest : EnvironmentIdentifier
{
    [Display("Workflow definition ID", Description = "The ID of the workflow definition to search for"), DataSource(typeof(WorkflowDefinitionDataHandler))]
    public string? WorkflowDefinitionId { get; set; }

    [Display("Current step names", Description = "Filter by current step names")]
    public IEnumerable<string>? CurrentStepNames { get; set; }
}