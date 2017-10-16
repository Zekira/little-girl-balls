using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Attach to the parent of a bunch of transforms with Menu attached
public class ScrollableMenuParent : MonoBehaviour {

    //To be set in the inspector
    public int displayCount; //How many of the children to show
    public int firstDisplayIndex; //The index of the first child to show
}
