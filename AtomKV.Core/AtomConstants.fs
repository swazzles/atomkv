namespace AtomKV.Core

open System

module AtomConstants = 
    (*
        Constants
    *)
    let endOfFile = -1L

    let keyLength = 50
    let keyRegex = "^[a-zA-Z0-9\._\-:]{1,50}$"

    let pageChannelBounding = 100
