//this tables was generated by GNESCC
//source grammar:../../../Tests/GNESCC/claret/braces_2/test_simple_braces_2.yrd
//date:10/9/2011 12:01:28 AM

module Yard.Generators.GNESCCGenerator.Tables_simple_braces_2

open Yard.Generators.GNESCCGenerator
open Yard.Generators.GNESCCGenerator.CommonTypes

type symbol =
    | T_RBR
    | T_LBR
    | NT_start
    | NT_gnesccStart
let getTag smb =
    match smb with
    | T_RBR -> 6
    | T_LBR -> 5
    | NT_start -> 4
    | NT_gnesccStart -> 2
let getName tag =
    match tag with
    | 6 -> T_RBR
    | 5 -> T_LBR
    | 4 -> NT_start
    | 2 -> NT_gnesccStart
    | _ -> failwith "getName: bad tag."
let prodToNTerm = 
  [| 1; 0 |];
let symbolIdx = 
  [| 2; 3; 1; 3; 0; 1; 0 |];
let startKernelIdxs =  [0]
let isStart =
  [| [| true; true |];
     [| false; false |];
     [| false; true |];
     [| false; false |];
     [| false; false |]; |]
let gotoTable =
  [| [| Some 1; None |];
     [| None; None |];
     [| Some 3; None |];
     [| None; None |];
     [| None; None |]; |]
let actionTable = 
  [| [| [Error]; [Shift 2]; [Error]; [Reduce 1] |];
     [| [Error]; [Error]; [Error]; [Accept] |];
     [| [Error]; [Reduce 1; Shift 2]; [Error]; [Error] |];
     [| [Shift 4]; [Error]; [Error]; [Error] |];
     [| [Error]; [Reduce 1; Shift 2]; [Error]; [Reduce 1] |]; |]
let tables = 
  {StartIdx=startKernelIdxs
   SymbolIdx=symbolIdx
   GotoTable=gotoTable
   ActionTable=actionTable
   IsStart=isStart
   ProdToNTerm=prodToNTerm}
