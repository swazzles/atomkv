namespace AtomKV.Core

open System
open System.Text
open System.Text.RegularExpressions
open System.Security.Cryptography
open System.Globalization

module AtomKeySpaceV1 = 
    let getKeyHash (key:string) = 
        use sha1 = SHA1.Create()
        Encoding.UTF8.GetBytes key
            |> sha1.ComputeHash
            |> Convert.ToHexString

    let getKeyShard (key:string) (keyspaceSize:int) = 
        let keyHash = getKeyHash key
        let keyHashInt = Int32.Parse(keyHash, NumberStyles.HexNumber)
        (keyHashInt % keyspaceSize |> abs) + 1

    let keyRegex (key:string) =
        let reg = new Regex(AtomConstants.keyRegex)
        reg.IsMatch(key)

    let validateKey (key:string) =
        match key with
            | x when x.Length > AtomConstants.keyLength -> false
            | x when (not (keyRegex x)) -> false
            | _ -> true