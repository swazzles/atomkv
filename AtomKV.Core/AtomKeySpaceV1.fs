namespace AtomKV.Core

open System
open System.Text
open System.Text.RegularExpressions
open System.Security.Cryptography
open System.Globalization

module AtomKeySpaceV1 = 
    (*
        Get an MD5 HexString of our Key
        Key is converted to UTF-8 bytes before hashing
    *)
    let getKeyHash (key:string) = 
        use hasher = MD5.Create()
        Encoding.UTF8.GetBytes key
            |> hasher.ComputeHash
            |> Convert.ToHexString

    (*
        Evenly distributes a particular Key across 1..n paritions where n = keyspaceSize
        We do this by:
            1. Getting the first 8 bytes and parsing as Int32
            2. Finding the remainder of said value divided by n
            3. Ensuring it is a number > 0

        This isused by an AtomTable to figure out which AtomPage to allocate a particular Key/Document to.
    *)
    let getKeyShard (key:string) (keyspaceSize:int) = 
        let keyHash = getKeyHash key 
        let first8 = keyHash[1..8]
        let asInt = Int32.Parse(first8, NumberStyles.HexNumber)
        let shard = asInt % keyspaceSize |> abs
        shard + 1

    let keyRegex (key:string) =
        let reg = new Regex(AtomConstants.keyRegex)
        reg.IsMatch(key)

    let validateKey (key:string) =
        match key with
            | x when x.Length > AtomConstants.keyLength -> false
            | x when (not (keyRegex x)) -> false
            | _ -> true