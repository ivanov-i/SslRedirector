namespace UnitTests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting

exception ExpectedError of string


[<TestClass>]
type ServerTest() = 

    let EndPointCreator = fun _ _ -> 
        null
    let SocketCreator = fun _ _ _->
        null 
    let Binder = fun _ _ -> ()
    let Listener = fun _ _ -> ()

    let dummyNetworkFunctions = {
        SslRedirectorServer.createEndPoint = EndPointCreator
        SslRedirectorServer.createSocket = SocketCreator
        SslRedirectorServer.bind = Binder
        SslRedirectorServer.listen = Listener
    }

    let setEndPointCreator f =
        {dummyNetworkFunctions with SslRedirectorServer.createEndPoint = f}
    let setSocketCreator f =
        {dummyNetworkFunctions with SslRedirectorServer.createSocket = f}
    let setBinder f =
        {dummyNetworkFunctions with SslRedirectorServer.bind = f}
    let setListener f =
        {dummyNetworkFunctions with SslRedirectorServer.listen = f}
 

    [<TestMethod>]
    [<ExpectedException(typeof<Exception>)>]
    member x.CreateEndpointCalled () =
        let EndPointCreator =
            fun _ _ -> failwith "This is expected"
        let dummyNetworkFunctions = setEndPointCreator EndPointCreator
        SslRedirectorServer.Start 0L 0 dummyNetworkFunctions |> ignore

    [<TestMethod>]
    member x.CreateEndpointCorrectParamsCalled () =
        let expectedAddr = 43890L
        let expectedPort = 48
        let EndPointCreator =
            fun addr port ->
                Assert.AreEqual( (expectedAddr, expectedPort), (addr, port))
                null
        let dummyNetworkFunctions = setEndPointCreator EndPointCreator
        SslRedirectorServer.Start expectedAddr expectedPort dummyNetworkFunctions |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<Exception>)>]
    member x.CreatesSocket () =
        let SocketCreator = fun _ _ _->
            failwith "This is expected"
        let dummyNetworkFunctions = setSocketCreator SocketCreator
        SslRedirectorServer.Start 0L 0 dummyNetworkFunctions |> ignore
        
    [<TestMethod>]
    member x.CreatesSocketWithCorrectParams () =
        let SocketCreator = fun addressFamily socketType protocolType ->
            match addressFamily, socketType, protocolType with
            |   System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream, 
                System.Net.Sockets.ProtocolType.IPv4
                    -> null
            | _,_,_ -> failwith "Socket created with incorrect parameters"
        let dummyNetworkFunctions = setSocketCreator SocketCreator
        SslRedirectorServer.Start 0L 0 dummyNetworkFunctions |> ignore


    [<TestMethod>]
    [<ExpectedException(typeof<Exception>)>]
    member x.BindCalled () =
        let binder = fun _ _ -> failwith "this is expected"
        let dummyNetworkFunctions = setBinder binder
        SslRedirectorServer.Start 0L 0 dummyNetworkFunctions |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<Exception>)>]
    member x.ListenCalled () =
        let listener = fun _ -> failwith "listen called"
        let dummyNetworkFunctions = setListener listener
        SslRedirectorServer.Start 0L 0 dummyNetworkFunctions |> ignore
