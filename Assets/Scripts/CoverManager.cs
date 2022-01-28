using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Collect and manage all cover points in the world. Provide access and search capabilities to cover users.

CoverManager is set to run early so that Kaiju can pull a random cover point in its Start. 
*/

public class CoverManager : MonoBehaviour
{
    CoverPoint[] coverPoints = null;
    
    void Start()
    {
     
    }

    void Update()
    {
        
    }

    void CollectCoverPoints() {

        coverPoints = FindObjectsOfType<CoverPoint>();

        // Iterate these and assign IDs.
        for (int i = 0; i < coverPoints.Length; i++) {

            coverPoints[i].id = i;
        }

        // TODO: Seed properly after development.
        Random.InitState(42);
    }

    public CoverPoint GetRandomCoverPoint() {

        if (coverPoints == null) {
            CollectCoverPoints();
        }

        int id = -1;

        // Find random wall cover, skip corners.
        while (true) {
            id = (int)Random.Range(0, coverPoints.Length - 1);
            if (coverPoints[id].coverType == CoverPoint.CoverType.WALL)
                break;
        }

        return coverPoints[id];
    }

    public CoverPoint FindNearLeftCover(CoverPoint startPoint) {

        // Find cover point due left.
        // If none, find cover point due back. (going around corner)

        // Some cheap hack just to get movement tests in quick. To a geometric search instead.

        // Cover points are wrapped around each building in clockwise order while facing the building. + index should be clockwise
        // IF the FindObjectsOfType iterates the hierarchy in the obvious manner.

        int id = (startPoint.id + 1) % (coverPoints.Length);

        return coverPoints[id];
    }

    public CoverPoint FindNearRightCover(CoverPoint startPoint) {

        int id = (startPoint.id - 1) % (coverPoints.Length);

        return coverPoints[id];
    }
}
