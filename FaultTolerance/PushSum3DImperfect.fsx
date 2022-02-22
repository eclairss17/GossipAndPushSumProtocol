module PushSum3DImperfect

#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"
#time "on"

open System
open Akka
open Akka.Actor
open Akka.FSharp

let system =
    System.create "system" (Configuration.defaultConfig ())

let arbitrary = Random()

type Command1 =
    | Start of decimal * decimal * int * Map<int, IActorRef>*int
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
    let mutable randomSet = Set.empty


    let tempTwo = 2.0 |> decimal



    let rec loop () =
        actor {
            let! msg = mailbox.Receive()

            match msg with
            | Start (s_incoming, w_incoming, id, nodeMap,edgecount) ->
                s <- s_incoming
                w <- w_incoming
                nodeId <- id
                actorsCount<-nodeMap.Count
                
                let mutable oneSize = edgecount
                let mutable size = Math.Pow(float (edgecount), 2.0) |> int
                let mutable k = 0


                if ((nodeId - 1) >= oneSize * int (nodeId / oneSize)) then
                    neighborsMap <- neighborsMap.Add(k, nodeMap.[nodeId - 1])
                    randomSet<-randomSet.Add(nodeId-1)
                    k <- k + 1

                if ((nodeId + 1) < oneSize * (int (nodeId / oneSize) + 1)) then
                    neighborsMap <- neighborsMap.Add(k, nodeMap.[nodeId + 1])
                    randomSet<-randomSet.Add(nodeId+1)
                    k <- k + 1

                if ((nodeId - oneSize) >= size * int (nodeId / size)) then
                    neighborsMap <- neighborsMap.Add(k, nodeMap.[nodeId - oneSize])
                    randomSet<-randomSet.Add(nodeId-oneSize)
                    k <- k + 1

                if ((nodeId + oneSize) < size * (int (nodeId / size) + 1)) then
                    neighborsMap <- neighborsMap.Add(k, nodeMap.[nodeId + oneSize])
                    randomSet<-randomSet.Add(nodeId+oneSize)
                    k <- k + 1

                if (nodeId - size) >= 0 then
                    neighborsMap <- neighborsMap.Add(k, nodeMap.[nodeId - size])
                    randomSet<-randomSet.Add(nodeId-size)
                    k <- k + 1

                if (nodeId + size) < actorsCount then
                    neighborsMap <- neighborsMap.Add(k, nodeMap.[nodeId + size])
                    randomSet<-randomSet.Add(nodeId+size)
                    k <- k + 1

                neighborCount <- neighborsMap.Count
                let mutable randomNode = arbitrary.Next(0,actorsCount)
                while randomNode = nodeId && randomSet.Contains(randomNode) do
                    randomNode <- arbitrary.Next(0,actorsCount)
                neighborsMap <- neighborsMap.Add(k,nodeMap.[randomNode])
                neighborCount <- neighborsMap.Count


            | ContinueGossip (sIncoming, wIncoming) ->
                iteration <- iteration + 2
                s <- s + sIncoming
                w <- w + wIncoming
                let mutable ratioNew = s / w
                
                if (Math.Abs(ratioNew - ratio) < delta) then
                    count <- count + 1
                else
                    count <- 0
                    ratio <- ratioNew

                s <- s / tempTwo
                w <- w / tempTwo
                if (iteration%100 = 0) && flag then 
                    nextnodeIndex<- arbitrary.Next(0,neighborCount)
                    neighborsMap.[nextnodeIndex] <! ContinueGossip (s,w)

                if (count >= 3) then
                    if not (nodeCompleteSet.Contains(nodeId)) then
                        nodeCompleteSet <- nodeCompleteSet.Add(nodeId)
                        // printfn "complete for %i" nodeId

                    // flag <- false

                // if flag then
                let mutable newindex = arbitrary.Next(0, neighborCount)

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
    // printfn "PushSum 3D imperfect initiating for %i nodes:" totalNodes

    num <- totalNodes

    for var = 0 to num-1 do
        nodeMap <- nodeMap.Add(var, spawn system (sprintf "actor%i" var) player)

    let edgeC = (Math.Cbrt(num |> float)) |> int    
    for var = 0 to num-1 do
        let state = nodeMap
        let sum = var |> decimal
        let weight = 1.0 |> decimal
        nodeMap.[var] <! Start(sum, weight, var, state, edgeC)



    let timer = System.Diagnostics.Stopwatch.StartNew()

    let s1 = 0.0 |> decimal
    let w1 = 0.0 |> decimal

    nodeMap.[arbitrary.Next(0, num )]
    <! ContinueGossip(s1, w1)

    // printfn "=========================================>Push Sum protocol inintiated!!----------------------------"
    let mutable temp = true

    while temp do
        if nodeCompleteSet.Count >= num then
            temp <- false

    timer.Stop()

    timer.Elapsed.TotalMilliseconds
