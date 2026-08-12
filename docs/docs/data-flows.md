# Data Onboarding & Processing flow Endpoints

The data onboarding and processing flows are primarily managed through respective DataGEMS conmponents such as the [DataGEMS Data Model Management](https://datagems-eosc.github.io/data-model-management), the [DataGEMS Data Workflow Orchestrator](https://datagems-eosc.github.io/dg-data-workflow), the [Dataset Profiler](https://datagems-eosc.github.io/dg-dataset-profiler), and others that can be found in the [DataGEMS documentation](https://datagems-eosc.github.io/).

The Gateway API provides entry points for some facets of the onboarding and processing phases as required and propagates processing requests to the underpinning components.

## Workflow model

A workflow execution is represented by a `WorkflowProcess`.

Each process contains one or more `WorkflowProcessStep` entries representing the configured processing steps. While the underlying workflow is running, task-instance callbacks update the corresponding workflow step so that the Gateway can expose the current processing state.

The normal processing chain is:

```text
Dataset Onboarding
        |
        v
Dataset Profiling
        |
        v
Dataset Packaging
        |
        v
Recommendation Registration
        |
        v
Cross-Dataset-Discovery Ingestion
```

The transition from one stage to the next is handled by the Gateway after the current stage has completed successfully. The client does not need to wait for a DAG to finish and manually invoke the next stage.

Individual stage endpoints still exist and may be used when a specific workflow stage needs to be started directly.

## Making data available to the platform

When it comes to making data available to the platform so that they can be ingested as datasets, there are various methods supported:

- Upload data files directly through the Gateway API
- Reference data that are publicly available through http / ftp
- Reference data previously staged to the platform as raw files
- Reference data previously staged to the platform as relations database

The first two methods are made available to users that have proper authorization to be used when registering a new dataset. The later cases are restricted to administrator users.

## Uploading data

A user with the approriate authorization can upload data that can then be used to register a dataset.

### Allowed file extensions

The `/api/storage/upload/allowed-extension` endpoint provides the file extensions that are allowed to be apploaded

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/storage/upload/allowed-extension' \
--header 'Authorization: Bearer ey...Bg'
```

This will provide an answer like the following:

```json
[".csv", ".xlsx", ".txt", ".pdf", ".png", ".jpeg", ".jpg", ".md"]
```

### Uploading files

The `/api/storage/upload/dataset` endpoint provides a way to stage data files in a controlled storage location so that they can then be used for dataset ingestion.

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/storage/upload/dataset' \
--header 'Authorization: Bearer eyJ...zg' \
--form 'file1=@"/path/to/file/test1.csv"' \
--form 'file2=@"/path/to/file/test2.csv"'
```

This will provide an answer like the following:

```json
[
  "/path/to/staged/test1.8fd16d58ce0c45c99532060bf61ecbd4.csv",
  "/path/to/staged/test2.d7ccbfdd929f4158ab7e446a65ebc726.csv"
]
```

The paths of the staged datasets need to be preserved by the caller in order to use them in the next steps that will register the dataset pointing to the staged files.

## Starting the onboarding workflow

Dataset onboarding is started through:

```text
POST /api/workflow-process/onboard
```

The request contains the dataset metadata and its data locations.

The endpoint also accepts the `f` query parameter used by the Gateway field-set mechanism to select the fields returned in the `WorkflowProcess` response.

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/workflow-process/onboard?f=<workflow-process-fields>' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
    "Name": "dataset-test-A",
    "Description": "dataset-test-A",
    "License": "dataset-test-A",
    "Url": "https://dataset-test-A.gr",
    "Headline": "dataset-test-A",
    "Keywords": ["dataset-test-A"],
    "FieldOfScience": ["dataset-test-A"],
    "Language": ["dataset-test-A"],
    "Country": ["dataset-test-A"],
    "DatePublished": "2025-10-22",
    "CiteAs": "dataset-test-A",
    "Doi": "dataset-test-A",
    "DataLocations": [
        {
            "Kind": 0,
            "Location": "/path/to/staged/test1.8fd16d58ce0c45c99532060bf61ecbd4.csv"
        },
    ]
}'
```

The response is a `WorkflowProcess`, not a dataset UUID. The exact response fields depend on the requested field set.

The returned workflow process can be used to monitor the execution through the workflow-process query and lookup endpoints described below.

## Data locations

For the `DataLocations` property, the supported values include the following. Refer to the OpenAPI reference for the current list.

- `File = 0` — Data is stored in a local or network file-system path.
- `Http = 1` — Data is accessible through an HTTP or HTTPS endpoint.
- `Ftp = 2` — Data is accessible through an FTP or FTPS server.
- `Remote = 3` — Reserved but currently not used.
- `Staged = 4` — The dataset is already staged.
- `Database = 5` — The dataset is stored in a database.

For uploaded, downloaded, or otherwise staged raw files, the file-system-based processing path is used. Relational database processing is intended for datasets that have already been staged in a relational database through an administrative/offline action.

## Automatic workflow progression

Once a workflow has been started, the caller does not need to manually trigger every subsequent DAG.

Task callbacks running as part of the workflow execution report task-instance state to the Gateway. The Gateway persists the state of the corresponding `WorkflowProcessStep`. When the relevant step or stage completes successfully, the corresponding finalize callback is used to finalize the stage and continue the configured processing chain.

Conceptually, the execution is:

```text
Client
  |
  | POST /api/workflow-process/onboard
  v
