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

This endpoint replaces the old pattern where onboarding was performed through `/api/dataset/onboard` and the caller subsequently had to invoke each processing stage separately.

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

Unlike the previous endpoint, the response is a `WorkflowProcess`, not a dataset UUID. The exact response fields depend on the requested field set.

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
    "...": "WorkflowProcessLookup predicates and projection"
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

## Workflow callback endpoints

The following endpoints support communication between workflow task callbacks and the Gateway.

They are part of the orchestration lifecycle and are not normally called by an end-user UI to advance the workflow manually.

All endpoints require authentication and apply the same validation and transaction handling configured by the Gateway API.

### Persist workflow-step progress

```text
POST /api/workflow-process/step/persist
```

This endpoint updates the state/details of a workflow process step. It is used by workflow callbacks to persist task-instance progress while the DAG is executing.

Typical callback events include task execution, retry, success, failure, and skipped-state handling, depending on the workflow callback configuration.

Updating a step does not by itself imply that the overall workflow stage has been finalized.

### Finalize onboarding

```text
POST /api/workflow-process/step/finalize-onboarding
```

The Gateway finalizes the onboarding step and receives the information required to continue with profiling.

### Finalize profiling

```text
POST /api/workflow-process/step/finalize-profiling
```

The request contains the workflow-process step information and the related dataset ID.

The Gateway finalizes the profiling stage and can continue with the next configured stage.

---

### Finalize packaging

```text
POST /api/workflow-process/step/finalize-packaging
```

The request contains the workflow-process step information and the related dataset ID.

The Gateway finalizes the packaging stage and can continue with the next configured stage.


### Finalize recommendation registration

```text
POST /api/workflow-process/step/finalize-recommendation
```

The request contains the workflow-process step information and the related dataset ID.

The Gateway finalizes recommendation registration and can continue with the next configured stage.

---

### Finalize CDD ingestion

```text
POST /api/workflow-process/step/finalize-cdd-ingestion
```

The request contains the workflow-process step information and the related dataset ID.

This finalizes the CDD ingestion stage and therefore completes the configured processing chain when CDD ingestion is the last stage.

## Migration from the previous API flow

Previously, a caller typically performed the following sequence:

```text
POST /api/dataset/onboard
        |
        v
wait for onboarding
        |
        v
POST /api/dataset/profile
        |
        v
wait for profiling
        |
        v
manually trigger the next processing operation
        |
       ...
```

The updated flow is workflow-oriented:

```text
POST /api/workflow-process/onboard
        |
        v
WorkflowProcess returned
        |
        v
callbacks update WorkflowProcessStep state
        |
        v
successful stage completion
        |
        v
Gateway automatically starts the next configured DAG
        |
        v
client monitors process/step state through
/api/workflow-process/... endpoints
```
