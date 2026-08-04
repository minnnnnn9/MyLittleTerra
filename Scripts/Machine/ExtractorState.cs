using MLT.Data.ItemSO;
using MLT.Machine;
using System.Collections.Generic;

public class ExtractorState
{
    public Queue<ItemCropData> InputQueue = new();
    public Queue<SeedOutput> OutputQueue = new();
    public bool IsProcessing;
    public float LastCycleMinute;
    public float Progress;
}