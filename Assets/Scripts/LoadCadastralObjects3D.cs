using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Diagnostics;

public partial class LoadCadastralObjects : MonoBehaviour {
    private void load3DParcels(int roundX, int roundY)
    {
        string parcels3DData = importFromDB("3D " + roundX + " " + roundY);
        //UnityEngine.Debug.Log("3D " + roundX + " " + roundY);
        ArrayList parcel3DData = new ArrayList();
        string[] parcels3DDataSplit = parcels3DData.Split('n');
        long suid = long.Parse(parcels3DDataSplit[0].Split(';')[0]);
        foreach (string data in parcels3DDataSplit)
        {
            try
            {
                //UnityEngine.Debug.Log("Parcel data row: " + data);
                string[] pointData = data.Split(';');
                if (suid != long.Parse(pointData[0]))
                {
                    //UnityEngine.Debug.Log("Adding parcel: " + parseParcel3DData(parcel3DData).parCis + " " + parseParcel3DData(parcel3DData).suid);
                    parcels3DToBeAdded.Add(parseParcel3DData(parcel3DData));
                    parcel3DData = new ArrayList();
                    suid = long.Parse(pointData[0]);
                }
                parcel3DData.Add(pointData);
            } catch (Exception) { }
        }
        //UnityEngine.Debug.Log("Adding parcel: " + parseParcel3DData(parcel3DData).parCis + " " + parseParcel3DData(parcel3DData).suid);
        parcels3DToBeAdded.Add(parseParcel3DData(parcel3DData));
        //addParcel3DLines(parcel3D);
    }

    /*private void addParcel3DBoundaryFaces(Parcel3D parcel3D)
    {
        foreach (Parcel3DBoundaryFace boundaryFace in parcel3D.boundaryFaces)
        {

        }

        ArrayList surfacePoints3D = new ArrayList();
        long bfid = -1;
        foreach (ParcelPoint3D parcelPoint3D in parcelPoints3D)
        {
            if(parcelPoint3D.bfid != bfid && bfid != -1)
            {
                SurfacesToBeAdded.Add(vector3(surfacePoints3D));
                surfacePoints3D = new ArrayList();
            }
            surfacePoints3D.Add(parcelPoint3D);
            bfid = parcelPoint3D.bfid;
        }
        SurfacesToBeAdded.Add(vector3(surfacePoints3D));
    }

    private void addParcel3DLines(Parcel3D parcel3D)
    {
        ParcelPoint3D point1 = (ParcelPoint3D)parcelPoints3D[0];
        foreach (ParcelPoint3D point2 in parcelPoints3D)
        {
            if(point2.seq != 1) //predpokladam, ze jdou za sebou
            {
                add3DLine(point1, point2);
            }
            point1 = point2;
        }
    }*/

