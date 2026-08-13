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

| Value | Status       | Meaning                                                                                                                        |
| ----: | ------------ | ------------------------------------------------------------------------------------------------------------------------------ |
|   `0` | `InProgress` | The process or step is currently executing. Airflow tasks that are waiting to be retried also leave the step in this state.    |
|   `1` | `Failed`     | The process or step failed terminally. If a step fails, the parent workflow process also becomes `Failed`.                     |
|   `2` | `Succeeded`  | The process or step completed successfully. In the onboarding workflow, the next configured step can then start automatically. |
|   `3` | `Pending`    | The step has not started yet and is waiting for preceding steps to complete.                                                   |

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

| Value | Kind       | Meaning                                               |
| ----: | ---------- | ----------------------------------------------------- |
|   `0` | `File`     | Data is stored in a local or network filesystem path. |
|   `1` | `Http`     | Data is accessible through HTTP or HTTPS.             |
|   `2` | `Ftp`      | Data is accessible through FTP or FTPS.               |
|   `3` | `Remote`   | Reserved but currently not used.                      |
|   `4` | `Staged`   | The dataset is already staged.                        |
|   `5` | `Database` | The dataset is stored in a database.                  |

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
[".csv", ".xlsx", ".txt", ".pdf", ".png", ".jpeg", ".jpg", ".md"]
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

The recommended monitoring flow consists of two pieces of information:

1. the **workflow configuration**, which describes what a workflow and its steps represent and in which order they execute;
2. the **workflow process**, which describes the current execution state of those steps.

The configuration can be retrieved through `/api/workflow-process/config` and does not need to be inferred from hardcoded workflow or step IDs.

The `WorkflowProcess.Id` returned when onboarding is started should then be retained and used to retrieve the current execution state.

### 4.1 Get the workflow configuration

```text
GET /api/workflow-process/config
```

Returns the currently configured workflow process definitions.

Example:

```bash
curl --location '<base url>/api/workflow-process/config' \
--header 'Authorization: Bearer ey...aQ'
```

The response is a `WorkflowProcessConfig` containing the available workflow definitions and their configured steps.

For example, the Dataset Onboarding workflow is represented as:

```json
{
  "items": [
    {
      "id": "25593b3b-f2b8-4304-bba2-e6eb6e3f4872",
      "kind": 0,
      "name": "Dataset Onboarding",
      "description": "Onboards a new dataset by collecting and registering its metadata and data location within the platform.",
      "steps": [
        {
          "id": "8352e21f-a84f-4d41-92c8-30dc05577235",
          "order": 0,
          "kind": 0,
          "taskId": "DatasetOnboarding_test"
        },
        {
          "id": "7d115bb4-21f2-4c70-af08-1cc066aeb033",
          "order": 1,
          "kind": 1,
          "taskId": "DatasetProfiling_test"
        },
        {
          "id": "bc5ed9e1-ac8c-47b4-b986-7b28165bdc82",
          "order": 2,
          "kind": 2,
          "taskId": "DatasetPackaging_test"
        },
        {
          "id": "ebb8dffb-8f5f-447b-9986-8753ae8db398",
          "order": 3,
          "kind": 3,
          "taskId": "DatasetRecommendationRegistering_test"
        },
        {
          "id": "ed906ed4-5445-4df6-af9f-ffc4dde5300f",
          "order": 4,
          "kind": 4,
          "taskId": "CDD_Ingest_test"
        }
      ]
    }
  ]
}
```

The example above shows only the Dataset Onboarding item for brevity. The actual response also contains the configured standalone workflow definitions.

The relevant configuration fields are:

| Field                    | Purpose                                                                                |
| ------------------------ | -------------------------------------------------------------------------------------- |
| `items[].id`             | Identifies a workflow definition. It corresponds to `WorkflowProcess.ProcessId`.       |
| `items[].kind`           | Identifies the type of workflow.                                                       |
| `items[].name`           | Human-readable workflow name.                                                          |
| `items[].description`    | Description of the workflow's purpose.                                                 |
| `items[].steps[].id`     | Identifies a configured workflow step. It corresponds to `WorkflowProcessStep.StepId`. |
| `items[].steps[].kind`   | Identifies the type of processing stage represented by the step.                       |
| `items[].steps[].order`  | Defines the position of the step inside the workflow.                                  |
| `items[].steps[].taskId` | Identifies the underlying workflow-orchestrator task/DAG associated with the step.     |

