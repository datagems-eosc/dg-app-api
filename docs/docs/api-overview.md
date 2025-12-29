# Api Overview

The complete API reference for the provided endpoints is available directly as an [Open API](openapi.md) reference. Additionally, common [Status & Error Codes](error-codes.md) information is detailed to cover the integration information needed.

In this section, we will present some primary endpoints, and groups of endpoints to assist integrating parties. Additionally, some common practices that are used for a number of endpoints are listed separately.

## FieldSet / Projection

The Gateway API offers the option to apply selective projection over the information that will be returned by the invoked endpoints. If the projection list is left empty, no information is returned. 

* The projection list consists of the response model property names that are requested
* If additional authorization is required for some of the requested model properties, these will be censored and not included in the response
* Requested properties may be qualified to include nested models
* Depending on the request, project list field sets may be either part of the request body, or query string parameters

## Query Structure

A number of available endpoints provide the ability for the requestor to execute complex queries on to control the kind of data that will be included in the response. These query endpoints have a similar structure across the different entities over which they can be applied:

* Predicate section: The available predicates by which the caller can filter the items included in the response. This section is dependant on the predicates supported for the specific entity
* Paging section: Section that controls the number of items that are requested to be returned as well the option to skip some items. 
    * Paging is only valid if ordering is also applied
    * If no paging is requested, all items matching the selected predicates are returned
* Ordering section: Section that controls the ordering applied to the items included in the response
    * If supported by the endpoint utilized, ordering on multiple properties is supported
    * Ascending ordering is defined using a + predicate on the ordered property name
    * Descending ordering is defined using a - predicate on the ordered property name
* Metadata section: Additional directives that may govern how the query should operate
    * Supported option includes requesting a total count of the items matching the predicates
* Projection section: List of fiels to be included for the items matching the predicates
    * As explained in the Fieldset / Projection section

## Vocabulary

The vocabulary related endpoints provide common reference for vocabulary data that can be used across the DataGEMS platform. As the data modeling needs evolve, additionjal vocabularies can be supported. An example for retrieving the fields of science vocabulary is the following:

```console
curl --location 'https://datagems-dev.scayle.es/gw/api/vocabulary/fields-of-science' \
--header 'Authorization: Bearer ey...umA'
```

The response will include the hierarchy of the fields of science vocabulary.

```json
{
    "hierarchy": [
        {
            "ordinal": 0,
            "code": "1",
            "name": "NATURAL SCIENCES",
            "children": [
                {
                    "ordinal": 0,
                    "code": "1.1",
                    "name": "MATHEMATICS"
                }
				//...
            ]
        },
        {
            "ordinal": 1,
            "code": "2",
            "name": "ENGINEERING AND TECHNOLOGY",
            "children": [
                {
                    "ordinal": 0,
                    "code": "2.1",
                    "name": "CIVIL ENGINEERING"
                }
				//...
            ]
        }
		//...
	]
}
```

## Datasets

Dataset retrieval related endpoints are primarily query and retrieve by id endpoints. The query endpoint allows retrieval by metadata predicates as described in the [Open API reference](openapi.md).

An example of this query would be

```console
curl --location 'https://datagems-dev.scayle.es/gw/api/dataset/query' \
--header 'Authorization: Bearer eyJ...Scw' \
--header 'Content-Type: application/json' \
--data '{
    "project": { "fields": ["id", "code", "name", "description", "license", "mimeType", "size", "url", "headline", "keywords", "datePublished" ] },
    "page":{
        "Offset": 0,
        "Size": 10
    },
    "Order":{
        "Items": ["-size"]
    },
    "Metadata":{
        "CountAll": true
    }
}'
```

The response will include the requested items that match the predicates (not set for this example) including also the total number of datasets available. For each dataset the properties included in the projection list would be available and the retrieved datasets would be ordered by size in a descernding order.

```json
{
    "items": [
        {
            "id": "9...b",
            "code": "U...1",
            "name": "D...d",
            "description": "A...d",
            "license": "CC BY 4.0",
            "mimeType": "image/tiff",
            "size": 11274289152,
            "url": "https://...",
            "headline": "H...d",
            "keywords": [
                "g...e",
                "s...n",
                "p...l"
            ],
            "datePublished": "2...4"
        }
		//...
   ],
   "count": 50
}
```

## Collections

Collections can be created ad-hoc from authorized users. 

To create a new collection, the thew following requests can be used:

```console
curl --location 'https://datagems-dev.scayle.es/gw/api/collection/persist?f=id&f=name&f=code' \
--header 'Authorization: Bearer ey...bxA' \
--header 'Content-Type: application/json' \
--data '{
    "id": null,
    "code": "collection-test-4",
    "name": "collection-test-4"
}'
```

