namespace LockChecker

open System.IO
open System.Net
open System.Net.Sockets
open System.Runtime.Serialization.Json
open System.Runtime.Serialization

open System.Xml
open LockChecker.Graph

[<DataContract>]
type NewMethodMessage =
    {
        [<field: DataMember(Name="method")>]
        method: Method
        
        [<field: DataMember(Name="edges")>]
        edges: RawEdge []
    }
    static member JsonReader = DataContractJsonSerializer(typeof<NewMethodMessage>)
    static member FromJson (source: Stream) =
        NewMethodMessage.JsonReader.ReadObject(source) :?> NewMethodMessage
        
[<DataContract>]
type MethodChangedMessage =
    {
        [<field: DataMember(Name="method")>]
        method: Method
        
        [<field: DataMember(Name="edges")>]
        edges: RawEdge []
    }
    static member JsonReader = DataContractJsonSerializer(typeof<MethodChangedMessage>)
    static member FromJson (source: Stream) =
        MethodChangedMessage.JsonReader.ReadObject(source) :?> MethodChangedMessage
        
[<DataContract>]
type MethodRemovedMessage =
    {
        [<field: DataMember(Name="name")>]
        name: string
    }
    static member JsonReader = DataContractJsonSerializer(typeof<MethodRemovedMessage>)
    static member FromJson (source: Stream) =
        MethodRemovedMessage.JsonReader.ReadObject(source) :?> MethodRemovedMessage

[<DataContract>]
type AddEdgePackMessage =
    {
        [<field: DataMember(Name="edges")>]
        edges: RawEdge []
    }
    static member JsonReader = DataContractJsonSerializer(typeof<AddEdgePackMessage>)
    static member FromJson (source: Stream) =
        AddEdgePackMessage.JsonReader.ReadObject(source) :?> AddEdgePackMessage

[<DataContract>]
type RunAnalysisMessage =
    {
        [<field: DataMember(Name="starts")>]
        starts: int []
    }
    static member JsonReader = DataContractJsonSerializer(typeof<RunAnalysisMessage>)
    static member FromJson (source: Stream) =
        RunAnalysisMessage.JsonReader.ReadObject(source) :?> RunAnalysisMessage
        
[<DataContract>]
type DecoderRecord =
    {
        [<field: DataMember(Name="code")>]
        code: string
        
        [<field: DataMember(Name="data")>]
        data: string
    }

[<DataContract>]
type UpdateDecoderMessage =
    {
        [<field: DataMember(Name="records")>]
        records: DecoderRecord []
    }
    static member JsonReader = DataContractJsonSerializer(typeof<UpdateDecoderMessage>)
    static member FromJson (source: Stream) =
        UpdateDecoderMessage.JsonReader.ReadObject(source) :?> UpdateDecoderMessage

type ServiceHost(graph: IControlFlowGraph, port) =
    let socket = TcpListener.Create (port)
    let mutable client = null
    let mutable stream = null
    let mutable reader = null
    let mutable xmlReader = null
    let mutable isProcess = true
   
    let performParsing() =
        graph.PrepareForParsing()
        
        let statistics = graph.GetStatistics()
        let parserSource = Parsing.generateParser statistics.calls statistics.locks statistics.asserts
        
        let results = Parsing.parseGraph parserSource graph
        
        graph.CleanUpAfterParsing()
    
    member this.Start() =
        socket.Start()
        client <- socket.AcceptTcpClient()
        stream <- client.GetStream()
        reader <- new StreamReader(stream)
        
        use writer = new StreamWriter(stream)
        
        while isProcess do
            let messageType = reader.ReadLine()
            let data = reader.ReadLine()
            let mutable success = false
            
            try
                use dataStream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(data))
                match messageType with
                | "add_method" -> 
                    let message = NewMethodMessage.FromJson dataStream
                    graph.AddMethod message.method message.edges
                    success <- true
                | "alter_method" -> 
                    let message = MethodChangedMessage.FromJson dataStream
                    graph.AlterMethod message.method message.edges
                    success <- true
                | "add_edge_pack" -> 
                    let message = AddEdgePackMessage.FromJson dataStream
                    graph.AddEdges message.edges
                    success <- true
                | "remove_method" -> 
                    let message = MethodRemovedMessage.FromJson dataStream
                    graph.RemoveMethod message.name
                    success <- true
                | "update_decoder" -> 
                    let message = UpdateDecoderMessage.FromJson dataStream
                    message.records |> Array.iter (fun record -> ResultProcessing.decoder.[record.code] <- record.data)
                    success <- true
                | "run_analysis" ->
                    let message = RunAnalysisMessage.FromJson dataStream
                    graph.SetStarts message.starts
                    performParsing()
                    success <- true
                | _ -> ()
            with e -> printfn "%s" e.Message
            
            if success then
                writer.WriteLine("success")
            else
                writer.WriteLine("failure")
                
            writer.Flush()
                
        reader.Close()
        stream.Close()
        client.Close()
        socket.Stop()           
        
        
        
        
        
