module Gossip3D

#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"
#time "on"

open System
open Akka.Actor
open Akka.FSharp

let system =
    System.create "system" (Configuration.defaultConfig ())

type MessageObject =
    | Start of int * Map<int, IActorRef> * int * int
    | BeginGossip of string
    // | Callback of int
    | ContinueGossip of string

let arbitrary = Random()

let mutable nodeMap = Map.empty
let mutable num = -1
let mutable x = 0

let player (mailbox: Actor<_>) =
    let mutable actorsCount = 0
    let mutable neighborsMap = Map.empty
    let mutable nodeId = 0
    let mutable neighborCount = 0
    let mutable count = 0
    let mutable completedOnce = true
    let mutable iteration = 0
    let mutable flag = true
    let mutable isFirst = true
    let mutable nextnodeIndex = 0
    // let mutable edgeCount = 0

    let rec loop () =
        actor {
            let! msg = mailbox.Receive()

            match msg with
            | Start (id, nodeMap, actorcount, edgecount) ->
                actorsCount <- actorcount
                nodeId <- id

                let mutable oneSize = edgecount
                let mutable size = Math.Pow(float (edgecount), 2.0) |> int
                let mutable k = 0

                // printfn "For loop ke andar for actor %i" nodeId
                // printfn "Neighbors length is %i" neighborsMap.Count

                if ((nodeId - 1) >= oneSize * int (nodeId / oneSize)) then
                    neighborsMap <- neighborsMap.Add(k, nodeMap.[nodeId - 1])
                    k <- k + 1
                    // printfn " Pass 1"
                    // printfn "No of neighbors after pass 1 are %i" neighborsMap.Count

                if ((nodeId + 1) < oneSize * (int (nodeId / oneSize) + 1)) then
                    neighborsMap <- neighborsMap.Add(k, nodeMap.[nodeId + 1])
                    k <- k + 1
                    // printfn " Pass 2"
                    // printfn "No of neighbors after pass 2 are %i" neighborsMap.Count

                if ((nodeId - oneSize) >= size * int (nodeId / size)) then
                    neighborsMap <- neighborsMap.Add(k, nodeMap.[nodeId - oneSize])
                    k <- k + 1
                    // printfn " Pass 3"
                    // printfn "No of neighbors after pass 3 are %i" neighborsMap.Count

                if ((nodeId + oneSize) < size * (int (nodeId / size) + 1)) then
                    neighborsMap <- neighborsMap.Add(k, nodeMap.[nodeId + oneSize])
                    k <- k + 1
                    // printfn " Pass 4"
                    // printfn "No of neighbors after pass 4 are %i" neighborsMap.Count

                if (nodeId - size) >= 0 then
                    neighborsMap <- neighborsMap.Add(k, nodeMap.[nodeId - size])
                    k <- k + 1
                    // printfn " Pass 5 "
                    // printfn "No of neighbors after pass 5 are %i" neighborsMap.Count

                if (nodeId + size) < actorsCount then
                    neighborsMap <- neighborsMap.Add(k, nodeMap.[nodeId + size])
                    k <- k + 1
                    // printfn " Pass 6"
                    // printfn "No of neighbors after pass 6 are %i" neighborsMap.Count


                neighborCount <- neighborsMap.Count



            | BeginGossip (msg) ->
                // printfn ""
                count <- count + 1

                if isFirst then
                    isFirst <- false
                    mailbox.Self <! ContinueGossip msg

            | ContinueGossip (msg) ->
                // count<- count + 1
                iteration <- iteration + 2

                if (iteration % 8 = 0) then
                    nextnodeIndex <- arbitrary.Next(0, neighborCount)
                    neighborsMap.[nextnodeIndex] <! BeginGossip msg

                if count >= 10 && completedOnce then
                    completedOnce <- false
                    flag <- false
                    x <- x + 1
                
                if flag then
                    let mutable newindex = arbitrary.Next(0, neighborCount)


                    neighborsMap.[newindex]
                    <! BeginGossip "Hi please work"

                mailbox.Self <! ContinueGossip "Hi please work"

            return! loop ()
        }

    loop ()

let processStart (totalNodes: int) =
    // printfn "Gossip Line initiating for %i nodes in 3D Grid:" totalNodes

    num <- totalNodes

    let edgeC = (Math.Cbrt(num |> float)) |> int
    // printfn "cube root is: %i" x
    for var = 0 to num - 1 do
        nodeMap <- nodeMap.Add(var, spawn system (sprintf "actor%i" var) player)

    for var = 0 to num-1 do
        let state = nodeMap
        nodeMap.[var] <! Start(var, state, num, edgeC)



    let timer = System.Diagnostics.Stopwatch.StartNew()

    nodeMap.[arbitrary.Next(0, num)]
    <! BeginGossip "Bangtan"

    // printfn "=========================================>Gossip inintiated!!----------------------------"
    let mutable temp = true


    while temp do
        if x >= num then temp <- false

    timer.Stop()

    timer.Elapsed.TotalMilliseconds




