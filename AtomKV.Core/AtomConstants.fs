namespace AtomKV.Core

open System

module AtomConstants = 
    (*
        Constants
    *)
    let keyLength = 50
    let keyRegex = "^[a-zA-Z0-9\._\-:]{1,50}$"

    let indexLength = Int64.MaxValue.ToString().Length
    let indexHashLength = 128 //SHA512 length
    let indexRecordLength = int64(keyLength + indexHashLength + indexLength + Environment.NewLine.Length)
    let indexPadCharacter = ' '    

