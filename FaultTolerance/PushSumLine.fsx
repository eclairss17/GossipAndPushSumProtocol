module PushSumLine

#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"

//#r "bin/Debug/netcoreapp3.1/Akka.dll"
//#r "bin/Debug/netcoreapp3.1/Akka.FSharp.dll"
#time "on"

open System
open Akka
open Akka.Actor
// open Akka.Configuration
open Akka.FSharp

let system =
    System.create "system" (Configuration.defaultConfig ())

let arbitrary = Random()

type Command1 =
    | Start of decimal * decimal * int * Map<int, IActorRef>*Set<int>
    | ContinueGossip of decimal * decimal

let mutable nodeMap = Map.empty
let mutable num = -1
let mutable nodeCompleteSet = Set.empty

let player (mailbox: Actor<_>) =
    let mutable actorsCount = 0
    let mutable neighborsMap = Map.empty
    let mutable nodeId = 0
    let mutable neighborCount = 0
    let mutable nextnodeIndex = 0
    let mutable s = -1.0 |> decimal
    let mutable w = 1.0 |> decimal
    let mutable ratio = 0.0 |> decimal
    let mutable delta = 0.0000000001 |> decimal
    let mutable iteration = 0
    let mutable flag = true
    let mutable count = 0

    let tempTwo = 2.0 |> decimal



    let rec loop () =
        actor {
            let! msg = mailbox.Receive()

            match msg with
            | Start (s_incoming, w_incoming, id, nodeMap,fail) ->
                s <- s_incoming
                w <- w_incoming
                nodeId <- id

                if (nodeId = 1) then
                    neighborsMap <- neighborsMap.Add(0, nodeMap.[2])
                elif (nodeId = nodeMap.Count) then
                    neighborsMap <- neighborsMap.Add(0, nodeMap.[nodeMap.Count - 1])
                else
                    neighborsMap <-
                        neighborsMap
                            .Add(0, nodeMap.[nodeId - 1])
                            .Add(1, nodeMap.[nodeId + 1])

                neighborCount <- neighborsMap.Count
                
                if (fail.Contains(nodeId)) then
                    neighborsMap <- neighborsMap.Remove(nodeId)
                ratio <- s / w

            | ContinueGossip (sIncoming, wIncoming) ->
                iteration <- iteration + 2

                if (iteration % 400 = 0) && flag then
                    nextnodeIndex <- arbitrary.Next(0, neighborCount)
                    printfn "Gosssiping Node Id----> %i" nodeId

                    neighborsMap.[nextnodeIndex]
                    <! ContinueGossip(0 |> decimal, 0|> decimal)

                s <- s + sIncoming
                w <- w + wIncoming
                let mutable ratioNew = s / w
                // printfn
                if (Math.Abs(ratioNew - ratio) < delta) && count < 3 then
                    count <- count + 1
                    printfn " count increasing for nodeId %i" nodeId
                else if flag then
                    count <- 0
                    ratio <- ratioNew

               
                if (count >= 3) then
                    if not (nodeCompleteSet.Contains(nodeId)) then
                        nodeCompleteSet <- nodeCompleteSet.Add(nodeId)
                        printfn "complete for %i " nodeId

                    flag <- false

                // if flag then
                let mutable newindex = arbitrary.Next(0, neighborCount)

                s <- s / tempTwo
                w <- w / tempTwo

                while newindex = nodeId
                      && not (nodeCompleteSet.Contains(newindex)) do
                    newindex <- arbitrary.Next(0, neighborCount)

                // printfn "Gossiping for node %i" nodeId
                neighborsMap.[newindex] <! ContinueGossip(s, w)

            // mailbox.Self <! ContinueGossip(s, w)

            return! loop ()
        }

    loop ()

let processStart (totalNodes: int) =
    printfn "Push Sum Line initiating for %i nodes:" totalNodes

    num <- totalNodes
    let mutable tempSet = Set.empty



    for var = 1 to num do
        nodeMap <- nodeMap.Add(var, spawn system (sprintf "actor%i" var) player)

    let failnodes = int (totalNodes / 10)

    for node = 0 to failnodes do
        tempSet <- tempSet.Add(arbitrary.Next(1, num + 1))
    for var = 1 to num do
        let state = nodeMap
        let sum = var |> decimal
        let weight = 1.0 |> decimal
        nodeMap.[var] <! Start(sum, weight, var, state,tempSet)



    let timer = System.Diagnostics.Stopwatch.StartNew()

    let s1 = 0.0 |> decimal
    let w1 = 0.0 |> decimal

    nodeMap.[arbitrary.Next(1, num + 1)]
    <! ContinueGossip(s1, w1)

    printfn "=========================================>Push Sum protocol inintiated!!----------------------------"
    let mutable temp = true

    while temp do
        if nodeCompleteSet.Count >= num then
            temp <- false

    timer.Stop()
    timer.Elapsed.TotalMilliseconds
