module SslRedirector

open System
open System.Net
open System.Net.Sockets
open System.Collections.Generic

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
        Async.FromBeginEnd(SslRedirectorServer.toIList data, flags, 
                            (fun (data : IList<System.ArraySegment<byte>>,
                                    flags : SocketFlags, callback, state) ->
                                        this.BeginSend(data, flags, callback, state)),
                            this.EndSend)
    member this.MyReceiveAsync(data : byte array, flags : SocketFlags) =
        Async.FromBeginEnd(SslRedirectorServer.toIList data, flags, 
                            (fun (data : IList<System.ArraySegment<byte>>,
                                    flags : SocketFlags, callback, state) ->
                                        this.BeginReceive(data, flags, callback, state)),
                            this.EndReceive)

let realNetworkFunctions = {
    SslRedirectorServer.createEndPoint = fun (a:System.Net.IPAddress) b -> new System.Net.IPEndPoint(a, b)
    SslRedirectorServer.createSocket = fun a b c -> new System.Net.Sockets.Socket(a,b,c)
    SslRedirectorServer.bind = fun socket endpoint -> socket.Bind(endpoint) 
    SslRedirectorServer.listen = fun socket x -> socket.Listen(x)
    SslRedirectorServer.asyncAccept = fun (socket) -> socket.MyAcceptAsync() }

//        Async.FromBeginEnd((fun (callback, state) -> this.BeginAccept(callback, state)),
//                               this.EndAccept)
[<EntryPoint>]
let main _ = 
    let addr = System.Net.IPAddress.Parse("127.0.0.1")
    let disposable = SslRedirectorServer.StartNoExceptions addr 9324 realNetworkFunctions SslRedirectorServer.Loop
    System.Console.ReadKey() |> ignore
    disposable.Dispose()
    0 // return an integer exit code
