//this file was generated by GNESCC
//source grammar:binop.yrd
//date:16.12.2011 11:56:01

module GNESCC.Regexp_binop

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

let binop childsLst = 
    let str = buildStr childsLst
    let idxValMap = buildIndexMap childsLst
    let re = new Regex("((;0;)(;5;)(;0;))")
    let elts =
        let res = re.Match(str)
        if Seq.fold (&&) true [for g in res.Groups -> g.Success]
        then res.Groups
        else (new Regex("((;0;)(;5;)(;0;))",RegexOptions.RightToLeft)).Match(str).Groups
    let e2 =
        idxValMap.[elts.[4].Captures.[0].Index] |> RELeaf
    let e1 =
        idxValMap.[elts.[3].Captures.[0].Index] |> RELeaf
    let e0 =
        idxValMap.[elts.[2].Captures.[0].Index] |> RELeaf
    RESeq [e0; e1; e2]
let s childsLst = 
    let str = buildStr childsLst
    let idxValMap = buildIndexMap childsLst
    let re = new Regex("(((;2;))|((;7;)))")
    let elts =
        let res = re.Match(str)
        if Seq.fold (&&) true [for g in res.Groups -> g.Success]
        then res.Groups
        else (new Regex("(((;2;))|((;7;)))",RegexOptions.RightToLeft)).Match(str).Groups
    if elts.[3].Value = ""
    then
        let e2 =
            let e0 =
                idxValMap.[elts.[5].Captures.[0].Index] |> RELeaf
            RESeq [e0]
        None, Some (e2)
    else
        let e1 =
            let e0 =
                idxValMap.[elts.[3].Captures.[0].Index] |> RELeaf
            RESeq [e0]
        Some (e1),None
    |> REAlt


let ruleToRegex = dict [|(0,s); (2,binop)|]