Gateway
  |
  | starts onboarding DAG
  v
Workflow Orchestrator
  |
  | task callbacks
  |----> POST /api/workflow-process/step/persist
  |
  | onboarding completed successfully
  |----> POST /api/workflow-process/step/finalize-onboarding
  v
Gateway
  |
  | starts profiling DAG
  v
Workflow Orchestrator
  |
  | task callbacks / finalization
  v
Gateway
  |
  | starts next configured stage
  v
Packaging -> Recommendation Registration -> CDD Ingestion
```

If a task does not complete successfully, the successful-completion transition is not performed and the next workflow stage is not automatically started.

The process and step lookup endpoints can be used by clients to observe the resulting state.

## Monitoring workflow processes

### Query workflow processes

```text
POST /api/workflow-process/query
```

Queries workflow processes visible to the authenticated caller.

The request body is a `WorkflowProcessLookup`, allowing predicates, projection, and other supported lookup options.

```bash
curl --location '<base url>/api/workflow-process/query' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
    {
        "Ids": [<ids>],
        "UserIds": [<userIds>],
        "DatasetIds": [<datasetIds>],
        "Project": {"Fields": ["Id", "Status", "Steps.WorkflowTaskInstanceDetails", "Steps.Id", "Steps.Status"]}
    }
}'
```

The response is:

```text
QueryResult<WorkflowProcess>
```

and contains the matching workflow processes together with the result count.

Refer to the [OpenAPI Reference](openapi.md) for the exact `WorkflowProcessLookup` schema and supported projection syntax.

## Get a workflow process by ID

```text
GET /api/workflow-process/{id}
```

Returns a specific workflow process.

The `f` query parameter selects the fields included in the returned model.

```bash
curl --location '<base url>/api/workflow-process/<workflow-process-id>?f=<workflow-process-field>' \
--header 'Authorization: Bearer ey...aQ'
```

If the workflow process cannot be found, the endpoint returns `404`.

---

## Monitoring workflow process steps

### Query workflow process steps

```text
POST /api/workflow-process/step/query
```

Queries workflow process steps visible to the authenticated caller.

The request body is a `WorkflowProcessStepLookup`.

```bash
curl --location '<base url>/api/workflow-process/step/query' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
    "...": "WorkflowProcessStepLookup predicates and projection"
}'
```

The response is:

```text
QueryResult<WorkflowProcessStep>
```

Use this endpoint when a client needs more detailed visibility into the individual steps of a workflow process.

### Get a workflow process step by ID

```text
GET /api/workflow-process/step/{id}
```

Returns a specific workflow process step.

The `f` query parameter selects the fields included in the returned model.

```bash
curl --location '<base url>/api/workflow-process/step/<workflow-process-step-id>?f=<workflow-process-step-field>' \
--header 'Authorization: Bearer ey...aQ'
```

If the workflow process step cannot be found, the endpoint returns `404`.

## Starting individual processing stages

The Gateway still exposes explicit entry points for the individual stages of dataset processing.

These endpoints are useful when a caller intentionally needs to start a specific workflow stage rather than relying on the normal automatic end-to-end progression.

All of these endpoints return a `WorkflowProcess` and accept an `f` query parameter controlling the returned fields.

### Profiling

```text
POST /api/workflow-process/profile
```

Starts a dataset profiling workflow.

Profiling may be executed independently of the original onboarding operation.

```bash
curl --location '<base url>/api/workflow-process/profile?f=<workflow-process-fields>' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
    "id": "<dataset uuid>",
    "dataStoreKind": 0,
	"DatabaseName": null
}'
```

For `DataStoreKind`, supported values include:

- `FileSystem = 0` — The dataset is stored in a filesystem.
- `RelationalDatabase = 1` — The dataset is stored in a relational database.

For uploaded, downloaded, or staged raw files, use the filesystem option. `RelationalDatabase` is intended only for datasets already staged in a relational database, in which case DatabaseName is also used.

### Packaging

```text
POST /api/workflow-process/package
```

Starts a dataset packaging workflow.

```bash
curl --location '<base url>/api/workflow-process/package?f=<workflow-process-fields>' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
    "id": "<dataset uuid>"
}'
```

The exact payload is defined in the [OpenAPI Reference](openapi.md).

### Recommendation registration

```text
POST /api/workflow-process/recommendation-register
```

Starts the workflow that registers a dataset with the recommendation subsystem.

```bash
curl --location '<base url>/api/workflow-process/recommendation-register?f=<workflow-process-fields>' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
    "id": "<dataset uuid>"
}'
```

The exact payload is defined in the [OpenAPI Reference](openapi.md).

### Cross-Dataset-Discovery ingestion

```text
POST /api/workflow-process/cdd-ingest
```

Starts the workflow that ingests a dataset into Cross Dataset Discovery.

```bash
curl --location '<base url>/api/workflow-process/cdd-ingest?f=<workflow-process-fields>' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
    "id": "<dataset uuid>"
}'
```

The exact payload is defined in the [OpenAPI Reference](openapi.md).

## Example Onboarding Usage

The onboarding workflow is started by calling the following endpoint:

```text
POST /api/workflow-process/onboard?f=Id
```

For example:

```bash
curl --location '<base url>/api/workflow-process/onboard?f=Id' 
\ --header 'Authorization: Bearer ey...aQ' 
\ --header 'Content-Type: application/json' 
\ --data '{
    "CiteAs": "ca",
    "ConformsTo": "ct",
    "doi": "10.1000/1234567890",
    "Country": ["GR"],
    "license": "https://cds.climate.copernicus.eu/datasets/reanalysis-era5-land?tab=download",
    "name": "Era5land",
    "description": "A global atmospheric reanalysis dataset produced by the European Centre for Medium\u2010Range Weather Forecast\u2019s (ECMWF) and has data available from 1950, providing a consistent view of the evolution of land variables. It has an enhanced resolution of 0.1\u00b0 x 0.1\u00b0, while the temporal frequency of the model output is hourly.",
    "mimeType": "application/db",
    "url": "https://cds.climate.copernicus.eu/datasets/reanalysis-era5-land?tab=download",
    "headline": "Meteorological data time series by ECWMF",
    "keywords": [
     "weather", "weather prediction"
    ],
    "fieldOfScience": [
        "EARTH AND RELATED ENVIRONMENTAL SCIENCES"
    ],
    "language": [
        "en"
    ],
    "datePublished": "2025-05-24",
    "DataLocations": [
       {
               "KIND": 5,
               "LOCATION": "ds_era5_land"
        }
    ]
}'
```

Because only Id is requested through the f field selector, the response contains the identifier of the newly created workflow process.

For example:

```json
{
    "id": "7c380cfa-c509-469c-a181-67064089ff2b"
}
```

The workflow process ID can then be used to monitor both the overall process and the individual processing steps. To do this, call:

```bash
curl --location '<base url>/api/workflow-process/7c380cfa-c509-469c-a181-67064089ff2b?f=Id&f=Status&f=Steps.Status&f=Steps.Id&f=Steps.WorkflowTaskInstanceDetails' 
\ --header 'Authorization: Bearer ey...aQ'
```

The requested fields provide:

 - Id — The workflow process identifier.
 - Status — The current status of the overall workflow process.
 - Steps.Id — The identifier of each workflow process step.
 - Steps.Status — The current status of each step.
 - Steps.WorkflowTaskInstanceDetails — Details and logs about the workflow task instances associated with the step in a free text format.

This endpoint can be called repeatedly by a client to monitor the workflow while processing continues automatically in the background.

Both WorkflowProcess and WorkflowProcessStep use the following status values:

- `In Progress = 0` — The process or step is currently being executed.
- `Failed = 1` — The process or step failed. If a step fails, the overall workflow process also transitions to Failed.
- `Succeeded = 2` — The process or step completed successfully. When a step succeeds, the next configured step can begin automatically.
- `Pending = 3` — The step has not started yet and is waiting for the preceding steps to complete.

A typical workflow therefore progresses as follows:

```text
Step 1: InProgress
Step 2: Pending
Step 3: Pending
Step 4: Pending
        |
        v
Step 1: Succeeded
Step 2: InProgress
Step 3: Pending
Step 4: Pending
        |
        v
       ...
        |
        v
Step 1: Succeeded
Step 2: Succeeded
Step 3: Succeeded
Step 4: Succeeded

WorkflowProcess: Succeeded

```

If any step fails, the workflow stops progressing and the overall process is marked as failed:

```text
Step 1: Succeeded
Step 2: Failed
Step 3: Pending
Step 4: Pending

WorkflowProcess: Failed
```

The caller therefore only needs to start the onboarding workflow once and monitor its state. Progression between the configured processing stages is handled automatically by the workflow callbacks and the Gateway.