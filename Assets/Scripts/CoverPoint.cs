using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverPoint : MonoBehaviour
{
    public enum CoverType {
        NONE,
        WALL,
        CORNER
    }

    public int id;
    public CoverType coverType;
    public float headingOffset;
    public bool hiddenFromStart;
}
