module SslRedirector

open System

let ep = fun (a:Int64) b -> System.Net.IPEndPoint(a, b)

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    0 // return an integer exit code