And the response would include the id of the new collection along with the fields that were requested to be returned (name and code)

```json
{
    "id": "5a5dc519-6b9a-494b-89e9-9c08aed8075e",
    "code": "collection-test-4",
    "name": "collection-test-4",
    "datasetCount": 0
}
```

We can then search for collections based on some predicates with the following request

```console
curl --location 'https://datagems-dev.scayle.es/gw/api/collection/query' \
--header 'Authorization: Bearer eyJh...bxA' \
--header 'Content-Type: application/json' \
--data '{
    "project": { "fields": ["id", "code", "name", "permissions.browseCollection", "permissions.editCollection" ] },
    "page":{
        "Offset": 0,
        "Size": 10
    },
    "Order":{
        "Items": ["+code"]
    },
    "Metadata":{
        "CountAll": true
    },
    "like": "%test%"
}'
```

And this would include the collection we created

```json
{
    "items": [
        {
            "id": "5a5dc519-6b9a-494b-89e9-9c08aed8075e",
            "code": "collection-test-4",
            "name": "collection-test-4",
            "permissions": [
                "browsecollection",
                "editcollection"
            ]
        }
    ],
    "count": 1
}
```

Collections can be deleted by users with the appropriate access control

```console
curl --location --request DELETE 'https://datagems-dev.scayle.es/gw/api/collection/5a5dc519-6b9a-494b-89e9-9c08aed8075e' \
--header 'Authorization: Bearer ey...Ew'
```

## Conversations

Content retrieval searches (and not metadata lookups) can be organized in conversations. Conversation management can be either explicit (with calls to dedicated conversation endpoints) or implicit through directives in seach related endpoints.

There are dedicated conversation endpoints that can be used to crete, manage, and retrieve conversation history.

Explicitly creating a conversation could be achieved with the following request

```console
curl --location 'https://datagems-dev.scayle.es/gw/api/conversation/me/persist?f=id&f=etag&f=name' \
--header 'Authorization: Bearer ey...qQ' \
--header 'Content-Type: application/json' \
--data '{
    "id": null,
    "name": "my chat 1.1"
}'
```

and the response would include the conversation id

```json
{
    "id": "b18c6013-3b87-40f1-933d-9a952ae1572d",
    "name": "my chat 1.1",
    "eTag": "1767019638"
}
```

To update the conversation, the same persist endpoint can be used, setting the id ant eTag properties to perform the update

```console
curl --location 'https://datagems-dev.scayle.es/gw/api/conversation/me/persist?f=id&f=etag&f=name' \
--header 'Authorization: Bearer eyJ...qQ' \
--header 'Content-Type: application/json' \
--data '{
    "id": "b18c6013-3b87-40f1-933d-9a952ae1572d",
    "eTag": "1767019638",
    "name": "my chat 1.1.1"
}'
```

and the response now includes the updated information and eTag for the entity

```json
{
    "id": "b18c6013-3b87-40f1-933d-9a952ae1572d",
    "name": "my chat 1.1.1",
    "eTag": "1767019758"
}
```

We can query for specific conversations and get back the payload of the messages exchanged as in the following example:

```console
curl --location 'https://datagems-dev.scayle.es/gw/api/conversation/me/query' \
--header 'Authorization: Bearer eyJ...SoQ' \
--header 'Content-Type: application/json' \
--data '{
    "ids": ["b18c6013-3b87-40f1-933d-9a952ae1572d"],
    "project": { "fields": ["id", "name", "user.id", "user.name", "datasets.dataset.id", "datasets.dataset.code", "messages.kind", "messages.data", "messages.createdAt" ] },
    "page":{
        "Offset": 0,
        "Size": 10
    },
    "Order":{
        "Items": ["+name"]
    },
    "Metadata":{
        "CountAll": true
    }
}'
```

An extract of the response would look like the following

