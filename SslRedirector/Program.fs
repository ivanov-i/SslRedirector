module SslRedirector

open System

let realNetworkFunctions = {
    SslRedirectorServer.createEndPoint = fun (a:System.Net.IPAddress) b -> new System.Net.IPEndPoint(a, b)
    SslRedirectorServer.createSocket = fun a b c -> new System.Net.Sockets.Socket(a,b,c)
    SslRedirectorServer.bind = fun socket endpoint -> socket.Bind(endpoint) 
    SslRedirectorServer.listen = fun socket x -> socket.Listen(x)
}
[<EntryPoint>]
let main _ = 
    let addr = System.Net.IPAddress.Parse("127.0.0.1")
    SslRedirectorServer.StartNoExceptions addr 9324 realNetworkFunctions
    0 // return an integer exit code
