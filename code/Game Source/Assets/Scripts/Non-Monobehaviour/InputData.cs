using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For use in ReplayData
/// </summary>
public struct InputData {

    public int startingTick;
    public int duration;
    public bool[] keys;
	
    /// <summary>
    /// This function already assumes the input keys have been put through KeyData.InputToByte/KeyData.CurrentKeysDownToByte.
    /// </summary>
    public InputData(int startingTick, int duration, bool[] keys) {
        this.startingTick = startingTick;
        this.duration = duration;
        this.keys = keys;
    }
}
