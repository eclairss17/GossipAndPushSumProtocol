#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"
#time "on"

open System
open Akka.Actor
open Akka.FSharp

let system =
    System.create "system" (Configuration.defaultConfig ())

type MessageObject =
    | Start of int * Map<int, IActorRef> * int
    | BeginGossip of string
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

    let rec loop () =
        actor {
            let! msg = mailbox.Receive()

            match msg with
            | Start (id, nodeMap, actorcount) ->
                actorsCount <- actorcount
                nodeId <- id

                if (nodeId = 1) then
                    neighborsMap <- neighborsMap.Add(0, nodeMap.[2])
                elif (nodeId = actorcount) then
                    neighborsMap <- neighborsMap.Add(0, nodeMap.[actorcount - 1])
                else
                    neighborsMap <-
                        neighborsMap
                            .Add(0, nodeMap.[nodeId - 1])
                            .Add(1, nodeMap.[nodeId + 1])

                neighborCount <- neighborsMap.Count

            | BeginGossip (msg) ->
                count <- count + 1

                if isFirst then
                    isFirst <- false
                    mailbox.Self <! ContinueGossip msg

            | ContinueGossip (msg) ->
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
    // printfn "Gossip Line initiating for %i nodes:" totalNodes

    num <- totalNodes

    for var = 1 to num do
        nodeMap <- nodeMap.Add(var, spawn system (sprintf "actor%i" var) player)

    for var = 1 to num do
        let state = nodeMap
        nodeMap.[var] <! Start(var, state, num)

    let timer = System.Diagnostics.Stopwatch.StartNew()

    nodeMap.[arbitrary.Next(1, num + 1)]
    <! BeginGossip "Bangtan"

    // printfn "=========================================>Gossip inintiated!!----------------------------"
    let mutable temp = true

    while temp do
        if x >= num then temp <- false

    timer.Stop()

    timer.Elapsed.TotalMilliseconds





