module AtomKV.Tests

open NUnit.Framework
open FsUnit
open AtomKV.Core.Types
open AtomKV.Core
open System
open System.Collections.Generic
open System.Text
open System.Text.Json

[<SetUp>]
let Setup () =
    ()

type TestType = {
    TestField: int
}


[<Test>]
let ``test putting and retrieving from page``()  =
    let atom = AtomDefaultInitialization.initialize()
    
    let page = AtomPage.openPage "1"
    AtomPage.deletePage page

    let page = AtomPage.openPage "1"

    let docObject = {
        TestField = 1
    }

    let doc = JsonSerialization.serialize docObject

    for i in [1..100] do
        let key = $"mykey-{i}"
        AtomPage.put atom page key doc |> ignore

        let getResponse = AtomPage.get atom page key
        getResponse.Status |> should equal GetResponseStatus.Ok

        let newDocObject = getResponse |> AtomDocument.unwrap |> JsonSerialization.deserialize<TestType>
        newDocObject.TestField |> should equal docObject.TestField

[<Test>]
let ``test generating stable key hash``() = 
    let key = "thisismytestkey"
    let hash1 = KeySpace.getKeyHash AtomKeySpaceV1.getKeyHash key
    let hash2 = KeySpace.getKeyHash AtomKeySpaceV1.getKeyHash key
    hash1 |> should equal hash2

[<Test>]
let ``test generating key shard``() =
    let keyspacesize = 128

    let key1 = "thisismytestkey"
    let key2 = "thisismytestkey"
    let key3 = "thisismysecondtestkey"

    let keyspace1 = KeySpace.getKeyShard AtomKeySpaceV1.getKeyShard key1 keyspacesize
    let keyspace2 = KeySpace.getKeyShard AtomKeySpaceV1.getKeyShard key2 keyspacesize
    let keyspace3 = KeySpace.getKeyShard AtomKeySpaceV1.getKeyShard key3 keyspacesize

    keyspace1 |> should equal keyspace2
    keyspace1 |> should not' (equal keyspace3)

[<Test>]
let ``test valid and invalid keys `` () =   
    KeySpace.isKeyValid AtomKeySpaceV1.validateKey "abcdefg12345.:_-" |> should equal true
    KeySpace.isKeyValid AtomKeySpaceV1.validateKey "x 2! $ s" |> should equal false

