using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Diagnostics;

public partial class LoadCadastralObjects : MonoBehaviour {
    private void load2DParcels(int roundX, int roundY)
    {
        string parcelData2D = importFromDB("2D "+roundX+" "+roundY);
        add2DParcelGraphics(parcelData2D);
        //UnityEngine.Debug.Log("Parcel data: "+parcelData2D);
    }

    private void add2DParcelGraphics(string parcelData2D)
    {
        //UnityEngine.Debug.Log("add2DParcelGraphics");
        ArrayList parcelPoints2D = parse2DParcelData(parcelData2D);
        ParcelPoint2D point1 = (ParcelPoint2D)parcelPoints2D[0];
        foreach (ParcelPoint2D point2 in parcelPoints2D)
        {
            if(point2.seq != 1) //predpokladam, ze jdou za sebou
            {
                //UnityEngine.Debug.Log(point1.y + " " + point1.x+" "+ point2.y + " " + point2.x);
                add2DLine(point1, point2);
            }
            point1 = point2;
        }
    }

    private ArrayList parse2DParcelData(string parcelData2D)
    {
        //UnityEngine.Debug.Log("parse2DParcelData");
        ArrayList parcelPoints2D = new ArrayList();
        string[] data = parcelData2D.Split('n');
        //UnityEngine.Debug.Log("parse2DParcelData... Points: " + data.Length);
        foreach (string pointDataString in data)
        {
            try
            {
                //UnityEngine.Debug.Log("parse2DParcelData... Point: " + pointDataString);
                string[] pointData = pointDataString.Split(';');
                ParcelPoint2D point = new ParcelPoint2D();
                point.bfsid = long.Parse(pointData[0]);
                point.seq = int.Parse(pointData[1]);
                point.x = float.Parse(pointData[2]);
                point.y = float.Parse(pointData[3]);
                parcelPoints2D.Add(point);
            }
            catch (Exception e)
            {
                //UnityEngine.Debug.Log(e.Message);
            }
        }
        return parcelPoints2D;
    }

    private void add2DLine(ParcelPoint2D point1, ParcelPoint2D point2)
    {
        //UnityEngine.Debug.Log("create2DLine");
        LineToBeAdded lineToBeAdded = new LineToBeAdded();
        //UnityEngine.Debug.Log(point1.y + " " + point1.x + " " + point2.y + " " + point2.x);
        lineToBeAdded.v1 = new Vector3(point1.y - dy, 0, point1.x - dx);
        lineToBeAdded.v2 = new Vector3(point2.y - dy, 0, point2.x - dx);
        lineToBeAdded.c = Color.black;
        linesToBeAdded.Add(lineToBeAdded);
    }

    private struct ParcelPoint2D
    {
        public long bfsid { get; set; }
        public int seq { get; set; }
        public float y { get; set; }
        public float x { get; set; }
    }
}
