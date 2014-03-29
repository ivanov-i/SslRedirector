module SslRedirectorServer

open System

type NetworkFunctions = {
    createEndPoint : System.Net.IPAddress -> int -> System.Net.IPEndPoint
    createSocket : System.Net.Sockets.AddressFamily ->
                System.Net.Sockets.SocketType ->
                System.Net.Sockets.ProtocolType ->
                System.Net.Sockets.Socket
    bind: System.Net.Sockets.Socket -> System.Net.IPEndPoint -> unit
    listen: System.Net.Sockets.Socket -> int -> unit
}

let StartNoExceptions addr port networkFunctions =
    let {createEndPoint = endPointCreator
         createSocket = socketCreator
         bind = bind
         listen = listen
         } = networkFunctions
    let endpoint = endPointCreator addr port
    let socket = socketCreator 
                    System.Net.Sockets.AddressFamily.InterNetwork 
                    System.Net.Sockets.SocketType.Stream 
                    System.Net.Sockets.ProtocolType.Tcp
    bind socket endpoint
    listen socket 100
    ()

let Start = fun a b c ->
    try
        StartNoExceptions a b c
    with
    | :? ArgumentOutOfRangeException -> failwith "Argument out of range"
    | :? System.Net.Sockets.SocketException as ex -> failwith ("Socket exception (" + ex.ErrorCode.ToString() + ")") 
    | :? ArgumentNullException -> failwith "endpoint is null"
    | :? ObjectDisposedException as ex -> failwith ("object is disposed: " + ex.ObjectName)
    | :? Security.SecurityException -> failwith "security exception" 