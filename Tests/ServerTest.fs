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

    let dummyAddr = System.Net.IPAddress.Parse "127.0.0.1"

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
        SslRedirectorServer.Start dummyAddr 0 dummyNetworkFunctions |> ignore

    [<TestMethod>]
    member x.CreateEndpointCorrectParamsCalled () =
        let expectedAddr = dummyAddr
        let expectedPort = 48
        let EndPointCreator =
            fun addr port ->
                Assert.AreEqual( (expectedAddr, expectedPort), (addr, port))
                null
        let dummyNetworkFunctions = setEndPointCreator EndPointCreator
        SslRedirectorServer.Start expectedAddr expectedPort dummyNetworkFunctions |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<Exception>)>]
    member x.EndpointExceptionsHandled () =
        let EndPointCreator =
            fun _ _ -> raise (new ArgumentOutOfRangeException())
        let dummyNetworkFunctions = setEndPointCreator EndPointCreator
        SslRedirectorServer.Start dummyAddr 0 dummyNetworkFunctions |> ignore


    [<TestMethod>]
    [<ExpectedException(typeof<Exception>)>]
    member x.CreatesSocket () =
        let SocketCreator = fun _ _ _->
            failwith "This is expected"
        let dummyNetworkFunctions = setSocketCreator SocketCreator
        SslRedirectorServer.Start dummyAddr 0 dummyNetworkFunctions |> ignore
        
    [<TestMethod>]
    member x.CreatesSocketWithCorrectParams () =
        let SocketCreator = fun addressFamily socketType protocolType ->
            match addressFamily, socketType, protocolType with
            |   System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream, 
                System.Net.Sockets.ProtocolType.Tcp
                    -> null
            | _,_,_ -> failwith "Socket created with incorrect parameters"
        let dummyNetworkFunctions = setSocketCreator SocketCreator
        SslRedirectorServer.Start dummyAddr 0 dummyNetworkFunctions |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<Exception>)>]
    member x.SocketsExceptionsHandled () =
        let SocketCreator =
            fun _ _ -> raise (new System.Net.Sockets.SocketException())
        let dummyNetworkFunctions = setSocketCreator SocketCreator
        SslRedirectorServer.Start dummyAddr 0 dummyNetworkFunctions |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<Exception>)>]
    member x.BindCalled () =
        let binder = fun _ _ -> failwith "this is expected"
        let dummyNetworkFunctions = setBinder binder
        SslRedirectorServer.Start dummyAddr 0 dummyNetworkFunctions |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<Exception>)>]
    member x.BindExceptionsHandled () =
        let exceptions : System.Exception list = [(new ArgumentNullException())
                                                  (new System.Net.Sockets.SocketException())
                                                  (new ObjectDisposedException("eklmn"))
                                                  (new Security.SecurityException()) ]
        let checkExceptions = fun ex ->
            let binder = fun _ _ -> raise ex
            let dummyNetworkFunctions = setBinder binder
            SslRedirectorServer.Start dummyAddr 0 dummyNetworkFunctions |> ignore
            
        exceptions |> List.map checkExceptions |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<Exception>)>]
    member x.ListenCalled () =
        let listener = fun _ -> failwith "listen called"
        let dummyNetworkFunctions = setListener listener
        SslRedirectorServer.Start dummyAddr 0 dummyNetworkFunctions |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<Exception>)>]
    member x.ListenExceptionsHandled () =
        let exceptions : System.Exception list = [(new System.Net.Sockets.SocketException())
                                                  (new ObjectDisposedException("eklmn"))]
        let checkExceptions = fun ex ->
            let listener = fun _ _ -> raise ex
            let dummyNetworkFunctions = setListener listener
            SslRedirectorServer.Start dummyAddr 0 dummyNetworkFunctions |> ignore
            
        exceptions |> List.map checkExceptions |> ignore
