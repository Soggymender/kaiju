using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverObject : MonoBehaviour
{
    public enum CoverType {
        NONE,
        WALL,
        CORNER
    }

    [System.Serializable]
    public class CoverPoint {
        public CoverType coverType;
        public Transform transform;
    }

    public CoverPoint[] coverPoints;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
