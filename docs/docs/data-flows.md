# Data Onboarding & Processing Flow Endpoints

The data onboarding and processing flows are implemented across DataGEMS components such as the [DataGEMS Data Model Management](https://datagems-eosc.github.io/data-model-management), the [DataGEMS Data Workflow Orchestrator](https://datagems-eosc.github.io/dg-data-workflow), the [Dataset Profiler](https://datagems-eosc.github.io/dg-dataset-profiler), and other components documented in the [DataGEMS documentation](https://datagems-eosc.github.io/).

The Gateway API provides the user-facing entry points for starting and monitoring these flows and coordinates their execution with the underpinning components.

The main workflow endpoint group is:

```text
/api/workflow-process
```

More information about request models, field projections, validation rules, and response schemas can be found in the [OpenAPI Reference](openapi.md).

---

## 1. Workflow Model

A workflow execution is represented by a `WorkflowProcess`.

A `WorkflowProcess` contains one or more `WorkflowProcessStep` entries. Each step represents one configured processing stage. While the underlying Airflow DAG is running, task-instance callbacks update the corresponding workflow step so that the Gateway can expose the current state and collected execution details.

### 1.1 Dataset onboarding workflow

A normal dataset onboarding request creates **one workflow process** containing the complete processing chain:

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
Dataset Recommendation Registering
        |
        v
Cross Dataset Discovery Ingestion
```

When a step succeeds, the Gateway automatically starts the next configured step. The client does not need to wait for each DAG to finish and manually invoke the next stage.

Conceptually:

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
  | task callbacks update the current WorkflowProcessStep
  v
Gateway
  |
  | successful step finalization
  v
starts next configured DAG
  |
  v
Profiling -> Packaging -> Recommendation Registration -> CDD Ingestion
```

If a step fails terminally, the chain stops and later steps remain pending.

### 1.2 Workflow and step statuses

Both `WorkflowProcess` and `WorkflowProcessStep` use the same status values:

| Value | Status | Meaning |
|---:|---|---|
| `0` | `InProgress` | The process or step is currently executing. Airflow tasks that are waiting to be retried also leave the step in this state. |
| `1` | `Failed` | The process or step failed terminally. If a step fails, the parent workflow process also becomes `Failed`. |
| `2` | `Succeeded` | The process or step completed successfully. In the onboarding workflow, the next configured step can then start automatically. |
| `3` | `Pending` | The step has not started yet and is waiting for preceding steps to complete. |

A typical in-progress workflow may look like:

```text
Dataset Onboarding                  Succeeded
Dataset Profiling                   Succeeded
Dataset Packaging                   InProgress
Dataset Recommendation Registering  Pending
CDD Ingestion                       Pending

WorkflowProcess                     InProgress
```

A failed workflow may look like:

```text
Dataset Onboarding                  Succeeded
Dataset Profiling                   Succeeded
Dataset Packaging                   Failed
Dataset Recommendation Registering  Pending
CDD Ingestion                       Pending

WorkflowProcess                     Failed
```

A failed workflow process is terminal. Its status is not changed later, even if another workflow is subsequently executed successfully for the same dataset.

---

## 2. Preparing Data for Onboarding

Before a dataset can be onboarded, its data must be accessible to the platform.

Supported approaches include:

- Upload data files directly through the Gateway API.
- Reference data available through HTTP/HTTPS or FTP/FTPS.
- Reference data already staged on the platform as raw files.
- Reference data already staged on the platform in a relational database.

Uploading files and referencing publicly accessible data are available to appropriately authorized users. Referencing already staged files or relational databases is intended for administrative scenarios.

### 2.1 Data location kinds

The `DataLocations` property indicates where the source data is located.

| Value | Kind | Meaning |
|---:|---|---|
| `0` | `File` | Data is stored in a local or network filesystem path. |
| `1` | `Http` | Data is accessible through HTTP or HTTPS. |
| `2` | `Ftp` | Data is accessible through FTP or FTPS. |
| `3` | `Remote` | Reserved but currently not used. |
| `4` | `Staged` | The dataset is already staged. |
| `5` | `Database` | The dataset is stored in a database. |

Refer to the OpenAPI reference for the current list.

### 2.2 Check allowed upload extensions

```text
GET /api/storage/upload/allowed-extension
```

Example:

```bash
curl --location '<base url>/api/storage/upload/allowed-extension' \
--header 'Authorization: Bearer ey...Bg'
```

Example response:

```json
[
  ".csv",
  ".xlsx",
  ".txt",
  ".pdf",
  ".png",
  ".jpeg",
  ".jpg",
  ".md"
]
```

### 2.3 Upload dataset files

```text
POST /api/storage/upload/dataset
```

The endpoint stages uploaded files in a controlled storage location so that they can later be referenced during onboarding.

Example:

```bash
curl --location '<base url>/api/storage/upload/dataset' \
--header 'Authorization: Bearer eyJ...zg' \
--form 'file1=@"/path/to/file/test1.csv"' \
--form 'file2=@"/path/to/file/test2.csv"'
```

Example response:

```json
[
  "/path/to/staged/test1.8fd16d58ce0c45c99532060bf61ecbd4.csv",
  "/path/to/staged/test2.d7ccbfdd929f4158ab7e446a65ebc726.csv"
]
```

The returned paths must be preserved by the caller and supplied as data locations in the onboarding request.

---

## 3. Starting Dataset Onboarding

Dataset onboarding is started through:

```text
POST /api/workflow-process/onboard
```

The request contains the dataset metadata and its data locations.

The endpoint also accepts the `f` query parameter used by the Gateway field-set mechanism. Each `f` value selects a field to include in the returned `WorkflowProcess`.

### 3.1 Basic onboarding example

```bash
curl --location '<base url>/api/workflow-process/onboard?f=Id' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
  "CiteAs": "ca",
  "ConformsTo": "ct",
  "doi": "10.1000/1234567890",
  "Country": ["GR"],
  "license": "https://cds.climate.copernicus.eu/datasets/reanalysis-era5-land?tab=download",
  "name": "Era5land",
  "description": "A global atmospheric reanalysis dataset produced by the European Centre for Medium-Range Weather Forecasts (ECMWF) and has data available from 1950, providing a consistent view of the evolution of land variables. It has an enhanced resolution of 0.1° x 0.1°, while the temporal frequency of the model output is hourly.",
  "mimeType": "application/db",
  "url": "https://cds.climate.copernicus.eu/datasets/reanalysis-era5-land?tab=download",
  "headline": "Meteorological data time series by ECMWF",
  "keywords": [
    "weather",
    "weather prediction"
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
      "Kind": 5,
      "Location": "ds_era5_land"
    }
  ]
}'
```

Because only `Id` was requested, the response contains only the newly created workflow process identifier:

```json
{
  "id": "7c380cfa-c509-469c-a181-67064089ff2b"
}
```

The returned ID should be used to monitor this specific workflow execution.

### 3.2 File-based onboarding example

For a previously uploaded file, a data location can reference the staged path:

```json
{
  "Name": "dataset-test-A",
  "Description": "dataset-test-A",
  "License": "dataset-test-A",
  "Url": "https://dataset-test-A.gr",
  "Headline": "dataset-test-A",
  "Keywords": ["dataset-test-A"],
  "FieldOfScience": ["dataset-test-A"],
  "Language": ["en"],
  "Country": ["GR"],
  "DatePublished": "2025-10-22",
  "CiteAs": "dataset-test-A",
  "Doi": "dataset-test-A",
  "DataLocations": [
    {
      "Kind": 0,
      "Location": "/path/to/staged/test1.8fd16d58ce0c45c99532060bf61ecbd4.csv"
    }
  ]
}
```

---

## 4. Monitoring a Workflow

The recommended monitoring path is to keep the `WorkflowProcess.Id` returned by the start request and retrieve that process directly.

### 4.1 Get a workflow process by ID

```text
GET /api/workflow-process/{id}
```

Example:

```bash
curl --location '<base url>/api/workflow-process/7c380cfa-c509-469c-a181-67064089ff2b?f=Id&f=ProcessId&f=Dataset.Id&f=Status&f=CreatedAt&f=UpdatedAt&f=Steps.Id&f=Steps.StepId&f=Steps.Status&f=Steps.WorkflowTaskInstanceDetails' \
--header 'Authorization: Bearer ey...aQ'
```

A useful monitoring response should provide enough information to answer:

1. Is the overall workflow still running, completed, or failed?
2. Which processing step is currently active, or which step failed?
3. What happened inside the underlying Airflow tasks?

The main fields are:

| Field | Purpose |
|---|---|
| `Id` | Identifies this workflow execution. |
| `ProcessId` | Identifies the configured workflow definition. |
| `Dataset.Id` | Identifies the dataset associated with the workflow. |
| `Status` | Gives the overall workflow state. |
| `Steps.Id` | Identifies each workflow process step instance. |
| `Steps.StepId` | Identifies the configured step definition. |
| `Steps.Status` | Gives the state of each processing stage. |
| `Steps.WorkflowTaskInstanceDetails` | Contains task-level callback events and diagnostic logs. |
| `CreatedAt` / `UpdatedAt` | Help identify when the workflow execution was created and last updated. |

If the workflow process cannot be found, the endpoint returns `404`.

### 4.2 Interpreting the current state

While the workflow is `InProgress`, the client does not need to invoke another processing endpoint. The Gateway and workflow callbacks continue the configured onboarding chain automatically.

When the workflow becomes `Succeeded`, all configured onboarding stages have completed successfully.

When the workflow becomes `Failed`, locate the step with `Status = Failed` and inspect its `WorkflowTaskInstanceDetails` before deciding on a recovery action.

---

## 5. Task-Level Diagnostics

`WorkflowTaskInstanceDetails` contains the task-instance events and logs collected from the underlying Airflow DAG.

The value is stored as text containing newline-separated JSON event objects. Each event represents a callback from an Airflow task instance.

A simplified profiling example is:

```text
{"event":"execute","dag_id":"DATASET_PROFILING_test","task_id":"trigger_profile","run_id":"...","try_number":1,"map_index":-1,"exception":null,"logs":[]}
{"event":"success","dag_id":"DATASET_PROFILING_test","task_id":"trigger_profile","run_id":"...","try_number":1,"map_index":-1,"exception":null,"logs":[...]}
{"event":"execute","dag_id":"DATASET_PROFILING_test","task_id":"wait_to_complete_profiling","run_id":"...","try_number":1,"map_index":-1,"exception":null,"logs":[]}
{"event":"success","dag_id":"DATASET_PROFILING_test","task_id":"wait_to_complete_profiling","run_id":"...","try_number":1,"map_index":-1,"exception":null,"logs":[...]}
```

Useful fields include:

| Field | Meaning |
|---|---|
| `event` | Callback event such as execute, retry, success, failure, or skipped. |
| `dag_id` | The Airflow DAG that produced the event. |
| `task_id` | The Airflow task that produced the event. |
| `run_id` | Identifies the Airflow DAG run. |
| `try_number` | The task attempt number. |
| `exception` | Exception information when available. |
| `logs` | Application-specific diagnostic log entries collected during the task. |

Entries in `logs` may include:

- log level,
- message,
- payload,
- response from an underpinning service,
- timestamp,
- task ID,
- sequence number,
- retry or reschedule information.

For example, profiling logs may contain the payload sent to the Dataset Profiler, the returned profiling job ID, later profiling status checks, and the response returned when the completed profile is fetched.

### 5.1 Airflow retries

An Airflow retry does **not** immediately change the step to `Failed`.

While retries remain available:

```text
WorkflowProcessStep.Status = InProgress
```

The retry callback appends additional events and logs to `WorkflowTaskInstanceDetails`.

A step remaining `InProgress` for some time therefore does not necessarily mean that it is stuck. Inspect `event`, `try_number`, `exception`, and the related logs when more detail is required.

Only a terminal failure causes the workflow step and its parent workflow process to become `Failed`.

---

## 6. Finding Workflow Executions

The direct `GET /api/workflow-process/{id}` endpoint is the simplest way to monitor a workflow when its ID is already known.

The query endpoints are useful when the caller needs to discover workflow executions, for example all workflows associated with a dataset or user.

### 6.1 Query workflow processes

```text
POST /api/workflow-process/query
```

The request body is a `WorkflowProcessLookup`.

Supported process-specific filters include:

- `Ids`
- `ExcludedIds`
- `UserIds`
- `DatasetIds`

The common lookup model also supports paging, ordering, metadata, and field projection as explained in [api overview](api-overview.md).

Example: find workflows for a dataset.

```bash
curl --location '<base url>/api/workflow-process/query' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
  "DatasetIds": [
    "a72ee943-7a56-46e8-88b6-2cf382b2859b"
  ],
  "Project": {
    "Fields": [
      "Id",
      "ProcessId",
      "Dataset.Id",
      "Status",
      "CreatedAt",
      "UpdatedAt",
      "Steps.Id",
      "Steps.StepId",
      "Steps.Status"
    ]
  }
}'
```

The response is:

```text
QueryResult<WorkflowProcess>
```

and contains the matching workflow processes together with the result count.

When multiple results exist for the same dataset:

- `Id` distinguishes individual workflow executions.
- `ProcessId` identifies which workflow definition was run.
- `Status` shows the current or terminal state of each execution.
- `CreatedAt` and `UpdatedAt` help distinguish older and newer executions.
- `Steps.StepId` identifies the configured processing stage.
- `Steps.Status` shows where each execution is or where it stopped.

Previous workflow executions remain available as historical records.

> When paging is used, ordering must also be supplied.

### 6.2 Query workflow process steps

```text
POST /api/workflow-process/step/query
```

The request body is a `WorkflowProcessStepLookup`.

Supported step-specific filters include:

- `Ids`
- `ExcludedIds`
- `ProcessIds`

Example: retrieve the steps belonging to a workflow process.

```bash
curl --location '<base url>/api/workflow-process/step/query' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
  "ProcessIds": [
    "<workflow-process-id>"
  ],
  "Project": {
    "Fields": [
      "Id",
      "StepId",
      "Status",
      "CreatedAt",
      "UpdatedAt",
      "WorkflowTaskInstanceDetails"
    ]
  }
}'
```

The response is:

```text
QueryResult<WorkflowProcessStep>
```

### 6.3 Get a workflow process step by ID

```text
GET /api/workflow-process/step/{id}
```

Example:

```bash
curl --location '<base url>/api/workflow-process/step/<workflow-process-step-id>?f=Id&f=StepId&f=Status&f=CreatedAt&f=UpdatedAt&f=WorkflowTaskInstanceDetails' \
--header 'Authorization: Bearer ey...aQ'
```

If the workflow process step cannot be found, the endpoint returns `404`.

---

## 7. Failure Diagnosis and Recovery

A failed step should **not** automatically be interpreted as an instruction to call the standalone endpoint for that processing stage.

A failure may be caused by:

- an unavailable underpinning service,
- an HTTP or network communication failure,
- invalid or unavailable input data,
- an infrastructure issue that exhausted its retries,
- an unexpected response from another DataGEMS component,
- a configuration problem,
- an application bug,
- or another blocking condition.

Recovery should therefore begin with the collected task logs.

### 7.1 Recommended diagnostic flow

```text
WorkflowProcess = Failed
        |
        v
Identify the WorkflowProcessStep with Status = Failed
        |
        v
Inspect WorkflowTaskInstanceDetails
        |
        v
Identify the failed Airflow task and its last attempt
        |
        v
Inspect exception, messages, payloads and service responses
        |
        v
Resolve the underlying problem
        |
        v
Decide what workflow action, if any, is appropriate
```

For example, if the packaging step failed because the Airflow task could not communicate with an underpinning service, immediately calling:

```text
POST /api/workflow-process/package
```

may simply reproduce the failure. It also does not repair or resume the failed onboarding workflow.

### 7.2 Restarting onboarding

Once a Dataset Onboarding `WorkflowProcess` becomes `Failed`, it remains failed permanently.

If the diagnosed problem requires the full onboarding flow to be executed again, submit a new request:

```text
POST /api/workflow-process/onboard
```

This creates a **new** `WorkflowProcess` with a new ID.

Conceptually:

```text
Workflow A
Onboarding -> Profiling -> Packaging (Failed)
Status: Failed
        |
        | diagnose and resolve the blocking issue
        v
Workflow B
Onboarding -> Profiling -> Packaging -> Recommendation -> CDD
Status: InProgress / Succeeded
```

Workflow B does not modify Workflow A. The earlier failed execution remains available for diagnostics and historical reference.

---

## 8. Standalone Processing Workflows

The Gateway also exposes individual processing stages as standalone workflows.

These endpoints create new, independent `WorkflowProcess` instances. They do **not** resume an existing onboarding workflow and should not be treated as generic "retry the failed step" endpoints.

### 8.1 Dataset profiling

```text
POST /api/workflow-process/profile
```

Example:

```bash
curl --location '<base url>/api/workflow-process/profile?f=Id&f=Status' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
  "id": "<dataset uuid>",
  "dataStoreKind": 0,
  "DatabaseName": null
}'
```

Supported `DataStoreKind` values include:

- `FileSystem = 0` — the dataset is stored in a filesystem.
- `RelationalDatabase = 1` — the dataset is stored in a relational database.

For uploaded, downloaded, or staged raw files, use `FileSystem`. `RelationalDatabase` is intended for datasets already staged in a relational database, in which case `DatabaseName` is also used.

### 8.2 Dataset packaging

```text
POST /api/workflow-process/package
```

Example:

```bash
curl --location '<base url>/api/workflow-process/package?f=Id&f=Status' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
  "id": "<dataset uuid>"
}'
```

### 8.3 Dataset recommendation registration

```text
POST /api/workflow-process/recommendation-register
```

Example:

```bash
curl --location '<base url>/api/workflow-process/recommendation-register?f=Id&f=Status' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
  "id": "<dataset uuid>"
}'
```

### 8.4 Cross Dataset Discovery ingestion

```text
POST /api/workflow-process/cdd-ingest
```

Example:

```bash
curl --location '<base url>/api/workflow-process/cdd-ingest?f=Id&f=Status' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
  "id": "<dataset uuid>"
}'
```

### 8.5 Important standalone-workflow behavior

Each standalone endpoint creates a new process containing only the requested stage.

For example:

```text
POST /api/workflow-process/package
```

creates a Dataset Packaging workflow containing only a packaging step.

If it succeeds, it does **not** automatically continue with:

```text
Recommendation Registration
        |
        v
CDD Ingestion
```

Likewise, a successful standalone workflow does not modify the state of an earlier failed Dataset Onboarding process.

Whether a standalone workflow is appropriate after a failure depends on the actual failure cause and the intended recovery procedure.

---

## 9. Recommended Client Flow

For a normal dataset onboarding integration:

### Step 1 — Prepare the data

Upload files when necessary or provide another supported `DataLocation`.

### Step 2 — Start onboarding

```text
POST /api/workflow-process/onboard?f=Id
```

Store the returned `WorkflowProcess.Id`.

### Step 3 — Monitor the workflow

Periodically request:

```text
GET /api/workflow-process/{id}
```

At minimum, request:

```text
Id
Status
Steps.Id
Steps.StepId
Steps.Status
```

Request `Steps.WorkflowTaskInstanceDetails` when task-level diagnostics are required.

### Step 4 — Interpret the terminal state

If the process is `Succeeded`, all configured onboarding stages completed successfully.

If the process is `Failed`, identify the failed step, inspect its `WorkflowTaskInstanceDetails`, resolve the underlying problem, and only then decide whether a new onboarding process or another explicit workflow execution is appropriate.

### Step 5 — Discover historical executions when needed

If the process ID is no longer known, or multiple executions need to be reviewed, query by dataset:

```text
POST /api/workflow-process/query
```

using `DatasetIds`.

---

## 10. Workflow Definition Reference

The IDs below reflect the currently configured workflow definitions and are primarily useful for interpreting `ProcessId` and `StepId` values returned by the API.

### 10.1 Dataset Onboarding

| Property | Value |
|---|---|
| Process ID | `25593b3b-f2b8-4304-bba2-e6eb6e3f4872` |
| Name | Dataset Onboarding |

Configured steps:

| Order | Step | Step ID | Airflow DAG |
|---:|---|---|---|
| 0 | Dataset Onboarding | `8352e21f-a84f-4d41-92c8-30dc05577235` | `DatasetOnboarding_test` |
| 1 | Dataset Profiling | `7d115bb4-21f2-4c70-af08-1cc066aeb033` | `DatasetProfiling_test` |
| 2 | Dataset Packaging | `bc5ed9e1-ac8c-47b4-b986-7b28165bdc82` | `DatasetPackaging_test` |
| 3 | Dataset Recommendation Registering | `ebb8dffb-8f5f-447b-9986-8753ae8db398` | `DatasetRecommendationRegistering_test` |
| 4 | Cross Dataset Discovery Ingestion | `ed906ed4-5445-4df6-af9f-ffc4dde5300f` | `CDD_Ingest_test` |

### 10.2 Standalone workflow definitions

| Workflow | Process ID | Step ID | Airflow DAG |
|---|---|---|---|
| Dataset Profiling | `97852575-fa6e-4475-9725-7f8f8ff34e03` | `62a67d16-e9fd-405c-98e8-7fda4cf42bec` | `DatasetProfiling_test` |
| Dataset Packaging | `c6a8da71-0d78-458b-afed-f06b6ffe092e` | `97b486d4-fd09-4815-8d6a-17a8bf4c07d8` | `DatasetPackaging_test` |
| Dataset Recommendation Registering | `2b13dc1e-0a10-4c73-b3ec-15e760745c37` | `d51f0120-0070-4e9e-8e2f-a2eb7bfc7620` | `DatasetRecommendationRegistering_test` |
| Cross Dataset Discovery Ingestion | `caddac49-0db9-48ad-be52-d3a68031263b` | `805aea17-7f23-40dd-8430-f653621caaf6` | `CDD_Ingest_test` |

---

## 11. Endpoint Summary

| Method | Endpoint | Purpose |
|---|---|---|
| `GET` | `/api/storage/upload/allowed-extension` | Get allowed file extensions for dataset uploads. |
| `POST` | `/api/storage/upload/dataset` | Stage dataset files before onboarding. |
| `POST` | `/api/workflow-process/onboard` | Start the complete dataset onboarding workflow. |
| `GET` | `/api/workflow-process/{id}` | Retrieve one workflow process and selected fields. |
| `POST` | `/api/workflow-process/query` | Search workflow processes, including by dataset or user. |
| `GET` | `/api/workflow-process/step/{id}` | Retrieve one workflow process step. |
| `POST` | `/api/workflow-process/step/query` | Search workflow process steps. |
| `POST` | `/api/workflow-process/profile` | Start a standalone profiling workflow. |
| `POST` | `/api/workflow-process/package` | Start a standalone packaging workflow. |
| `POST` | `/api/workflow-process/recommendation-register` | Start a standalone recommendation-registration workflow. |
| `POST` | `/api/workflow-process/cdd-ingest` | Start a standalone Cross Dataset Discovery ingestion workflow. |

---

## 12. Operational Rules at a Glance

1. A Dataset Onboarding request creates one workflow process containing all five configured stages.
2. Successful onboarding steps automatically trigger the next configured step.
3. Airflow retries leave the corresponding workflow step in `InProgress`.
4. Task execution, retry, success, failure, and diagnostic information is accumulated in `WorkflowTaskInstanceDetails`.
5. A terminal step failure immediately marks the whole onboarding workflow as `Failed`.
6. A failed workflow process cannot be resumed or changed back to `InProgress`.
7. Recovery decisions should be based on the collected task logs, not only on the name of the failed step.
8. Re-running onboarding creates a new workflow process and preserves the earlier failed execution.
9. Standalone processing endpoints create separate one-step workflows.
10. Standalone workflows do not resume an onboarding chain or automatically continue to later stages.
11. Workflow executions for a dataset can be found through `/api/workflow-process/query` using `DatasetIds`.
12. Historical workflow executions remain available for diagnostics and comparison.
