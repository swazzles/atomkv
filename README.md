# AtomKV
A lightweight key-value database written from scratch in F# targeting .NET 6

## Disclaimer
Not to be confused with https://github.com/skeeto/atomkv (sorry, didn't look up the name beforehand)

This is not intended for any serious use, I work on this in my free time to learn F#.

If you trust me (someone who is currently learning F# and has no formal experience in building an actual database) to build a robust production-ready key-value database then please by all means use this in prod ASAP :)

Documentation here in the README is also a WIP.

## Summary
AtomKV is a key-value database writen from scratch in F# with minimal dependencies that uses an append-only approach to storing data so that data can be stored quickly and retrieved efficiently.

Data is stored as individual records of uniform binary data meaning you can store pretty much any form of data you need and create, update and retrieve it via a single key.

Under the hood AtomKV uses async computation expressions, multi-threading and [.NET Channels](https://learn.microsoft.com/en-us/dotnet/api/system.threading.channels.boundedchanneloptions?view=net-7.0) for high throughput read/write whilst minimising blocking.

Interaction with the DB is performed via a RESTful HTTP API.

## Getting Started - REST API
Using the AtomKV.Service Web API we can interact with the database via a simple RESTful API (Spec a WIP)

### Create/Update a document
```json

PUT /document/{key}
    REQUEST:
    {
        "doc": "dGVzdA=="
    }

    RESPONSE:
    200 OK
    {
        "requestId": "a72974f3-b599-4f28-b063-e7a52aa61d99",
        "status": 0
    }
``` 

### Get a document
```json
GET /document/{key}
    RESPONSE:
    200 OK
    {
        "requestId": "6587693c-4e92-424b-bbc2-a60d1d257f7e",
        "status": 0,
        "document": "dGVzdP8="
    }
```

## What can I store in AtomKV?
Anything.

AtomKV stores compressed binary data against a unique key. This means that you can store any kind of data you like as long as you can figure out how to serialize it to a byte array.
From JSON documents to video files, the world (within the confines of AtomKV) is your oyster.

## KeySpace
Documents in AtomKV are stored in a number of different pages on disk. 
When you open a table you can specifiy the total number of different pages to split your database up in.
Using key hashing AtomKV lets you distribute documents over these different pages to improve performance and to prevent IO bottlenecks reading/writing from the same file.
When you create a document, the key you provide is allocated a number from 1..n where n is the size of your keyspace.
This number maps directly to a page on disk and the document will be stored and retrieved from that page every time using the same key.
This allows for documents to be evenly distributed across all of the pages and prevents any one individual page from being overused.


## Use cases
- Prototyping: using AtomKV as a local dev database to get prototypes up quicker
- Virtual/Cloud-based file systems: using keys as file paths
- Caching: put AtomKV in front of your long-term data stores as a cache


## Roadmap
- Document versioning: assining a unique key a version where each new iteration of the document increments the version. This can be used for preventing deadlocks whilst maintaining data integrity. 
- Elastic key-space - Create tables without shards specified, as page files fill up expand our key-space dynamically whilst still allowing retrieval of documents by key only. 
- Backup and restore utilities
- Key conditions: search for documents using advanced operations on keys e.g. startsWith, contains.
- Document retrieval paging: retrieve 1-n documents from 1-n pages where key condition is met.
- Multi-part document put and get: for large documents where putting and getting to and from the DB in one go would be infeasible.
    - Consider making all get and put operations asynchronous and multi-part by default 
- Multi-node redundancy: allowing multiple machines to synchronize the database for data redundancy and failover.
- Multi-node sharding: bringing the on-disk KeySpace to the network level, allowing different parts of the KeySpace to be distributed over multiple networked nodes.
