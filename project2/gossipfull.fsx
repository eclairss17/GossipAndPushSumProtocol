module GossipFull
#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"
#time "on"

open System
open Akka.Actor
open Akka.FSharp

let system =
    System.create "system" (Configuration.defaultConfig ())

type MessageObject =
    | Start of int * Map<int, IActorRef> * int*int
    // | Callback of int
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
            | Start (id, nodeMap, actorcount,fail) ->
                actorsCount <- actorcount
                nodeId <- id

                neighborsMap<- nodeMap

                neighborCount <- neighborsMap.Count

            // | Callback (complete) ->
            //     x <- x + 1
                // printfn "complete for %i" nodeId

            //condition with node, without node and count
            | BeginGossip (msg) ->
                count <- count + 1

                if isFirst then
                    isFirst <- false
                    mailbox.Self <! ContinueGossip msg

            | ContinueGossip (msg) ->
                // count<- count + 1
                iteration <- iteration + 2
                if (iteration % 8 = 0) then
                    nextnodeIndex <- arbitrary.Next(1, neighborCount+1)
                    neighborsMap.[nextnodeIndex] <! BeginGossip msg

                if count >= 10 && completedOnce then
                    completedOnce <- false
                    flag <- false
                    x <- x + 1
                    // mailbox.Self <! Callback complete


                if flag then
                    let mutable newindex = arbitrary.Next(1, neighborCount+1)

                    while newindex = nodeId do
                        newindex <- arbitrary.Next(1, neighborCount+1)

                    // nextnodeIndex <- newindex

                    neighborsMap.[newindex]
                    <! BeginGossip "Hi please work"

                mailbox.Self <! ContinueGossip "Hi please work"

            return! loop ()
        }

    loop ()

let processStart (totalNodes: int) =
    // printfn "Gossip Line initiating for %i nodes:" totalNodes

    num <- totalNodes

    let failnodes = int(totalNodes/10)
    

    for var = 1 to num do
        nodeMap <- nodeMap.Add(var, spawn system (sprintf "actor%i" var) player)

    for var = 1 to num do
        let state = nodeMap
        nodeMap.[var] <! Start(var, state, num,failnodes)

    let timer = System.Diagnostics.Stopwatch.StartNew()

    nodeMap.[arbitrary.Next(1, num + 1)]
    <! BeginGossip "Bangtan"

    // printfn "=========================================>Gossip inintiated!!----------------------------"
    let mutable temp = true

    while temp do
        if x >= num then temp <- false

    timer.Stop()

    timer.Elapsed.TotalMilliseconds





//condition that
// player <! Callback nodeId
// nextnodeIndex<-arbitrary.Next(0,neighborCount)


//start neighbor gossip
// nodeMap.[nextnodeIndex] <! mailbox.Self <! BeginGossip msg

// else if count>10 then
//     flag<-false


// increment<-increment+1
// temp<- nodeId

//call once an external actor and tell him that we are done with one node(aint coming back to this) lets go to next
//     let newindex = nextnodeIndex
//     while newindex = nextnodeIndex do
//         newindex = arbitrary.Next(0,neighborCount)
//     nextnodeIndex<- newindex
//     nodeMap.[nextnodeIndex] <! BeginGossip msg
// if (increment = actorsCount) then
