//this file was generated by GNESCC
//source grammar:../../../Tests/GNESCC/claret/braces_2/test_simple_braces_2.yrd
//date:10/9/2011 12:01:28 AM

module GNESCC.Regexp_simple_braces_2

open Yard.Generators.GNESCCGenerator
open System.Text.RegularExpressions

let buildIndexMap kvLst =
    let ks = List.map (fun (x:string,y) -> x.Length + 2,y) kvLst
    List.fold (fun (bl,blst) (l,v) -> bl+l,((bl,v)::blst)) (0,[]) ks
    |> snd
    |> dict

let buildStr kvLst =
    let sep = ";;"
    List.map fst kvLst 
    |> String.concat sep
    |> fun s -> ";" + s + ";"

let start childsLst = 
    let str = buildStr childsLst
    let idxValMap = buildIndexMap childsLst
    let re = new Regex("((((;5;)(;1;)(;6;)))*)")
    let elts = re.Match(str).Groups
    let e0 =
        let ofset = ref 0
        let e i =
            let str = elts.[2].Captures.[i].Value
            let re = new Regex("((;5;)(;1;)(;6;))")
            let elts = re.Match(str).Groups
            let res =
                let e2 =
                    idxValMap.[!ofset + elts.[4].Captures.[0].Index] |> RELeaf
                let e1 =
                    idxValMap.[!ofset + elts.[3].Captures.[0].Index] |> RELeaf
                let e0 =
                    idxValMap.[!ofset + elts.[2].Captures.[0].Index] |> RELeaf
                RESeq [e0; e1; e2]
            ofset := !ofset + str.Length
            res
        REClosure [for i in [0..elts.[2].Captures.Count-1] -> e i]

    RESeq [e0]

let ruleToRegex = dict [|(1,start)|]

