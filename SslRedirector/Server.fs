module SslRedirectorServer

open System
open System.Net
open System.Net.Sockets
open System.Collections.Generic

let toIList<'T> (data : 'T array) =
    let segment = new System.ArraySegment<'T>(data)
    let data = new List<System.ArraySegment<'T>>() :> IList<System.ArraySegment<'T>>
    data.Add(segment)
    data

type System.Net.Sockets.Socket with 
    member this.MyAcceptAsync() =
        Async.FromBeginEnd((fun (callback, state) -> this.BeginAccept(callback, state)),
                            this.EndAccept)

    member this.MyConnectAsync(ipAddress : IPAddress, port : int) =
        Async.FromBeginEnd(ipAddress, port,
                            (fun (ipAddress:IPAddress, port, callback, state) ->
                                this.BeginConnect(ipAddress, port, callback, state)),
                            this.EndConnect)
    member this.MySendAsync(data : byte array, flags : SocketFlags) =
        Async.FromBeginEnd(toIList data, flags, 
                            (fun (data : IList<System.ArraySegment<byte>>,
                                    flags : SocketFlags, callback, state) ->
                                        this.BeginSend(data, flags, callback, state)),
                            this.EndSend)
    member this.MyReceiveAsync(data : byte array, flags : SocketFlags) =
        Async.FromBeginEnd(toIList data, flags, 
                            (fun (data : IList<System.ArraySegment<byte>>,
                                    flags : SocketFlags, callback, state) ->
                                        this.BeginReceive(data, flags, callback, state)),
                            this.EndReceive)

type NetworkFunctions = {
    createEndPoint : System.Net.IPAddress -> int -> System.Net.IPEndPoint
    createSocket : System.Net.Sockets.AddressFamily ->
                System.Net.Sockets.SocketType ->
                System.Net.Sockets.ProtocolType ->
                System.Net.Sockets.Socket
    bind: System.Net.Sockets.Socket -> System.Net.IPEndPoint -> unit
    listen: System.Net.Sockets.Socket -> int -> unit
    asyncAccept: System.Net.Sockets.Socket -> Async<System.Net.Sockets.Socket>
}

let StartNoExceptions addr port networkFunctions acceptLoop =
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
    let cts = new System.Threading.CancellationTokenSource()
    Async.Start(acceptLoop socket, cts.Token)
    {new IDisposable with member x.Dispose() = cts.Cancel(); socket.Close()}

let Start = fun a b c d ->
    try
        StartNoExceptions a b c d
    with
    | :? ArgumentOutOfRangeException -> failwith "Argument out of range"
    | :? System.Net.Sockets.SocketException as ex -> failwith ("Socket exception (" + ex.ErrorCode.ToString() + ")") 
    | :? ArgumentNullException -> failwith "endpoint is null"
    | :? ObjectDisposedException as ex -> failwith ("object is disposed: " + ex.ObjectName)
    | :? Security.SecurityException -> failwith "security exception"

let rec Loop (socket : System.Net.Sockets.Socket) = async{
    let! socket = socket.MyAcceptAsync()
    let buf = Array.zeroCreate<byte>(65535)
    let flags = new SocketFlags()
    let! nBytes = socket.MyReceiveAsync(buf, flags) 
    printfn "Received %d bytes from client computer." nBytes
    let buffer2 = Array.rev buf
    printfn "Sending..." 
    let! flag = socket.MySendAsync(buffer2, flags)
    printfn "Completed." 
    return ()
}    
     