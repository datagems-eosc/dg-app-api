# Data Onboarding & Processing flow Endpoints

The data onboarding and processing flows are primarily managed through respective DataGEMS conmponents such as the [DataGEMS Data Model Management](https://datagems-eosc.github.io/data-model-management), the [DataGEMS Data Workflow Orchestrator](https://datagems-eosc.github.io/dg-data-workflow), the [Dataset Profiler](https://datagems-eosc.github.io/dg-dataset-profiler), and others that can be found in the [DataGEMS documentation](https://datagems-eosc.github.io/).

The Gateway API provides entry points for some facets of the onboarding and processing phases as required and propagates processing requests to the underpinning components.

## Making data available to the platform

When it comes to making data available to the platform so that they can be ingested as datasets, there are various methods supported:
* Upload data files directly through the Gateway API
* Reference data that are publicly available through http / ftp
* Reference data previously staged to the platform as raw files
* Reference data previously staged to the platform as relations database

The first two methods are made available to users that have proper authorization to be used when registering a new dataset. The later two cases are restricted to administrator users as they require previous communication, offline actions and internal platform knowledge to be referenced.

## Uploading data

A user with the approriate authorization can upload data that can then be used to register a dataset.

### Allowed file extensions

The ```/api/storage/upload/allowed-extension``` endpoint provides the file extensions that are allowed to be apploaded

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/storage/upload/allowed-extension' \
--header 'Authorization: Bearer ey...Bg'
```

This will provide an answer like the following:

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

### Uploading files

The ```/api/storage/upload/dataset``` endpoint provides a way to stage data files in a controlled storage location so that they can then be used for dataset ingestion.

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

## Onboarding Dataset

Onboading a dataset is performed through the ```/api/dataset/onboard``` endpoint. Through this operation, an authorized user provides the required metadata to onboard a new dataset and link it to the related data. 

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/dataset/onboard' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
    "code": "dataset-test-A",
    "name": "dataset-test-A",
    "Description": "dataset-test-A",
    "License": "dataset-test-A",
    "MimeType": "dataset-test-A",
    "Size": 10,
    "Url": "https://dataset-test-A.gr",
    "Version": "dataset-test-A",
    "Headline": "dataset-test-A",
    "Keywords": ["dataset-test-A"],
    "FieldOfScience": ["dataset-test-A"],
    "Language": ["dataset-test-A"],
    "Country": ["dataset-test-A"],
    "DatePublished": "2025-10-22",
    "CiteAs": "dataset-test-A",
    "ConformsTo": "dataset-test-A",
    "DataLocations": [
        {
            "Kind": 0,
            "Location": "/path/to/staged/test1.8fd16d58ce0c45c99532060bf61ecbd4.csv"
        },
        {
            "Kind": 0,
            "Location": "/path/to/staged/test2.d7ccbfdd929f4158ab7e446a65ebc726.csv"
        }
    ]
}'
```

This will provide an answer like the following:

```json
"<dataset uuid>"
```

For the DataLocation property, as described in the [OpenAPI Reference](openapi.md), possible values include (please see the reference for the updated list): 
* File = 0 - Data is stored in a local or network file system path
* Http = 1 - Data is accessible via an HTTP or HTTPS endpoint
* Ftp = 2 - Data is accessible via an FTP or FTPS server
* Remote = 3 - Reserved but not used
* Staged = 4 - The dataset is already staged
* Database = 5 - The dataset is stored in a database

These options relate with the possible onboarding storage locations as detailed above.

As a result of this process a new dataset entry will be created with the provided metadata anf an ID will be assigned to the dataset. The dataset will not yet be profiled and no other processing steps will be done for this dataset. These will need to be executed explicitly. 

## Profiling Dataset

After a dataset is onboarded, an authorized user may request that the dataset is profiled. THis is a process that can happen multiple times for a dataset and is not directly linked to the dataset onboarding action that only takes place once in a dataset's lifetime. The profiling action is available through the ```/api/dataset/profile``` endpoint.

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/dataset/profile' \
--header 'Authorization: Bearer ey...aQ' \
--header 'Content-Type: application/json' \
--data '{
    "id": "<dataset uuid>",
    "dataStoreKind": 0
}'
```

This will provide an answer like the following:

```json
"<dataset uuid>"
```

For the DataStoreKind property, as described in the [OpenAPI Reference](openapi.md), possible values include (please see the reference for the updated list): 
* FileSystem = 0 - The dataset is stored in a filesystem
* RelationalDatabase = 1 - The dataset is stored in a relational database

These options relate with the possible onboarding storage locations as detailed above. For all cases where raw files are either uploaded, downloaded or already staged, the ```filesystem``` option should be set. The ```RelationalDatabase``` should be set only for the case where the dataset is already staged in a relational database as a administrative offline task.