The configuration is particularly important when interpreting a workflow process response because the `steps` array returned by the process endpoint should **not be assumed to be ordered by execution order**.

The frontend should use the configuration's `Order` value to order the returned process steps.

---

### 4.2 Get a workflow process by ID

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

| Field                               | Purpose                                                                                    |
| ----------------------------------- | ------------------------------------------------------------------------------------------ |
| `Id`                                | Identifies this workflow execution.                                                        |
| `ProcessId`                         | Identifies the configured workflow definition. Match this against `config.items[].id`.     |
| `Dataset.Id`                        | Identifies the dataset associated with the workflow.                                       |
| `Status`                            | Gives the overall workflow state.                                                          |
| `Steps.Id`                          | Identifies each workflow process step instance.                                            |
| `Steps.StepId`                      | Identifies the configured step definition. Match this against `config.items[].steps[].id`. |
| `Steps.Status`                      | Gives the execution state of each processing stage.                                        |
| `Steps.WorkflowTaskInstanceDetails` | Contains task-level callback events and diagnostic logs.                                   |
| `CreatedAt` / `UpdatedAt`           | Indicate when this execution was created and last updated.                                 |

If the workflow process cannot be found, the endpoint returns `404`.

An example response is:

```json
{
  "id": "222eec2d-0c65-4242-86bf-15acb1ad1e48",
  "processId": "25593b3b-f2b8-4304-bba2-e6eb6e3f4872",
  "dataset": {
    "id": "a72ee943-7a56-46e8-88b6-2cf382b2859b"
  },
  "steps": [
    {
      "id": "96f240c6-d3b9-4690-8a98-7d98cafbde31",
      "stepId": "7d115bb4-21f2-4c70-af08-1cc066aeb033",
      "status": 2
    },
    {
      "id": "7d18f642-a2d2-412f-9398-aba5aaf1ac68",
      "stepId": "8352e21f-a84f-4d41-92c8-30dc05577235",
      "status": 2
    },
    {
      "id": "25f0f6af-d55f-4178-92c8-ad612e1c0667",
      "stepId": "bc5ed9e1-ac8c-47b4-b986-7b28165bdc82",
      "status": 2
    },
    {
      "id": "1a2bc96e-5cfd-4ec6-bcca-0aca41e1983c",
      "stepId": "ebb8dffb-8f5f-447b-9986-8753ae8db398",
      "status": 2
    },
    {
      "id": "e45d197f-63eb-479f-8fc8-768a21597a02",
      "stepId": "ed906ed4-5445-4df6-af9f-ffc4dde5300f",
      "status": 1
    }
  ],
  "status": 1,
  "createdAt": "2026-08-12T12:43:14.94128Z",
  "updatedAt": "2026-08-12T12:49:14.596913Z"
}
```

---

### 4.3 Interpreting the current state

The `WorkflowProcess.Status` represents the overall state of the workflow, while each `WorkflowProcessStep.Status` represents the state of an individual processing stage.

To understand the workflow's current progress, the client should combine:

```text
Workflow configuration
        +
WorkflowProcess response
```

The configuration answers:

```text
What does this process/step represent?
In which order should the steps be displayed?
```

while the process response answers:

```text
What is happening to this particular execution?
```

The step statuses are:

| Status       | Value | Interpretation                                                                                                                                    |
| ------------ | ----: | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| `InProgress` |   `0` | The step is currently being processed. This is normally the current active stage. An Airflow retry also leaves the step in this state.            |
| `Failed`     |   `1` | The step failed terminally. The parent `WorkflowProcess` also becomes `Failed` and no later pending steps are started.                            |
| `Succeeded`  |   `2` | The step completed successfully. If the workflow is still `InProgress`, execution has moved to, or is about to move to, the next configured step. |
| `Pending`    |   `3` | The step has not started yet and is waiting for preceding steps to complete successfully.                                                         |

For an `InProgress` workflow, the typical ordered pattern is:

```text
Succeeded
Succeeded
InProgress
Pending
Pending
```

For example:

```text
Dataset Onboarding                  Succeeded
Dataset Profiling                   Succeeded
Dataset Packaging                   InProgress
Dataset Recommendation Registering  Pending
Cross Dataset Discovery Ingestion   Pending

WorkflowProcess                     InProgress
```

