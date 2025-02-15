﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]

[ExecuteInEditMode]

public class ElectricShock_LightningLineDemo : MonoBehaviour
{

    public float detail = 1;

    public float displacement = 15;

    public Transform EndPostion;
    public Vector3 anchorOffset;

    public Transform StartPosition;

    public float yOffset = 0;

    private LineRenderer _lineRender;

    private List<Vector3> _linePosList;

    private void Awake()

    {
        _lineRender = GetComponent<LineRenderer>();

        _linePosList = new List<Vector3>();

    }

    private void Update()

    {

        

        if (Time.timeScale != 0)

        {

            _linePosList.Clear();

            Vector3 startPos = Vector3.zero;

            Vector3 endPos = Vector3.zero;

            if (EndPostion != null)

            {

                endPos = EndPostion.position + anchorOffset + Vector3.up * yOffset;

            }

            if (StartPosition != null)

            {

                startPos = StartPosition.position + Vector3.up * yOffset;

            }

           

            CollectLinPos(startPos, endPos, displacement);

            _linePosList.Add(endPos);

            

            _lineRender.positionCount=_linePosList.Count;

            for (int i = 0, n = _linePosList.Count; i < n; i++)

            {

                _lineRender.SetPosition(i, _linePosList[i]);

            }

        }

    }

   

    private void CollectLinPos(Vector3 startPos, Vector3 destPos, float displace)

    {

       

        if (displace < detail)

        {

            _linePosList.Add(startPos);

        }

        else

        {

            float midX = (startPos.x + destPos.x) / 2;

            float midY = (startPos.y + destPos.y) / 2;

            float midZ = (startPos.z + destPos.z) / 2;

            midX += (float)(UnityEngine.Random.value - 0.5) * displace;

            midY += (float)(UnityEngine.Random.value - 0.5) * displace;

            midZ += (float)(UnityEngine.Random.value - 0.5) * displace;

            Vector3 midPos = new Vector3(midX, midY, midZ);

            CollectLinPos(startPos, midPos, displace / 2);

            CollectLinPos(midPos, destPos, displace / 2);

        }

    }

}
