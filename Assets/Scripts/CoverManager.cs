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

    CoverPoint FindCoverPoint(CoverPoint startPoint, Vector3 searchDir, bool far, bool back) {

        RaycastHit hit;

        Vector3 p1 = startPoint.transform.position;
        float distToNext = 0;

        LayerMask coverPointLM = LayerMask.NameToLayer("Cover Point");

        // Close cover: Building is 20 meters wide, 32 with sidewalks. 18 meters between cover points. 
        // Far cover: Gap between buildings is 40 meters, and kaiju should be able to leap from a wall cover past adjacent corner cover to wall cover across road lanes. 60 meters

        float dist = 27; // This is far enough to find adjacent on same building, and "back" across the street.

        // Far needs to pass over close cover and search farther.
        if (far && !back) {
            p1 += searchDir * (60 - 24);
        }

        // Cast from far to near so we get the last point when moving far. When moving near there should only be one or no possible matches but when going far there will be up to two corners between.
        if (Physics.SphereCast(p1 + (searchDir * dist), 1.0f, -searchDir, out hit, dist, 1 << coverPointLM)) {
            distToNext = hit.distance;

            CoverPoint nextPoint = hit.transform.GetComponent<CoverPoint>();

            return nextPoint;
        }

        return null;
    }

    public CoverPoint FindLeftCover(CoverPoint startPoint, bool far) {

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

            if (far) {
                // Get this point's -X vec.
                searchDir = -startPoint.transform.right;
            }
            else {
                // Get this point's Z vec.
                searchDir = startPoint.transform.forward;
            }
        }

        else {
            throw new System.Exception("Cover point has no type.");
        }

        return FindCoverPoint(startPoint, searchDir, far, false);
    }

    public CoverPoint FindRightCover(CoverPoint startPoint, bool far) {

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

            if (far) {
                // Get this point's -X vec.
                searchDir = -startPoint.transform.forward;
            }
            else {
                searchDir = startPoint.transform.right;
            }
        }

        else {
            throw new System.Exception("Cover point has no type.");
        }

        return FindCoverPoint(startPoint, searchDir, far, false);
    }

    public CoverPoint FindBackCover(CoverPoint startPoint) {

        // Find cover point back from current cover.
        // Actually confusing because it's "forward" of Kaiju, but back from default camera angle.

        Vector3 searchDir;

        // If startPoint is wall
        if (startPoint.coverType == CoverPoint.CoverType.WALL) {
            // Get this point's X vec.
            searchDir = -startPoint.transform.forward;
        }

        // If startPoint is corner 
        else if (startPoint.coverType == CoverPoint.CoverType.CORNER) {

            // Just no. You can't switch to far back cover from a corner. Corners are only for getting around buildings clockwise or counter clockwise.
            return null;
        }

        else {
            throw new System.Exception("Cover point has no type.");
        }

        return FindCoverPoint(startPoint, searchDir, true, true);
    }
}