This means:

- Dataset Onboarding has completed successfully.
- Dataset Profiling has completed successfully.
- Dataset Packaging is the current active stage.
- Dataset Recommendation Registering and Cross Dataset Discovery Ingestion have not started.
- No additional processing endpoint needs to be invoked by the client.

A step may remain `InProgress` while an underlying Airflow task is being retried. An `InProgress` status therefore does not by itself mean that processing is stuck.

When all configured steps succeed:

```text
Dataset Onboarding                  Succeeded
Dataset Profiling                   Succeeded
Dataset Packaging                   Succeeded
Dataset Recommendation Registering  Succeeded
Cross Dataset Discovery Ingestion   Succeeded

WorkflowProcess                     Succeeded
```

the complete workflow has finished successfully.

If a step fails:

```text
Dataset Onboarding                  Succeeded
Dataset Profiling                   Succeeded
Dataset Packaging                   Failed
Dataset Recommendation Registering  Pending
Cross Dataset Discovery Ingestion   Pending

WorkflowProcess                     Failed
```

then:

- the failed step identifies where processing stopped;
- earlier `Succeeded` steps completed normally;
- later `Pending` steps were never executed and should not be interpreted as failures;
- the failed step's `WorkflowTaskInstanceDetails` should be inspected before deciding on a recovery action.

Once a `WorkflowProcess` becomes `Failed`, it remains failed.

---

### 4.4 Reading the workflow process response using the configuration

The workflow process response contains runtime identifiers and statuses. The configuration endpoint supplies the metadata required to interpret those identifiers.

The relationship is:

```text
WorkflowProcess.ProcessId
        |
        v
WorkflowProcessConfig.Items[].Id
```

and, for each step:

```text
WorkflowProcess.Steps[].StepId
        |
        v
WorkflowProcessConfig.Items[].Steps[].Id
```

#### Resolve the workflow definition

Given:

```json
{
  "processId": "25593b3b-f2b8-4304-bba2-e6eb6e3f4872"
}
```

find the configuration item where:

```text
configItem.Id == WorkflowProcess.ProcessId
```

For the example, this resolves to:

```text
Dataset Onboarding
```

The selected configuration item also provides the complete set of configured steps and their execution order.

#### Resolve each process step

A workflow process step contains:

```json
{
  "id": "e45d197f-63eb-479f-8fc8-768a21597a02",
  "stepId": "ed906ed4-5445-4df6-af9f-ffc4dde5300f",
  "status": 1
}
```

`id` and `stepId` represent different things:

| Field            | Meaning                                                                                                           |
| ---------------- | ----------------------------------------------------------------------------------------------------------------- |
| `steps[].id`     | ID of this particular `WorkflowProcessStep` execution. It can be used with `GET /api/workflow-process/step/{id}`. |
| `steps[].stepId` | ID of the configured step definition. Match this against the selected workflow configuration item's `Steps[].Id`. |

For each process step:

```text
processStep.StepId
        |
        v
find matching configStep.Id
        |
        +--> configStep.Order
        +--> configStep.Kind
        +--> configStep.TaskId
```

The `Order` determines where the step belongs in the workflow.

> **Important:** do not use the position of an entry in the `WorkflowProcess.steps` response array to determine execution order.

In the example response, Dataset Profiling appears in the array before Dataset Onboarding even though onboarding executes first.

The frontend should therefore join the process steps with the workflow configuration and sort them by the configuration's `Order`.

Conceptually:

```text
WorkflowProcess
    |
    | processId
    v
WorkflowProcessConfigItem
    |
    | steps[].stepId -> config Steps[].Id
    v
Configured step
    |
    +--> Order
    +--> Kind
    +--> TaskId
    |
    + process step Status
    v
Displayable workflow stage
```

#### Suggested client-side resolution

Conceptually, a client can resolve the response as follows:

```text
processConfig =
    config.Items
        .find(item => item.Id == process.ProcessId)

resolvedSteps =
    process.Steps
        .map(processStep => {
            configStep =
                processConfig.Steps
                    .find(step => step.Id == processStep.StepId)

            stageConfig =
                config.Items
                    .find(item => item.Kind == configStep.Kind)

            return {
                id: processStep.Id,
                stepId: processStep.StepId,
                name: stageConfig.Name,
                description: stageConfig.Description,
                order: configStep.Order,
                taskId: configStep.TaskId,
                status: processStep.Status
            }
        })
        .orderBy(step => step.order)
```

