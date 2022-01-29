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

    CoverPoint FindCoverPoint(Vector3 startPos, Vector3 searchDir) {

        RaycastHit hit;

        Vector3 p1 = startPos;
        float distToNext = 0;

        // Building is 20 meters wide, 32 with sidewalks. 18 meters between cover points. 
        if (Physics.SphereCast(p1, 1.0f, searchDir, out hit, 20)) {
            distToNext = hit.distance;

            CoverPoint nextPoint = hit.transform.GetComponent<CoverPoint>();

            return nextPoint;
        }

        return null;
    }

    public CoverPoint FindNearLeftCover(CoverPoint startPoint) {

        // Find cover point due left.
        // If none, find cover point due back. (going around corner)

        Vector3 searchDir;

        // If startPoint is wall
        if (startPoint.coverType == CoverPoint.CoverType.WALL) {
            // Get this point's X vec.
            searchDir = -startPoint.transform.right;
        }

        // If startPoint is corner 
        else if (startPoint.coverType == CoverPoint.CoverType.CORNER) {
            // Get this point's Z vec.
            searchDir = startPoint.transform.forward;
        }

        else {
            throw new System.Exception("Cover point has no type.");
        }

        return FindCoverPoint(startPoint.transform.position, searchDir);
    }

    public CoverPoint FindNearRightCover(CoverPoint startPoint) {

        // Find cover point due right.
        // If none, find cover point due back. (going around corner)

        Vector3 searchDir;

        // If startPoint is wall
        if (startPoint.coverType == CoverPoint.CoverType.WALL) {
            // Get this point's X vec.
            searchDir = startPoint.transform.right;
        }

        // If startPoint is corner 
        else if (startPoint.coverType == CoverPoint.CoverType.CORNER) {
            // Get this point's Z vec.
            searchDir = startPoint.transform.right;
        }

        else {
            throw new System.Exception("Cover point has no type.");
        }

        return FindCoverPoint(startPoint.transform.position, searchDir);
    }
}
