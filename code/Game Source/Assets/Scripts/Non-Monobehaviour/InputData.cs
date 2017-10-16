using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For use in ReplayData
/// </summary>
public struct InputData {

    public int startingTick; //ints are fine. nobody's gonna make a replay that's longer than 9001 hours.
    public int duration;
    public byte keys;
	
    /// <summary>
    /// This function already assumes the input keys have been put through KeyData.InputToByte/KeyData.CurrentKeysDownToByte.
    /// </summary>
    public InputData(int startingTick, int duration, byte keys) {
        this.startingTick = startingTick;
        this.duration = duration;
        this.keys = keys;
    }
}
