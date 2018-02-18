using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOL3D : MonoBehaviour {
    public int resX = 12;
    public int resY = 12;
    public int resZ = 12;
    public GameObject[,,] gol3DGOs;
    GameObject parent;
    [Range(0, 27)]
    public int numAlone = 5; 
    [Range(0, 27)]
    public int numCrowded = 7;
    [Range(0, 27)]
    public int numNew = 2;
    public float elapsedTime;
    public int total;
    public float density;
    public bool[,,] grid3D;
    public bool[,,] grid3Dnew;
    public bool[,,] grid3Dold;
    public bool ynReset;
    float startStepTime;
    public bool ynStep;

	// Use this for initialization
	void Start () {
        total = resX * resY * resZ;
//        ynReset = true;
//        UpdateX();
//        UpdateX();
	}
	
	// Update is called once per frame
	void Update () {
        if (ynStep == true) {
            if (Time.realtimeSinceStartup - startStepTime > 1)
            {
                startStepTime = Time.realtimeSinceStartup;
                updateGrid3D();
            }
        } else {
            updateGrid3D();
        }
	}

    void updateGrid3D() {
        Debug.Log("updateGrid3D");
        float t = Time.realtimeSinceStartup;
        regenGrid3Dnew();
        drawGrid3D();
        elapsedTime = Time.realtimeSinceStartup - t;
    }

    void initGrid3D() {
        Debug.Log("initGrid");
        initGrid3Drandom();
    }

    void initGrid3Drandom() {
        for (int x = 0; x < resX; x++)
        {
            for (int y = 0; y < resY; y++)
            {
                for (int z = 0; z < resZ; z++)
                {
                    grid3D[x, y, z] = getRandomBool();
                }
            }
        }
    }

    void regenGrid3Dnew() {
        if (grid3D == null) {
            grid3D = new bool[resX, resY, resZ];
            grid3Dnew = new bool[resX, resY, resZ];
            grid3Dold = new bool[resX, resY, resZ];
            initGrid3D();
            Debug.Log("1");
            return;
        }
        if (ynReset == true) {
            ynReset = false;
            initGrid3D();
        }
        int cntAlive = 0;
        for (int x = 0; x < resX; x++)
        {
            for (int y = 0; y < resY; y++)
            {
                for (int z = 0; z < resZ; z++)
                {
                    int cnt = getNeighbor3DCount(x, y, z);
                    bool ynAlive = grid3D[x, y, z];
                    if (ynAlive == true) {
                        if (cnt < numAlone || cnt > numCrowded) {
                            ynAlive = false;
                        }
                    } else {
                        if (cnt == numNew)
                        {
                            ynAlive = true;
                        }
                    }
                    if (ynAlive == true) {
                        cntAlive++;
                    }
                    grid3Dnew[x, y, z] = ynAlive;
                }
            }
        }
        density = cntAlive / (float)(resX * resY * resZ);
        System.Array.Copy(grid3Dnew, grid3D, grid3D.Length);
    }

    int getNeighbor3DCount(int x, int y , int z) {
        int cnt = 0;
        for (int i = -1; i <= 1; i++) 
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    int ii = x + i;
                    int jj = y + j;
                    int kk = z + k;
                    ii = (ii + resX) % resX;
                    jj = (jj + resY) % resY;
                    kk = (kk + resZ) % resZ;
                    if (grid3D[ii, jj, kk] == true) {
                        cnt++;                        
                    }
                }
            }
            if (grid3D[x,y,z] == true) {
                cnt--;
            }
        }
        return cnt;
    }

    void drawGrid3D() {
        if (gol3DGOs == null) {
            parent = new GameObject("parent");
            parent.transform.parent = transform;
            gol3DGOs = new GameObject[resX, resY, resZ];
        }
        for (int x = 0; x < resX; x++) 
        {
            for (int y = 0; y < resY; y++)
            {
                for (int z = 0; z < resZ; z++)
                {
                    GameObject go = gol3DGOs[x, y, z];
                    if (go == null) {
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.transform.parent = parent.transform;
                        go.name = "lod3D " + x + " " + y + " " + z;
                        go.transform.position = new Vector3(x, y, z);
                        float s = .9f;
                        go.transform.localScale = new Vector3(s, s, s);
                        gol3DGOs[x, y, z] = go; 
                    }
                    if (grid3D[x, y, z] == true)
                    {
                        setColor(go, new Color(1, 0, 0, 1));
                    }
                    else
                    {
                        setColor(go, new Color(.5f, .5f, .5f, 0.125f));
                    }
                }
            }
        }
    }

    void setColor(GameObject go, Color col) {
        Renderer rend = go.GetComponent<Renderer>();
        rend.material.color = col;
        rend.material.shader = Shader.Find("Transparent/Diffuse");
    }

    bool getRandomBool() {
        return (Random.Range(0, 2) > .5f);
    }
}