```json
{
    "items": [
        {
            "id": "b18c6013-3b87-40f1-933d-9a952ae1572d",
            "name": "my chat 1.1.1",
            "user": {
                "id": "a...1",
                "name": "the user name"
            },
            "messages": [
                {
                    "conversation": {},
                    "kind": 3,
                    "data": {
                        "kind": 3,
                        "payload": {
                            "question": "What is the average daily temperature of Bern in the year 2020?",
                            "data": {
								//...
                            },
                            "status": 0,
                            "entries": [
                                {
                                    "process": {
                                        "kind": 1,
                                        "sql": {
											//...
                                        }
                                    },
                                    "result": {
                                        "kind": 1,
                                        "table": {
                                            "columns": [
                                                {
                                                    "columnNumber": 0,
                                                    "name": "date"
                                                },
                                                {
                                                    "columnNumber": 1,
                                                    "name": "avg_daily_celsius_temp"
                                                }
                                            ],
                                            "rows": [
												//...
                                            ]
                                        }
                                    },
                                    "status": 0
                                }
                            ]
                        },
                        "version": "V1"
                    },
                    "createdAt": "2025-12-29T14:52:00.683697Z"
                },
                {
                    "conversation": {},
                    "kind": 2,
                    "data": {
                        "kind": 2,
                        "payload": {
                            "question": "What is the average daily temperature of Bern in the year 2020?",
                            "datasetIds": [
                                "8...1"
                            ]
                        },
                        "version": "V1"
                    },
                    "createdAt": "2025-12-29T14:52:00.635993Z"
                }
            ]
        }
    ],
    "count": 1
}
```

## Current Principal

A dedicated endpoint is available to retrieve information on the logged in user and receive profile information as well as static permissions granted to the user

```console
curl --location 'https://datagems-dev.scayle.es/gw/api/principal/me' \
--header 'Authorization: Bearer eyJ...CtA'
```

The response includes

```json
{
    "isAuthenticated": true,
    "principal": {
        "subject": "c...5",
        "name": "dg user-1",
        "username": "dg-user-1",
        "givenName": "dg",
        "familyName": "user-1",
        "email": "dg-user-1@datagems.eu"
    },
    "token": {
        "issuer": "https://datagems-dev.scayle.es/oauth/realms/dev",
        "tokenType": "Bearer",
        "authorizedParty": "d...i",
        "audience": [
            "d...i",
            "a...p",
            "a...t"
        ],
        "expiresAt": "2025-12-29T15:07:56Z",
        "issuedAt": "2025-12-29T15:02:56Z",
        "scope": [
            "openid profile d...i email offline_access"
        ]
    },
    "roles": [
        "G...v",
        "G...r",
        "G...s",
        "G...n"
    ],
    "permissions": [
        "Can...ery",
        "Can...ion",
        "Can...ion",
        "Cre...ion",
        "Loo...ups"
    ],
    "deferredPermissions": [
        "Bro...set",
        "Bro...ion",
        "Bro...ion",
        "Del...ion",
        "Edi...ion",
        "Add...oup",
        "Rem...oup"
    ]
}
```

## Context Grants

For permissions granted at the context level, there are dedicated endpoints that allows retrieval and nanagement of context based access grants. These can resolve grants assigned explicitly to users or inherited through group memberships. Additionally, they will allow searcing based on specific users, grant types, dataset and collection context.

To retrieve the list of context grants assigned to the current user, the following endpoint can be used

```console
curl --location 'https://datagems-dev.scayle.es/gw/api/principal/me/context-grants' \
--header 'Authorization: Bearer eyJ...Nk-g'
```

and the response will be in the following form

```json
[
    {
        "principalId": "c...5",
        "principalType": 1,
        "targetType": 0,
        "targetId": "0...9",
        "role": "dg_ds-browse"
    },
    {
        "principalId": "c...5",
        "principalType": 1,
        "targetType": 0,
        "targetId": "0...9",
        "role": "dg_ds-search"
    },
    {
        "principalId": "c...5",
        "principalType": 1,
        "targetType": 1,
        "targetId": "4...1",
        "role": "dg_col-browse"
    },
    {
        "principalId": "c...5",
        "principalType": 1,
        "targetType": 0,
        "targetId": "a...8",
        "role": "dg_ds-search"
    },
    {
        "principalId": "c...5",
        "principalType": 0,
        "targetType": 1,
        "targetId": "2...a",
        "role": "dg_col-manage"
    }
]
```

## Storage

The gateway offers some storage handling endpoints. Data can be uploaded to support dataset onboarding flows. Specific file types are supported for data uploading. These can be retrieved using the following request

```console
curl --location 'https://datagems-dev.scayle.es/gw/api/storage/upload/allowed-extension' \
--header 'Authorization: Bearer eyJ...yA' \
```

and the response inclued the supported file formats

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

Uploading data that can then be used for the dataset onboarding can be performed using the following endpoint

```console
curl --location 'https://datagems-dev.scayle.es/gw/api/storage/upload/dataset' \
--header 'Authorization: Bearer eyJh...AayA' \
--form 'file1=@"test1.csv"' \
--form 'file2=@"test2.csv"'
```
