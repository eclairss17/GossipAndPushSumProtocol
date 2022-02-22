#load "gossip.fsx"
#load "PushSumLine.fsx"
#load "gossipfull.fsx"
#load "PushSumFull.fsx"
#load "gossip3D.fsx"
#load "PushSum3D.fsx"
#load "gossipImp3D.fsx"
#load "PushSum3DImperfect.fsx"

let main () =
    let totalNodes = int fsi.CommandLineArgs.[1]
    let protocol = string fsi.CommandLineArgs.[2]
    let topology = string fsi.CommandLineArgs.[3]
    let mutable time = 0.0

    if protocol = "gossip" && topology = "line" then
        time <- Gossip.processStart totalNodes
        printfn "%f" time

    if protocol = "gossip" && topology = "full" then
        time <- GossipFull.processStart totalNodes
        printfn "%f" time

    if protocol = "gossip" && topology = "3D" then
        time <- Gossip3D.processStart totalNodes
        printfn "%f" time

    if protocol = "gossip" && topology = "3DImperfect" then
        time <- Gossip3DImp.processStart totalNodes
        printfn "%f" time

    if protocol = "pushsum" && topology = "3D" then
        time <- PushSum3D.processStart totalNodes
        printfn "%f" time

    if protocol = "pushsum" && topology = "line" then
        time<- PushSumLine.processStart totalNodes
        printfn "%f" time

    if protocol = "pushsum" && topology = "full" then
        time<- PushSumFull.processStart totalNodes
        printfn "%f" time
    if protocol = "pushsum" && topology = "3DImperfect" then
        time<- PushSum3DImperfect.processStart totalNodes
        printfn "%f" time
       


main ()
