#!/bin/bash

vtx=50
for i in  {1..20}; do
    time=$(dotnet fsi Master.fsx $vtx pushsum line)
    echo "$vtx,$time"
    echo "$vtx,$time" >> pushsumLine.csv
    vtx=$((vtx + 50))
done