using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOL : MonoBehaviour {
    public int resX = 100;
    public int resY = 50;
    public GameObject[,] golGOs;
    public bool[,] grid;
    public bool[,] gridNew;
    public bool[,] gridOld;
    public bool ynShowNeighbors;
    [Header(" ")]
    public bool ynShowNew;
    public bool ynShowAlone;
    public bool ynShowCrowded;
    [Header(" ")]
    public bool ynGlide;
    public bool yn3D;
    [Header(" ")]
    public float elapsedTime;
    public int total;
    public float density;
    public bool ynStep;
    public bool ynRestart;
    GameObject parent;
    bool ynGlideLast;
    float tStepStart;
    int maxCrowdX;
    int maxCrowdY;
    int maxCrowd;
    [Range(1, 50)]
    public float cameraDist = 5;

    void Update () {
        if (ynStep == true)
        {
            if (Time.realtimeSinceStartup - tStepStart > 1) {
                tStepStart = Time.realtimeSinceStartup;
                regenGrid();
            }
        } else {
            regenGrid();
        }
        followCrowded();
        checkRestart();
	}

    void followCrowded() {
        Vector3 pos = new Vector3(resX / 2, 0, resY / 2);
        if (ynGlide == true) {
            pos = new Vector3(maxCrowdX, 0, maxCrowdY);
        }
        Camera.main.transform.position = new Vector3(pos.x + cameraDist, cameraDist * 2, pos.y);
        Camera.main.transform.LookAt(pos);
    }

    void checkRestart() {
        if (ynGlide != ynGlideLast)
        {
            ynRestart = true;
        }
        ynGlideLast = ynGlide;
    }

    void regenGrid() {
        float t = Time.realtimeSinceStartup;
        if (golGOs == null) {
            golGOs = new GameObject[resX, resY];
            initGrid();
        } else {
            if (ynRestart == true)
                {
                ynRestart = false;
                initGrid();
                System.Array.Clear(gridNew, 0, gridNew.Length);
            }
            int cnt = 0;
            for (int x = 0; x < resX; x++)
            {
                for (int y = 0; y < resY; y++)
                {
                    bool ynAlive = getGolState(x, y);
                    if (ynAlive == true) {
                        cnt++;                        
                    }
                    gridNew[x, y] = ynAlive;
                }
            }
            total = resY * resY;
            density = cnt / (float)total;
            System.Array.Copy(grid, gridOld, gridOld.Length);
            System.Array.Copy(gridNew, grid, grid.Length);
            System.Array.Clear(gridNew,0, gridNew.Length);
        }
        drawGrid();
        elapsedTime = Time.realtimeSinceStartup - t;
    }

    void drawGrid()
    {
        for (int x = 0; x < resX; x++)
        {
            for (int y = 0; y < resY; y++)
            {
                setGo(x, y);
                setGolColor(x, y);
                setGolScale(x, y);
            }
        }
    }

    void setGolScale(int x, int y) {
        GameObject go = golGOs[x, y];
        float sx = .9f;
        float sy = .9f;
        float sz = .9f;
        Vector3 pos = go.transform.position;
        pos.y = 0;
        if (ynShowNeighbors == true)
        {
            sy *= getNeighborCount(x, y);
            pos.y = sy / 2;
        }
        go.transform.localScale = new Vector3(sx, sy, sz);
        go.transform.position = pos;
    }

    void setGolColor(int x, int y)
    {
        Color col = Color.black;
        bool ynAlive = grid[x, y];
        if (ynAlive == true)
        {
            col = Color.black;
                if (ynShowNew == true)
            {
                if (gridOld[x, y] == false)
                {
                    col = Color.magenta;
                }
            }
            if (ynShowAlone == true)
            {
                if (getNeighborCount(x, y) == 1)
                {
                    col = Color.cyan;
                }
            }
            if (ynShowCrowded == true)
            {
                if (getNeighborCount(x, y) > 3)
                {
                    col = Color.red;
                }
            }
        } else {
            col = Color.white;
        }
        GameObject go = golGOs[x, y];
        setColor(go, col);
    }

    void setGo(int x, int y)
    {
        GameObject go = golGOs[x, y];
        if (go == null)
        {
            if (parent == null)
            {
                parent = new GameObject("parent");
                parent.transform.parent = transform;
            }
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.parent = parent.transform;
            go.name = "lod " + x + " " + y;
            Vector3 pos = new Vector3(x, 0, y);
            go.transform.position = pos;
            golGOs[x, y] = go;
        }
    }

    void initGrid() {
        grid = new bool[resX, resY];
        gridNew = new bool[resX, resY];
        gridOld = new bool[resX, resY];
        if (ynGlide == true)
        {
            initGridGlider();
        }
        else
        {
            initGridRandom();
        }
    }

    void initGridRandom() {
        for (int x = 0; x < resX; x++)
        {
            for (int y = 0; y < resY; y++)
            {
                setGrid(x, y, getRandomBool());
            }
        }
    }

    void initGridGlider() {
        System.Array.Clear(grid, 0, grid.Length);
        setGrid(1, 0, true);
        setGrid(2, 1, true);
        setGrid(0, 2, true);
        setGrid(1, 2, true);
        setGrid(2, 2, true);
    }

    void setGrid(int x, int y, bool yn) {
        grid[x, y] = yn;
   }

    bool getRandomBool() {
        return (Random.value > 0.5f);
    }

    bool getGolState(int x, int y) {
        int cnt = getNeighborCount(x, y);
        bool ynAlive = grid[x, y];
        if (ynAlive == true) {
            if (cnt < 2 || cnt > 3) {
                ynAlive = false;                
            }            
        } else {
            if (cnt == 3) {
                ynAlive = true;                
            }
        }
        if (ynGlide == true) {
            if (x == 0 && y == 0)
            {
                maxCrowd = -1;
            }
            if (cnt > 3) {
                if (maxCrowd == -1 || cnt > maxCrowd)
                {
                    maxCrowdX = x;
                    maxCrowdY = y;
                    maxCrowd = cnt;
                }
            }
        }
        return ynAlive;
    }

    int getNeighborCount(int x, int y) {
        int cnt = 0;
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++)
            {
                int xi = (x + i + resX) % resX;
                int yj = (y + j + resY) % resY;
                if (grid[xi, yj] == true) {
                    cnt++;
                }
            }
        }
        if (grid[x, y] == true) {
            cnt--;
        }
        return cnt;
    }

    void setColor(GameObject go, Color col) {
        Renderer rend = go.GetComponent<Renderer>();
        rend.material.color = col;
    }
}
