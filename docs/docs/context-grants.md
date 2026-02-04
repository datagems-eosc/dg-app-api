# Context Grant Endpoints

Authorization is managed horizontally thrugh the [DataGEMS AAI](https://datagems-eosc.github.io/dg-aai). Some permission granting capbilities are also exposed through the Gateway API endpoints that proxy the requests to the DataGEMS AAI service.

These endpoints are under the ```/api/principal/context-grants/...``` route. and can be separated in thr following coarse categories:
* Querying context grants assigned to logged in user
* Querying context grants assigned to some user or user group
* Assign / Unassign context grant to some user or user group

The context grants that can be assigned and are returned by the respective lookup options are the ones managed through the AAI service and are documented in the [DataGEMS AAI Security Model Context Roles](https://datagems-eosc.github.io/dg-aai/latest/security/#context-roles).

## Querying context grants

The ```/api/principal/context-grants/query``` endpoint allows querying based on predicates such as
* Dataset ids: Limit the response to context grants assigned for the specific dataset ids
* Collection ids: Limit the response to context grants assigned for the specific collection ids
* Roles: Limit the response to context grants assigned for the specific roles / context grants
* Subject Id: Which user to search for. Leaving it empty implies current user
* Target Kind: The kind of items the grant is assigned for eg Dataset / Collection

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/principal/context-grants/query' \
--header 'Authorization: Bearer eyJ...ZA' \
--header 'Content-Type: application/json' \
--data '{
    "roles": ["dg_ds-browse"]
}'
```

This will provide an answer like the following:

```json
[
    {
        "principalId": "16...d1",
        "principalType": 1,
        "targetType": 0,
        "targetId": "07...19",
        "role": "dg_ds-browse"
    },
    {
        "principalId": "16...d1",
        "principalType": 1,
        "targetType": 0,
        "targetId": "1f...0c",
        "role": "dg_ds-browse"
    }
]
```

## Querying context grants assigned to logged in user

There are three endpoints that allow retrieval of context grants for the logged in user:

### Retrieving all context grants for the logged in user

Using the ```/api/principal/me/context-grants``` endpoint we retrieve all context grants assigned to the logged in user

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/principal/me/context-grants' \
--header 'Authorization: Bearer eyYA'
```

This will provide an answer like the following:

```json
[
    {
        "principalId": "16...d1",
        "principalType": 1,
        "targetType": 0,
        "targetId": "07...19",
        "role": "dg_ds-browse"
    },
    {
        "principalId": "16...d1",
        "principalType": 1,
        "targetType": 0,
        "targetId": "07...19",
        "role": "dg_ds-search"
    },
    {
        "principalId": "16...d1",
        "principalType": 1,
        "targetType": 0,
        "targetId": "07...19",
        "role": "dg_ds-download"
    },
    {
        "principalId": "16...d1",
        "principalType": 1,
        "targetType": 1,
        "targetId": "23...43",
        "role": "dg_col-browse"
    }
]
```

### Retrieving context grants assigned for specific datasets for the logged in user

Using the ```/api/principal/me/context-grants/dataset``` endpoint we retrieve the context grants assigned to the logged in user for one or more datasets

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location 'http://localhost:50000/api/principal/me/context-grants/dataset?id=07...19&id=67...32' \
--header 'Authorization: Bearer ey...YA'
```

This will provide an answer like the following:

```json
{
    "07...19": [
        "dg_ds-browse",
        "dg_ds-search",
        "dg_ds-download"
    ],
    "67...32": [
        "dg_ds-browse"
    ]
}
```

### Retrieving context grants assigned for specific collections for the logged in user

Using the ```/api/principal/me/context-grants/collection``` endpoint we retrieve the context grants assigned to the logged in user for one or more collections

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location 'http://localhost:50000/api/principal/me/context-grants/collection?id=41...f1&id=7h...r8' \
--header 'Authorization: Bearer ey...YA'
```

This will provide an answer like the following:

```json
{
    "41...f1": [
        "dg_col-browse"
    ],
    "7h...r8": [
        "dg_col-browse"
    ]
}
```

## Querying context grants assigned to some user or user group

There are 6 endpoints that allow retrieval of context grants for an arbitrary user of user group:

### Retrieving all context grants assigned to some user

Using the ```/api/principal/user/<subject id>/context-grants``` endpoint we retrieve all context grants assigned to some user

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/principal/user/ec...2e/context-grants' \
--header 'Authorization: Bearer ey...IA'
```

This will provide an answer like the following:

```json
[
    {
        "principalId": "ec...2e",
        "principalType": 1,
        "targetType": 0,
        "targetId": "07...19",
        "role": "dg_ds-browse"
    },
    {
        "principalId": "ec...2e",
        "principalType": 1,
        "targetType": 0,
        "targetId": "07...19",
        "role": "dg_ds-search"
    }
]
```

### Retrieving all context grants assigned to some user group

Using the ```/api/principal/group/<group id>/context-grants``` endpoint we retrieve all context grants assigned to some user group

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/principal/group/f9...8f/context-grants' \
--header 'Authorization: Bearer ey...IA'
```

This will provide an answer like the following:

```json
[
    {
        "principalId": "f9...8f",
        "principalType": 1,
        "targetType": 0,
        "targetId": "07...19",
        "role": "dg_ds-browse"
    },
    {
        "principalId": "f9...8f",
        "principalType": 1,
        "targetType": 0,
        "targetId": "07...19",
        "role": "dg_ds-search"
    }
]
```

### Retrieving context grants assigned for specific collections to some user

Using the ```/api/principal/user/<user id>/context-grants/collection``` endpoint we retrieve context grants assigned for specific collections to some user

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/principal/user/ec...2e/context-grants/collection?id=23...43' \
--header 'Authorization: Bearer ey...IA'
```

This will provide an answer like the following:

```json
{
    "23...43": [
        "dg_col-browse"
    ]
}
```

### Retrieving all context grants assigned for specific collections to some user group

Using the ```/api/principal/group/f9...8f/context-grants/collection``` endpoint we retrieve all context grants assigned for specific collections to some user group

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/principal/group/f9...8f/context-grants/collection?id=23...43' \
--header 'Authorization: Bearer ey...IA'
```

This will provide an answer like the following:

```json
{
    "23...43": [
        "dg_col-browse"
    ]
}
```

### Retrieving context grants assigned for specific datasets to some user

Using the ```/api/principal/user/<user id>/context-grants/dataset``` endpoint we retrieve context grants assigned for specific datasets to some user

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/principal/user/ec...2e/context-grants/dataset?id=22...89' \
--header 'Authorization: Bearer ey...IA'
```

This will provide an answer like the following:

```json
{
    "22...89": [
        "dg_ds-browse",
        "dg_ds-search",
        "dg_ds-download"
    ]
}
```

### Retrieving all context grants assigned for specific datasets to some user group

Using the ```/api/principal/group/<group id>/context-grants/dataset``` endpoint we retrieve all context grants assigned for specific datasets to some user group

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location '<base url>/api/principal/group/f9...8f/context-grants/dataset?id=22...89' \
--header 'Authorization: Bearer ey...IA'
```

This will provide an answer like the following:

```json
{
    "22...89": [
        "dg_ds-browse",
        "dg_ds-search",
        "dg_ds-download"
    ]
}
```

## Assign / Unassign context grant to some user or user group

There are 8 endpoints that allow assigning and unassigning context grants to an arbitrary user of user group for a dataset or collection:

### Assign user specific access to a dataset

Using the ```/api/principal/context-grants/user/<subject id>/dataset/<dataset id>/role/<context grant>``` endpoint we assign user specific access to a dataset

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location --request POST '<base url>/api/principal/context-grants/user/dc...87/dataset/58...ad/role/dg_ds-browse' \
--header 'Authorization: Bearer ey...uA'
```

### Assign group specific access to a dataset

Using the ```/api/principal/context-grants/group/<subject id>/dataset/<dataset id>/role/<context grant>``` endpoint we assign user group specific access to a dataset

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location --request POST '<base url>/api/principal/context-grants/group/f9...8f/dataset/58...ad/role/dg_ds-browse' \
--header 'Authorization: Bearer ey...uA'
```

### Unassign user specific access to a dataset

Using the ```/api/principal/context-grants/user/<subject id>/dataset/<dataset id>/role/<context grant>``` endpoint we unassign user specific access to a dataset

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location --request DELETE '<base url>/api/principal/context-grants/user/dc...87/dataset/58...ad/role/dg_ds-browse' \
--header 'Authorization: Bearer ey...uA'
```

### Unassign group specific access to a dataset

Using the ```/api/principal/context-grants/group/<subject id>/dataset/<dataset id>/role/<context grant>``` endpoint we unassign user group specific access to a dataset

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location --request DELETE '<base url>/api/principal/context-grants/group/f9...8f/dataset/58...ad/role/dg_ds-browse' \
--header 'Authorization: Bearer ey...uA'
```

### Assign user specific access to a collection

Using the ```/api/principal/context-grants/user/<subject id>/collection/<collection id>/role/<context grant>``` endpoint we assign user specific access to a collection

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location --request POST '<base url>/api/principal/context-grants/user/dc...87/collection/d3...d2/role/dg_col-browse' \
--header 'Authorization: Bearer ey...uA'
```

### Assign group specific access to a collection

Using the ```/api/principal/context-grants/group/<subject id>/collection/<collection id>/role/<context grant>``` endpoint we assign user group specific access to a collection

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location --request POST '<base url>/api/principal/context-grants/group/f9...8f/collection/d3...d2/role/dg_col-browse' \
--header 'Authorization: Bearer ey...uA'
```

### Unassign user specific access to a collection

Using the ```/api/principal/context-grants/user/<subject id>/collection/<collection id>/role/<context grant>``` endpoint we unassign user specific access to a collection

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location --request DELETE '<base url>/api/principal/context-grants/user/dc...87/collection/d3...d2/role/dg_col-browse' \
--header 'Authorization: Bearer ey...uA'
```

### Unassign group specific access to a collection

Using the ```/api/principal/context-grants/group/<subject id>/collection/<collection id>/role/<context grant>``` endpoint we unassign user group specific access to a collection

More information can be found in the [OpenAPI Reference](openapi.md).

```bash
curl --location --request DELETE '<base url>/api/principal/context-grants/group/f9...8f/collection/d3...d2/role/dg_col-browse' \
--header 'Authorization: Bearer ey...uA'
```
