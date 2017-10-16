using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For use in ReplayManager
/// </summary>
public class ReplayData {
    //Can always assume this is in order because new things get added to it after older things. That's how time works.
    private List<InputData> inputData = new List<InputData>();

    public void AddInputData(InputData data) {
        inputData.Add(data);
    }

    /// <summary>
    /// Returns a byte containing key data described by KeyData.InputToByte
    /// </summary>
    public byte GetInputAtTime(int time) {
        byte returnBool = 0;
        for (int i = 0; (i < inputData.Count) && (inputData[i].startingTick <= time); i++) {
            if (inputData[i].startingTick + inputData[i].duration >= time) { //This input "collides" with that point in time
                returnBool = KeyData.Combine(returnBool, inputData[i].keys);
            }
        }
        return returnBool;
    }
}
