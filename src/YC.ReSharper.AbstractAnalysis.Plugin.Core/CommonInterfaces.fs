﻿module YC.SDK.CommonInterfaces

open Microsoft.FSharp.Collections
open Yard.Generators.RNGLR.OtherSPPF
open QuickGraph
open QuickGraph.Algorithms
open AbstractAnalysis.Common
open Yard.Generators.RNGLR.AST
open YC.FST.AbstractLexing.Interpreter
open YC.FST.FstApproximation

type TreeGenerationState<'node> = 
    | Start
    | InProgress of 'node * int list
    | End of 'node

type LexingFinishedArgs<'node> (tokens : ResizeArray<'node>, lang:string) =
     inherit System.EventArgs()
     member this.Tokens = tokens
     member this.Lang = lang

type ParsingFinishedArgs(lang:string) = 
    inherit System.EventArgs()
    member this.Lang = lang


exception LexerError of string*obj

type InjectedLanguageAttribute(language : string) = 
    inherit System.Attribute()

    member this.language = language

[<Interface>]
type IInjectedLanguageModule<'br,'range,'node> =    
     abstract Name: string
     abstract LexingFinished: IEvent<LexingFinishedArgs<'node>>
     abstract ParsingFinished: IEvent<ParsingFinishedArgs>
     abstract XmlPath: string
     abstract GetNextTree: int -> 'node*bool
     abstract GetForestWithToken: 'range -> ResizeArray<'node>
     abstract GetPairedRanges: int -> int -> 'range -> bool -> ResizeArray<'range>
     abstract Process
        : Appr<'br> -> ResizeArray<string * 'range> * ResizeArray<string * 'range>


type Processor<'TokenType,'br, 'range, 'node>  when 'br:equality and  'range:equality and 'node:null
    (
        tokenize: Appr<'br> -> ParserInputGraph<'TokenType>
        , parse, translate, tokenToNumber: 'TokenType -> int, numToString: int -> string, tokenData: 'TokenType -> obj, tokenToTreeNode, lang, calculatePos:_->seq<'range>
        , getDocumentRange:'br -> 'range
        , printAst: Tree<'TokenType> -> string -> unit
        , printOtherAst: OtherTree<'TokenType> -> string -> unit) as this =

    let lexingFinished = new Event<LexingFinishedArgs<'node>>()
    let parsingFinished = new Event<ParsingFinishedArgs>()
    let mutable forest: list<Tree<'TokenType> * _> = [] 
    let mutable otherForest : list<OtherTree<'TokenType>> = []

    let mutable generationState : TreeGenerationState<'node> = Start
    
    let prepareToHighlighting (graphOpt : ParserInputGraph<'token> option) tokenToTreeNode = 
        if graphOpt.IsSome
        then
            let tokensList = new ResizeArray<_>()

            let inGraph = graphOpt.Value 
            inGraph.TopologicalSort()
            |> Seq.iter 
                (fun vertex -> 
                        inGraph.OutEdges vertex 
                        |> Seq.iter (fun edge -> tokensList.Add <| tokenToTreeNode edge.Tag)
                )

            lexingFinished.Trigger(new LexingFinishedArgs<'node>(tokensList, lang))

    let processLang graph addLError addPError =
        let tokenize g =
            try 
                tokenize g
                |> Some 
            with
            | LexerError(t,grToken) ->
                (t, (grToken :?> GraphTokenValue<'br>).Edges |> Seq.nth 0 |> fun e -> e.BackRef |> getDocumentRange)
                |> addLError
                None

        
        let tokenizedGraph = tokenize graph 
        prepareToHighlighting tokenizedGraph tokenToTreeNode 

        tokenizedGraph 
        |> Option.map parse
        |> Option.iter
            (function 
                | Yard.Generators.RNGLR.Parser.Success(tree, _, errors) ->
                    forest <- (tree, errors) :: forest
                    otherForest <- new OtherTree<'TokenType>(tree) :: otherForest
                    parsingFinished.Trigger (new ParsingFinishedArgs (lang))
                | Yard.Generators.RNGLR.Parser.Error(_,tok,_,_,_) -> tok |> Array.iter addPError 
            )

    let getNextTree index : TreeGenerationState<'node> = 
        if forest.Length <= index
        then
            generationState <- End(null)
        else
            let mutable curSppf, errors = List.nth forest index
            let unprocessed = 
                match generationState with
                | Start ->   Array.init curSppf.TokensCount (fun i -> i) |> List.ofArray
                | InProgress (_, unproc) ->  unproc
                | _ -> failwith "Unexpected state in treeGeneration"
                
            let nextTree, unproc = curSppf.GetNextTree unprocessed (fun _ -> true)
            
            let treeNode = this.TranslateToTreeNode nextTree errors
            
            if unproc.IsEmpty
            then generationState <- End (treeNode)
            else generationState <- InProgress (treeNode, unproc) 

        generationState

    let getForestWithToken range = 
        let res = new ResizeArray<'node>()
        for ast, errors in forest do
            let trees = ast.GetForestWithToken range <| (this.TokenToPos calculatePos)
            for tree in trees do
                let treeNode = (this.TranslateToTreeNode tree errors)
                res.Add treeNode
        res

    member this.GetNextTree index =         
        let state = getNextTree 0//index
        match state with
        | InProgress (treeNode, _) -> treeNode, false
        | End (treeNode) -> 
            generationState <- Start
            treeNode, true
        | _ -> failwith "Unexpected state in tree generation"

    member this.TokenToPos calculatePos (token : 'TokenType)= 
        let data : string * GraphTokenValue<'br> = unbox <| tokenData token
        calculatePos <| snd data

    member this.GetForestWithToken range = getForestWithToken range

    member this.GetPairedRanges leftNumber rightNumber range toRight = 
        let tokens = new ResizeArray<_>()
        let tokToPos = this.TokenToPos calculatePos

        for otherTree in otherForest do
            tokens.AddRange <| otherTree.FindAllPair leftNumber rightNumber range toRight tokenToNumber tokToPos
        
        let ranges = new ResizeArray<_>()
        tokens 
        |> ResizeArray.iter (fun token -> 
            Seq.iter (fun r -> ranges.Add r) <| tokToPos token)

        ranges

    member this.TranslateToTreeNode nextTree errors = (Seq.head <| translate nextTree errors)
    
    member this.Process (graph:Appr<'br>) = 
        let parserErrors = new ResizeArray<_>()
        let lexerErrors = new ResizeArray<_>()

        let addError tok =
            let e tokName lang (grToken: GraphTokenValue<'br>) = 
                grToken |> calculatePos
                |> Seq.iter
                    (fun br -> parserErrors.Add <| ((sprintf "%A(%A)" tokName lang), br))
            let name = tok |> (tokenToNumber >>  numToString)
            let (language : string), br = tokenData tok :?> _
            e name language br
        
        
        processLang graph lexerErrors.Add addError

        lexerErrors, parserErrors

    [<CLIEvent>]
    member this.LexingFinished = lexingFinished.Publish

    [<CLIEvent>]
    member this.ParsingFinished = parsingFinished.Publish
