//this tables was generated by GNESCC
//source grammar:../../../Tests/GNESCC/test_alt_in_cls/test_alt_in_cls.yrd
//date:10/9/2011 12:01:29 AM

module Yard.Generators.GNESCCGenerator.Tables_alt_in_cls

open Yard.Generators.GNESCCGenerator
open Yard.Generators.GNESCCGenerator.CommonTypes

type symbol =
    | T_PLUS
    | T_MINUS
    | NT_s
    | NT_gnesccStart
let getTag smb =
    match smb with
    | T_PLUS -> 6
    | T_MINUS -> 5
    | NT_s -> 4
    | NT_gnesccStart -> 2
let getName tag =
    match tag with
    | 6 -> T_PLUS
    | 5 -> T_MINUS
    | 4 -> NT_s
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
     [| false; false |];
     [| false; false |]; |]
let gotoTable =
  [| [| Some 1; None |];
     [| None; None |];
     [| None; None |];
     [| None; None |]; |]
let actionTable = 
  [| [| [Shift 3]; [Shift 2]; [Error]; [Reduce 1] |];
     [| [Error]; [Error]; [Error]; [Accept] |];
     [| [Shift 3]; [Shift 2]; [Error]; [Reduce 1] |];
     [| [Shift 3]; [Shift 2]; [Error]; [Reduce 1] |]; |]
let tables = 
  {StartIdx=startKernelIdxs
   SymbolIdx=symbolIdx
   GotoTable=gotoTable
   ActionTable=actionTable
   IsStart=isStart
   ProdToNTerm=prodToNTerm}
