# AtomKV
A lightweight key-value database written from scratch in F# targeting .NET 6

## Summary
AtomKV is a key-value database writen from scratch in F# that uses an append-only approach to storing data.
Data is stored as individual records of uniform binary data meaning you can put pretty much store any form of data you need and create, update and retrieve it via a single key.
The database can be used via a number of interfaces:
- AtomKV.CLI which uses custom interactive F# session with its own domain-specific meta language
- AtomKV.Service a RESTful WebAPI
- AtomKV.Core the core library which you can embed in any .NET application

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

## Roadmap
- Multi-node redundancy: allowing multiple machines to synchronize the database for data redundancy and failover.
- Multi-node sharding: bringing the on-disk KeySpace to the network level, allowing different parts of the KeySpace to be distributed over multiple networked nodes.
- Document versioning: assining a unique key a version where each new iteration of the document increments the version. This can be used for preventing deadlocks whilst maintaining data integrity.
- KeySpace size adjustment: allowing the KeySpace of an existing database to be adjusted to increase or decrease the number of partitions.
