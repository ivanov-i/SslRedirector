module SslRedirectorServer

open System

type NetworkFunctions = {
    createEndPoint : Int64 -> int -> System.Net.IPEndPoint
    createSocket : System.Net.Sockets.AddressFamily ->
                System.Net.Sockets.SocketType ->
                System.Net.Sockets.ProtocolType ->
                System.Net.Sockets.Socket
    bind: System.Net.Sockets.Socket -> System.Net.IPEndPoint -> unit
    listen: System.Net.Sockets.Socket -> int -> unit
}

let Start addr port networkFunctions =
    let {createEndPoint = endPointCreator
         createSocket = socketCreator
         bind = bind
         listen = listen
         } = networkFunctions
    let endpoint = endPointCreator addr port
    let socket = socketCreator 
                    System.Net.Sockets.AddressFamily.InterNetwork 
                    System.Net.Sockets.SocketType.Stream 
                    System.Net.Sockets.ProtocolType.IPv4
    bind socket endpoint
    listen socket 100
    ()