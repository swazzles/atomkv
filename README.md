# AtomKV
A lightweight key-value database written from scratch in F# targeting .NET 6

## Disclaimer
Not to be confused with https://github.com/skeeto/atomkv (sorry, didn't look up the name and I can't be bothered changing it)

This is not intended for any serious use, I work on this in my free time to learn F#.

If you trust me (someone who is currently learning F# and has no formal experience in building an actual database) to build a robust production-ready key-value database then please by all means use this in prod ASAP :)

"Why aren't there any comments in code?" Idk read it and figure it out

## Summary
AtomKV is a key-value database writen from scratch in F# with minimal dependencies that uses an append-only approach to storing data so that data can be stored quickly and retrieved efficiently.

Data is stored as individual records of uniform binary data meaning you can store pretty much any form of data you need and create, update and retrieve it via a single key.

AtomKV can be used via a number of interfaces:
- AtomKV.CLI (Not useable yet) which uses custom interactive F# session with its own domain-specific meta language
- AtomKV.Service (Doesn't even build, just unload the project) a RESTful WebAPI
- AtomKV.Core (Kind of works) the core library which you can embed in any .NET application

## Getting Started - Embedded
Using the AtomKV.Core library we can interact with the database like so
```fsharp

// setup our services to be injected
let services = {
            DocumentSerializer = JsonDocumentSerialization.objectToJsonBytes
            DocumentDeserializer = JsonDocumentSerialization.objectFromJsonBytes

            KeyHasher = AtomKeySpaceV1.getKeyHash
            KeySharder = AtomKeySpaceV1.getKeyShard
            KeyValidator = AtomKeySpaceV1.validateKey

            Compressor = GZipCompression.compress
            Decompressor = GZipCompression.decompress
        }
        
// open a table with a 16-partition keyspace
let table = AtomTable.open "MyTable" 16 services

// create .NET object to represent some data
let myObject = {
  Name = "Bob"
  Age = 55
}

// serialize the data to JSON bytes
let someData = DocumentSerialization.serializeDocument services.DocumentSerializer myObject

// store it in the db 
AtomTable.put table "mykey" someData

// retrieve the document
let doc = AtomTable.get table "mykey" |> DocumentSerialization.deserializeDocument services.DocumentSerializer
``` 

## Extensibility
You may notice the services bundle at the top of the previous code example. AtomKV is architected with dependency-injection at its core meaning all of the key features of the database can be swapped out with your own custom implementation if you so choose.
There are defaults provided for things like compression, document serialization, key-space management but you can swap these out altogether if you need to.

## KeySpace
Documents in AtomKV are stored in a number of different pages on disk. 
When you open a table you can specifiy the total number of different pages to split your database up in.
Using key hashing AtomKV lets you distribute documents over these different pages to improve performance and to prevent IO bottlenecks reading/writing from the same file.
When you create a document, the key you provide is allocated a number from 1..n where n is the size of your keyspace.
This number maps directly to a page on disk and the document will be stored and retrieved from that page every time using the same key.
This allows for documents to be evenly distributed across all of the pages and prevents any one individual page from being overused.

## What can I store in AtomKV?
Anything.

AtomKV stores compressed binary data against a unique key. This means that you can store any kind of data you like as long as you can figure out how to serialize it to a byte array.
From JSON documents to video files, the world (within the confines of AtomKV) is your oyster.

## What if I want to query on more than one key?
Unfortunately AtomKV isn't specifically designed to operate like a relational database. If you do need to query on multiple pieces of data and have them be indexed then you're better off using a SQL database.
However, that's not to say this isn't possible. It's very easy for you to use a key-value database like a relational one through the use of multiple records.

For example

```fsharp
let employee = {
    EmployeeId = 25
    Name = "Bob"
    CompanyId = 12
}

let company = {
    Name = "Big Mega Corp Inc."
    CompanyId = 12
}

AtomTable.put table "employee-25" JsonSerialization.serialize employee
AtomTable.put table "company-12" JsonSerialization.serialize employee
```
We have just stored two records, an employee and a company. Already we can see a relationship has been established between Employee and Company through `employee.CompanyId`.
By first retrieving the employee, you can then perform a second get to grab the company that the employee is a part of.

## Use cases
- Prototyping: using AtomKV as a local dev database to get prototypes up quicker
- Virtual/Cloud-based file systems: using keys as file paths
- Caching: put AtomKV in front of your long-term data stores as a cache


## Roadmap
- Document versioning: assining a unique key a version where each new iteration of the document increments the version. This can be used for preventing deadlocks whilst maintaining data integrity.
- Backup and restore utilities
- Key conditions: search for documents using advanced operations on keys e.g. startsWith, contains.
- Document retrieval paging: retrieve 1-n documents from 1-n pages where key condition is met.
- Multi-part document put and get: for large documents where putting and getting to and from the DB in one go would be infeasible.
    - Consider making all get and put operations asynchronous and multi-part by default 
- Multi-node redundancy: allowing multiple machines to synchronize the database for data redundancy and failover.
- Multi-node sharding: bringing the on-disk KeySpace to the network level, allowing different parts of the KeySpace to be distributed over multiple networked nodes.
- KeySpace size adjustment: allowing the KeySpace of an existing database to be adjusted to increase or decrease the number of partitions.