The exact client implementation may differ, but the important relationships are:

```text
ProcessId -> workflow configuration item

StepId -> configured workflow step

configured step Kind -> human-readable workflow/stage definition

configured step Order -> display/execution order

process step Status -> runtime state
```

#### `WorkflowTaskInstanceDetails`

For normal progress monitoring, the client generally only needs:

```text
Id
Status
Steps.Id
Steps.StepId
Steps.Status
```

A smaller request can therefore be used:

```bash
curl --location '<base url>/api/workflow-process/<workflow-process-id>?f=Id&f=Status&f=Steps.Id&f=Steps.StepId&f=Steps.Status' \
--header 'Authorization: Bearer ey...aQ'
```

`WorkflowTaskInstanceDetails` should be requested when detailed execution information is required, particularly for a failed or long-running step:

```bash
curl --location '<base url>/api/workflow-process/<workflow-process-id>?f=Id&f=Status&f=Steps.Id&f=Steps.StepId&f=Steps.Status&f=Steps.WorkflowTaskInstanceDetails' \
--header 'Authorization: Bearer ey...aQ'
```

In summary:

```text
GET /api/workflow-process/config
        |
        v
Understand workflow definitions and step order
        |
        v
GET /api/workflow-process/{id}
        |
        v
Read WorkflowProcess.Status
        |
        v
Join Steps.StepId with configuration Steps.Id
        |
        v
Sort steps by configuration Order
        |
        v
Read each Steps.Status
        |
        v
Determine completed / active / pending / failed stages
        |
        v
Inspect WorkflowTaskInstanceDetails when diagnostics are required
```

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

| Field        | Meaning                                                                |
| ------------ | ---------------------------------------------------------------------- |
| `event`      | Callback event such as execute, retry, success, failure, or skipped.   |
| `dag_id`     | The Airflow DAG that produced the event.                               |
| `task_id`    | The Airflow task that produced the event.                              |
| `run_id`     | Identifies the Airflow DAG run.                                        |
| `try_number` | The task attempt number.                                               |
| `exception`  | Exception information when available.                                  |
| `logs`       | Application-specific diagnostic log entries collected during the task. |

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

## 10. Workflow Configuration Reference

Workflow definitions should be obtained through:

```text
GET /api/workflow-process/config
```

rather than by maintaining workflow and step IDs independently in the client.

The endpoint returns a `WorkflowProcessConfig`:

```text
WorkflowProcessConfig
└── Items[]
    ├── Id
    ├── Kind
    ├── Name
    ├── Description
    └── Steps[]
        ├── Id
        ├── Kind
        ├── Order
        └── TaskId
```

### 10.1 Workflow process configuration item

Each item describes one workflow definition.

| Field         | Description                                                                 |
| ------------- | --------------------------------------------------------------------------- |
| `Id`          | Identifier of the workflow definition. Matches `WorkflowProcess.ProcessId`. |
| `Kind`        | Workflow type.                                                              |
| `Name`        | Human-readable name of the workflow.                                        |
| `Description` | Description of the workflow's purpose.                                      |
| `Steps`       | Ordered workflow-step definitions belonging to the workflow.                |

For example:

```json
{
  "id": "25593b3b-f2b8-4304-bba2-e6eb6e3f4872",
  "kind": 0,
  "name": "Dataset Onboarding",
  "description": "Onboards a new dataset by collecting and registering its metadata and data location within the platform.",
  "steps": [...]
}
```

### 10.2 Workflow step configuration item

Each configured step contains:

| Field    | Description                                                              |
| -------- | ------------------------------------------------------------------------ |
| `Id`     | Identifier of the configured step. Matches `WorkflowProcessStep.StepId`. |
| `Kind`   | Type of processing stage represented by the step.                        |
| `Order`  | Position of the step within its parent workflow.                         |
| `TaskId` | Underlying workflow-orchestrator task/DAG identifier.                    |

The configuration should be used whenever a client needs to:

- determine which workflow a `WorkflowProcess` represents;
- determine which stage a `WorkflowProcessStep` represents;
- display a human-readable stage name;
- order process steps correctly;
- associate a step with the underlying workflow-orchestrator task.

This avoids coupling clients to hardcoded workflow-process IDs, step IDs, or step ordering.
