﻿open Yard.Generators.GLL
open Yard.Generators.Common.ASTGLLFSA
open Yard.Generators.GLL.ParserCommon
open AbstractAnalysis.Common
open Yard.Frontends.YardFrontend
open YC.API
open AbstractParser
open System.Collections.Generic

let loadGraph graphFile tokenizer =
    let data = System.IO.File.ReadAllLines(graphFile)
    let startVrts = data.[0].Split ' ' |> Array.map int
    let edges = 
        data.[1..] |> Array.map (fun s -> s.Split ' ' |> fun a -> new ParserEdge<_>(int a.[0], int a.[2], a.[1]))
    let graph = new SimpleInputGraph<_>(startVrts, [||], tokenizer)
    graph.AddVerticesAndEdgeRange edges |> ignore
    graph

(*
ba: ASSERT
ca: ASSERT

s0: C s0 RT s0 | G s0 RL s0 | ca s0 | ca | eps

s1: C s1 RT s1 | G s0 RL s1 | eps

[<Start>]
s: s1 ba | ba s1 | ba s | s ba| s1 s | s s1 | ba | C s RT s1 | C s1 RT s | C s RT s 
*)
let loadGrammar grammarFile = 
    let data = System.IO.File.ReadAllLines grammarFile
    let getLocks = int <| data.[0].Trim()    
    let calls = int <| data.[1].Trim()
    let asserts = int <| data.[2].Trim()
    let assertsGrm = [|0 .. asserts - 1|] |> Array.map (fun i -> "A" + string i) |> String.concat " | "
    let mutable grm = 
        "ba: " +  assertsGrm + " \n"
      + "ca: " +  assertsGrm + " \n"
      + "[<Start>]\n"
      + "s: s1 ba | ba s1 | ba s | s ba | s s1 | s1 s | ba \n"
    let genBrs tmplt count =
        [|0..count - 1|] 
        |> Array.map (fun i -> sprintf tmplt i i)
        |> String.concat "\n    |" 

    let s1Head = "\ns1: {} \n"
    let s1Calls = genBrs " C%i s1 RT%i s1" calls
    let s1Locks = genBrs " G%i s0 RL%i s1" getLocks
    

    let sCalls1 = genBrs " C%i s RT%i s1" calls
    let sCalls2 = genBrs " C%i s1 RT%i s" calls
    let sCalls3 = genBrs " C%i s RT%i s" calls
    
    let s0Calls = genBrs " C%i s0 RT%i s0" calls
    let s0Locks = genBrs " G%i s0 RL%i s0" getLocks
    let s0Head = "\ns0: {} | ca s0 | ca \n"
    
    grm + "    |" + sCalls1 + "\n    |" + sCalls2 + "\n    |" + sCalls3 + "\n" + s0Head + "    |" + s0Calls + "\n    |" + s0Locks + "\n"
    + s1Head + "    |" + s1Calls +  "    |" + s1Locks + "\n"

let singlePathToBadAssertForRoot (root: INode) (badAssertInt : int) (intToString : Dictionary<_,_>) : seq<string> =
    let results = new Dictionary<INode, _>() 
    let rec getPath : INode -> seq<string> = function
        | :? IntermidiateNode as i ->
            let isGot,value = results.TryGetValue i
            if isGot
            then 
                Seq.empty
            else
                results.Add(i, null)
                getPath i.First
        | :? TerminalNode as t ->
            let res = new List<_>()
            if t.Name <> -1<token> 
            then 
                seq{yield intToString.[int t.Name]}
            else
                Seq.empty
        | :? PackedNode as p ->
            let rightPath = getPath p.Right
            let leftPath = getPath p.Left
            Seq.append leftPath rightPath
        | :? NonTerminalNode as n ->
            let isGot,value = results.TryGetValue n
            if isGot
            then 
                Seq.empty
            else
                results.Add(n, null)
                if (int n.Name = badAssertInt)
                then
                    seq{yield intToString.[int n.Name]}
                else
                    getPath n.First
        | :? EpsilonNode as eps ->
            Seq.empty
        | _ -> failwith "Unexpected node type. rly?"

    getPath root

[<EntryPoint>]
let main argv =
    //let graph = ".\\..\\..\\graph"
    //let grammarFile = ".\\..\\..\\grammar"
    let graph = argv.[0]
    let grammarFile = argv.[1]
    
    let grammar = loadGrammar grammarFile    
    
    System.IO.File.WriteAllText("resultGrammar.yrd", grammar)

    let parserSource =
        let fe = new YardFrontend()
        let gen = new GLL()
        GenerateFromStrToObj grammar fe gen None Seq.empty [||] :?> ParserSourceGLL
    
    let tokenizer str =
        str |> parserSource.StringToToken |> int

    let inputGraph = loadGraph graph tokenizer

    let treesForEachInitialInputPosition = getAllSPPFRootsAsINodes parserSource inputGraph
    let badAssertInt = 
        let k = ref 0
        for kvp in parserSource.IntToString do
            if kvp.Value = "ba"
            then k := kvp.Key
        !k

    let result = 
        singlePathToBadAssertForRoot treesForEachInitialInputPosition.[0] badAssertInt parserSource.IntToString
        |> Array.ofSeq
    
    let outputFile = argv.[2]
    System.IO.File.WriteAllLines(outputFile, result)    
    
    0 // return an integer exit code