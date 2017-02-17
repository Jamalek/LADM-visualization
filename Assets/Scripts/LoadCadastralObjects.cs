using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Diagnostics;

public partial class LoadCadastralObjects : MonoBehaviour {
    float dy = -820770.74f;
    float dx = -1073222.38f;
    ArrayList loadedTiles = new ArrayList();
    int lastTileX = 0;
    int lastTileY = 0;

    // Use this for initialization
    void Start()
    {
        Camera.main.gameObject.transform.position = new Vector3(0, 5, -20);
        //DrawLine(new Vector3(0, 0, 0), new Vector3(200, 0, 200), Color.black);
    }

    ArrayList linesToBeAdded = new ArrayList();
    ArrayList parcels3DToBeAdded = new ArrayList();

    // Update is called once per frame
    void Update()
    {
        loadAdjacentTiles();
        if (linesToBeAdded.Count != 0) addLines();
        if (parcels3DToBeAdded.Count != 0) addParcels3D();
    }

    private void loadAdjacentTiles()
    {
        Vector3 position = Camera.main.gameObject.transform.position;
        int roundY = (int)Math.Round((position.x + dy) / 200) * 200;
        int roundX = (int)Math.Round((position.z + dx) / 200) * 200;
        //UnityEngine.Debug.Log(roundY + " " + roundX);
        if (lastTileX != roundX || lastTileY != roundY)
        {
            lastTileX = roundX;
            lastTileY = roundY;
            int ts = 200; //tile size
            Tile tile = new Tile(roundX, roundY);
            if (!tileLoaded(tile)) LoadData(tile);
            tile = new Tile(roundX + ts, roundY);
            if (!tileLoaded(tile)) LoadData(tile);
            tile = new Tile(roundX + ts, roundY + ts);
            if (!tileLoaded(tile)) LoadData(tile);
            tile = new Tile(roundX,      roundY + ts);
            if (!tileLoaded(tile)) LoadData(tile);
            tile = new Tile(roundX - ts, roundY + ts);
            if (!tileLoaded(tile)) LoadData(tile);
            tile = new Tile(roundX - ts, roundY);
            if (!tileLoaded(tile)) LoadData(tile);
            tile = new Tile(roundX - ts, roundY - ts);
            if (!tileLoaded(tile)) LoadData(tile);
            tile = new Tile(roundX,      roundY - ts);
            if (!tileLoaded(tile)) LoadData(tile);
            tile = new Tile(roundX + ts, roundY - ts);
            if (!tileLoaded(tile)) LoadData(tile);
        }
    }

    private bool tileLoaded(Tile tile)
    {
        bool test = loadedTiles.Contains(tile);
        /*UnityEngine.Debug.Log(tile.roundY + " " + tile.roundX);
        UnityEngine.Debug.Log(test);
        UnityEngine.Debug.Log("");*/
        return test;
    }

    ArrayList parcel3DAddedList = new ArrayList(); //long

    private void addParcels3D()
    {
        while (parcels3DToBeAdded.Count != 0)
        {
            Parcel3D parcel3D = (Parcel3D)parcels3DToBeAdded[0];
            //UnityEngine.Debug.Log(parcel3D.parCis+" se načítá");
            if (!parcel3DAddedList.Contains(parcel3D.suid)) {
                UnityEngine.Debug.Log(parcel3D.parCis + " se načte");
                //foreach (Parcel3DBoundaryFace bf in parcel3D.boundaryFaces) createBoundaryFace(bf);
            }
            parcels3DToBeAdded.Remove(parcel3D);
        }
    }

    private void addLines()
    {
        try
        {
            while (linesToBeAdded.Count != 0)
            {
                LineToBeAdded line = (LineToBeAdded)linesToBeAdded[0];
                DrawLine(line.v1, line.v2, line.c);
                linesToBeAdded.Remove(line);
            }
        } catch
        {
            linesToBeAdded.Remove(linesToBeAdded[0]);
        }
    }

    void LoadData(Tile tile)
    {
        try
        {
            //UnityEngine.Debug.Log("Loading tile: " + roundY + " " + roundX);
            loadedTiles.Add(tile);
            Thread thread2D = new Thread(new ThreadStart(() => load2DParcels(tile.roundY, tile.roundX)));
            Thread thread3D = new Thread(new ThreadStart(() => load3DParcels(tile.roundY, tile.roundX)));
            thread2D.Start();
            thread3D.Start();
        }
        catch (Exception e)
        {
            print(e);
        }
    }
    
    private string importFromDB(string args)
    {
        /*try
        {
            UnityEngine.Debug.Log(args);
            Process myProcess = new Process();
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.FileName = "C:\\Google Drive\\Programy\\VSprojects\\ParcelLoader\\ParcelLoader\\bin\\Debug\\ParcelLoader.exe";
            myProcess.StartInfo.Arguments = args;
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
            myProcess.WaitForExit(5000);
            int ExitCode = myProcess.ExitCode;
            string output = myProcess.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log(output);
            return output;
            //print(ExitCode);
        }
        catch (Exception e)
        {
            print(e);
        }
        UnityEngine.Debug.Log("Error"   );
        return "";*/
        var processInfo = new ProcessStartInfo("java.exe", "-jar C:\\Users\\Jan\\Desktop\\DBLoader.jar " + args)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true
        };
        Process proc;
        if ((proc = Process.Start(processInfo)) == null)
        {
            UnityEngine.Debug.Log("Nepodařilo se načíst "+(args.Split(' ')[0])+" parcely");
        }
        return proc.StandardOutput.ReadToEnd();
    }

    void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        //UnityEngine.Debug.Log("DrawLine "+start.x+" "+start.z+" "+end.x + " " + end.z);
        GameObject myLine = new GameObject("BoundaryLine");
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = (Material)Resources.Load("ParcelLineMaterial");
        lr.startColor = color;
        lr.startWidth = 0.3f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private struct LineToBeAdded
    {
        public Vector3 v1 { get; set; }
        public Vector3 v2 { get; set; }
        public Color c { get; set; }
    }

    private struct Tile
    {
        public int roundX;
        public int roundY;

        public Tile(int roundX, int roundY) : this()
        {
            this.roundX = roundX;
            this.roundY = roundY;
        }
    }
}