    private Parcel3D parseParcel3DData(ArrayList parcel3DData)
    {
        //UnityEngine.Debug.Log("parseParcel3DData");
        Parcel3D parcel = new Parcel3D();
        parcel.boundaryFaces = new ArrayList();
        Parcel3DBoundaryFace bf = new Parcel3DBoundaryFace();
        bf.parrentParcel = parcel;
        bf.points = new ArrayList();
        bf.bfid = -1;
        //UnityEngine.Debug.Log("parse2DParcelData... Length: " + parcel3DData.Count);
        //UnityEngine.Debug.Log(((String[])parcel3DData[0]).Length);
        foreach (string[] pointData in parcel3DData)
        {
            try
            {
                if (bf.bfid == -1) bf.bfid = long.Parse(pointData[2]);
                if (bf.bfid != long.Parse(pointData[2]))
                {
                    parcel.boundaryFaces.Add(bf);
                    bf = new Parcel3DBoundaryFace();
                    bf.parrentParcel = parcel;
                    bf.points = new ArrayList();
                    bf.bfid = long.Parse(pointData[2]);
                }
                parcel.suid = long.Parse(pointData[0]);
                parcel.parCis = pointData[1];
                bf.orientation = char.Parse(pointData[3]);
                Parcel3DPoint point = new Parcel3DPoint();
                point.y = float.Parse(pointData[6]);
                point.x = float.Parse(pointData[5]);
                point.h = float.Parse(pointData[7]);
                bf.points.Add(point);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
        }
        parcel.boundaryFaces.Add(bf);
        //UnityEngine.Debug.Log("parcela 3D... " + parcel.parCis);
        //UnityEngine.Debug.Log("  čílslo parcely: " + parcel.parCis);
        //UnityEngine.Debug.Log("  počet bf: " + parcel.boundaryFaces.Count);
        return parcel;
    }

    private void add3DLine(Parcel3DPoint point1, Parcel3DPoint point2)
    {
        //UnityEngine.Debug.Log("create2DLine");
        LineToBeAdded lineToBeAdded = new LineToBeAdded();
        //UnityEngine.Debug.Log("3D Line to be added: " + point1.y + " " + point1.x + " " + point2.y + " " + point2.x);
        lineToBeAdded.v1 = vector3(point1);
        lineToBeAdded.v2 = vector3(point2);
        //lineToBeAdded.c = Color.blue; //prepsano materialem
        linesToBeAdded.Add(lineToBeAdded);
    }

    private void createBoundaryFace(Parcel3DBoundaryFace boundaryFace)
    {
        //UnityEngine.Debug.Log("createSurface");
        GameObject mySurface = new GameObject("BoundaryFace suid: "+boundaryFace.parrentParcel.suid);
        MeshCollider mc = mySurface.AddComponent<MeshCollider>();
        mySurface.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = mySurface.AddComponent<MeshRenderer>();
        if(boundaryFace.parrentParcel.suid == 1) meshRenderer.material = (Material)Resources.Load("Parcel3DSelectedMaterial");
        else meshRenderer.material = (Material)Resources.Load("Parcel3DMaterial");
        Mesh mesh = mySurface.GetComponent<MeshFilter>().mesh;
        boundaryFace.meshRenderer = meshRenderer;

        ArrayList boundaryFacePolygonPointsVector = vector3(boundaryFace);
        int n = boundaryFacePolygonPointsVector.Count - 1; //prvni bod je tam dvakrat
        var vertices = new Vector3[n + 2]; //pridavam stredovy bod a dvojnasobny prvni bod
        var triangles = new int[n * 3]; //počet bodů polygonu * 3

        Vector3 pM = calculateAvePoint(boundaryFacePolygonPointsVector);
        vertices[0] = pM;
        for (int i = 0; i < n; i++)
        {
            vertices[i + 1] = (Vector3)boundaryFacePolygonPointsVector[i];
            triangles[3 * i] = 0;
            if (boundaryFace.orientation == '-')
            {
                triangles[3 * i + 1] = i + 1;
                triangles[3 * i + 2] = i + 2;
            }
            else if (boundaryFace.orientation == '+')
            {
                triangles[3 * i + 1] = i + 2;
                triangles[3 * i + 2] = i + 1;
            }
            else throw new Exception("Wrong orientation sign");
        }
        vertices[n + 1] = (Vector3)boundaryFacePolygonPointsVector[n];
        string tri = "";
        foreach (int t in triangles) tri += t + ", ";
        //UnityEngine.Debug.Log(tri);
        //foreach (Vector3 v in vertices) UnityEngine.Debug.Log(v.x + " " + v.y + " " + v.z);
        //UnityEngine.Debug.Log("");
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private Vector3 calculateAvePoint(ArrayList boundaryFacePolygonPointsVector)
    {
        var pAve = new Vector3(0, 0, 0);
        var n = boundaryFacePolygonPointsVector.Count;
        for(int i = 0; i < n; i++)
        {
            Vector3 pN = (Vector3)boundaryFacePolygonPointsVector[i];
            pAve.x += pN.x;
            pAve.y += pN.y;
            pAve.z += pN.z;
        }
        pAve.x /= n;
        pAve.y /= n;
        pAve.z /= n;
        return pAve;
    }

    private Vector3 vector3(Parcel3DPoint p)
    {
        return new Vector3(p.y - dy, p.h, p.x - dx);
    }

    private ArrayList vector3(Parcel3DBoundaryFace boundaryFace)
    {
        ArrayList boundaryFacePolygonPoints = boundaryFace.points;
        ArrayList boundaryFacePolygonPointsVector = new ArrayList();
        foreach (Parcel3DPoint p in boundaryFacePolygonPoints)
        {
            boundaryFacePolygonPointsVector.Add(vector3(p));
        }
        return boundaryFacePolygonPointsVector;
    }

    public class Parcel3D
    {
        public string parCis { get; set; }
        public long suid { get; set; }
        public ArrayList boundaryFaces { get; set; }
    }

    public struct Parcel3DBoundaryFace
    {
        public Parcel3D parrentParcel { get; set; }
        public long bfid { get; set; }
        public char orientation { get; set; }
        public ArrayList points { get; set; }
        public MeshRenderer meshRenderer { get; set; }
    }

    private struct Parcel3DPoint
    {
        public float y { get; set; }
        public float x { get; set; }
        public float h { get; set; }
    }
}
