using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    FloorMaster[] floors;
    public PlayerScript player;
    public int floorCount = 6;
    public float unitSize = 1;
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject cornerPrefab;
    public GameObject uPrefab;
    public GameObject oPrefab;
    public StairsController stairsDownPrefab;
    public StairsController stairsUpPrefab;

    public int worldSize = 20;
    public int birthLimit = 4;
    public int deathLimit = 3;
    public int simulationSteps = 10;
    public float mapWallChance = .6f;
    public float weldDistance = .01f;
    public float weldBucket = 1;
    public Vector3 bumpDistance = Vector3.one;
    public LayerMask wallLayer;
    public SpawnInfo[] spawnInfo;
    int floorIndex = 0;

    public AudioSource audio;

    void OnEnable()
    {
        Generate();
        CameraFade.StartAlphaFade(Color.black, true, 4);
        floorIndex = 0;
        floors[floorIndex].gameObject.SetActive(true);
        player.transform.position = floors[floorIndex].stairsUp.transform.position;
    }
    bool fading = false;
    public void EnterFloor(int index)
    {
        if (fading)
            return;
        fading = true;
        CameraFade.StartAlphaFade(Color.black, false, .5f, 0, ()=>
        {
            floors[floorIndex].gameObject.SetActive(false);
            bool down = floorIndex < index;
            floorIndex = index;
            floors[floorIndex].gameObject.SetActive(true);
            if (down)
            {
                player.transform.position = floors[floorIndex].stairsUp.transform.position;
                player.transform.rotation = floors[floorIndex].stairsUp.transform.rotation;
            }else
            {
                player.transform.position = floors[floorIndex].stairsDown.transform.position;
                player.transform.rotation = floors[floorIndex].stairsDown.transform.rotation;
            }
            fading = false;
            Invoke("FadeIn", 0f);
        });
    }

    void FadeIn()
    {

        CameraFade.StartAlphaFade(Color.black, true, 1f);
    }

    public void NextFloor()
    {
        Debug.Log("NEXT FLOOR");
        if (floorIndex == floorCount - 1)
            return;
        EnterFloor(floorIndex + 1);
    }

    public void PrevFloor()
    {
        Debug.Log("PREV FLOOR");
        if (floorIndex == 0)
            return;
        EnterFloor(floorIndex - 1);
    }

    public void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.L))
            player.transform.position = floors[floorIndex].stairsDown.transform.position;
        if (Input.GetKeyDown(KeyCode.K))
            player.transform.position = floors[floorIndex].stairsUp.transform.position;

        audio.pitch = Mathf.Lerp(audio.pitch, 1 - floorIndex * .1f, .05f);
    }

    [ContextMenu("Generate~")]
    public void Generate()
    {
        for (int i = transform.childCount-1; i>=0 ; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        floors = new FloorMaster[floorCount];
        for (int i = 0; i < floorCount; i++)
        {
            int[,] worldMap = GenerateMap();

            GameObject g = new GameObject();
            FloorMaster floor = g.AddComponent<FloorMaster>();
            floor.gemHolder = new GameObject("Gems").transform;
            floor.wallHolder = new GameObject("Walls").transform;
            floor.gemHolder.parent = floor.wallHolder.parent = floor.transform;
            floor.gemHolder.localPosition = floor.wallHolder.localPosition = Vector3.zero;
            floor.map = worldMap;
            floor.transform.parent = transform;
            DrawMap(floor);
            floor.floorNumber = i;
            CombineMesh(floor);
            PlaceStairsUp(floor);
            PlaceStairsDown(floor);
            floor.FindEmpties();
            PlaceGems(floor);
            //floor.Enter();
            floors[i] = floor;
            //StartCoroutine(BumpWalls(floors[i]));
            if (i != 0)
            {
                Vector3 offset = floors[i - 1].stairsUp.transform.position - floor.stairsDown.transform.position;
                floor.transform.position = offset;
            }
            floors[i].gameObject.SetActive(false);
        }
    }

    int[,] GenerateMap()
    {
        int[,] worldMap = new int[worldSize, worldSize];
        for (int i = 0; i < worldSize; i++)
        {
            for (int j = 0; j < worldSize; j++)
            {
                worldMap[i, j] = Random.value < mapWallChance ? 0 : 1;
            }
        }
        for (int i = 0; i < simulationSteps; i++)
        {
            worldMap = DoStep(worldMap);
        }
        int[,] map = ArrayUtils.Create2D(1, worldSize + 2, worldSize + 2);
        map = ArrayUtils.Merge2D(map, worldMap, 1, 1);
        map = GetLargestSection(map);
        return map;
    }

    public int[,] GetLargestSection(int[,] map)
    {
        int largestCount = 0;
        int num = 2;
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y]==0)
                {
                    int tSize = 0;
                    ArrayUtils.Flood2D(ref map, x, y, 0, 3, ref tSize);
                    num++;
                    if (tSize > largestCount)
                    {
                        map = ArrayUtils.SwapValue2D(map, 2, 1);
                        map = ArrayUtils.SwapValue2D(map, 3, 2);
                        largestCount = tSize;
                    }else
                    {
                        map = ArrayUtils.SwapValue2D(map, 3, 1);
                    }
                }
            }
        }
        map = ArrayUtils.SwapValue2D(map, 2, 0);
        return map;
    }

    public int[,] DoStep(int[,] oldMap)
    {
        int[,] newMap = new int[oldMap.GetLength(0), oldMap.GetLength(1)];
        for (int x = 0; x < oldMap.GetLength(0); x++)
        {
            for (int y = 0; y < oldMap.GetLength(1); y++)
            {
                int nbs = CountAliveNeighbours(oldMap, x, y);
                if (oldMap[x, y] == 1)
                {
                    if (nbs < deathLimit)
                    {
                        newMap[x, y] = 0;
                    }
                    else
                    {
                        newMap[x, y] = 1;
                    }
                }
                else
                {
                    if (nbs > birthLimit)
                    {
                        newMap[x, y] = 1;
                    }
                    else
                    {
                        newMap[x, y] = 0;
                    }
                }
            }
        }
        return newMap;
    }

    public int CountAliveNeighbours(int[,] map, int x, int y)
    {
        int count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int neighbour_x = x + i;
                int neighbour_y = y + j;
                if (i == 0 && j == 0)
                    continue;
                else if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= map.GetLength(0) || neighbour_y >= map.GetLength(1))
                {
                    count = count + 1;
                }
                else if (map[neighbour_x, neighbour_y] == 1)
                {
                    count = count + 1;
                }
            }
        }
        return count;
    }

    void DrawMap(FloorMaster floor)
    {
        floor.walls = new List<GameObject>();
        for (int i = 1; i < floor.map.GetLength(0)-1; i++)
        {
            for (int j = 1; j < floor.map.GetLength(1)-1; j++)
            {
                if (floor.map[i, j] == 1)
                    continue;
                Vector3 rot;
                GameObject prefab = GetPrefab(floor.map, i,j, out rot);
                GameObject g = Instantiate<GameObject>(prefab);
                g.transform.parent = floor.wallHolder;
                g.transform.localPosition = new Vector3(i * unitSize, 0, j * unitSize);
                g.transform.localEulerAngles = rot;
                floor.walls.Add(g);
            }
        }
    }

    GameObject GetPrefab(int[,] map, int i, int j, out Vector3 rot)
    {
        rot = Vector3.zero;
        if (map[i - 1, j] == 1 && map[i + 1, j] == 0 && map[i, j - 1] == 0 && map[i, j + 1] == 0)
        {
            rot = Vector3.up * 270;
            return wallPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 1 && map[i, j - 1] == 0 && map[i, j + 1] == 0)
        {
            rot = Vector3.up * 90;
            return wallPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 0 && map[i, j - 1] == 1 && map[i, j + 1] == 0)
        {
            rot = Vector3.up * 180;
            return wallPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 0 && map[i, j - 1] == 0 && map[i, j + 1] == 1)
        {
            return wallPrefab;
        }
        else if (map[i - 1, j] == 1 && map[i + 1, j] == 0 && map[i, j - 1] == 1 && map[i, j + 1] == 0)
        {
            rot = Vector3.up * 180;
            return cornerPrefab;
        }
        else if (map[i - 1, j] == 1 && map[i + 1, j] == 0 && map[i, j - 1] == 0 && map[i, j + 1] == 1)
        {
            rot = Vector3.up * 270;
            return cornerPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 1 && map[i, j - 1] == 1 && map[i, j + 1] == 0)
        {
            rot = Vector3.up * 90;
            return cornerPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 1 && map[i, j - 1] == 0 && map[i, j + 1] == 1)
        {
            return cornerPrefab;
        }
        else if (map[i - 1, j] == 1 && map[i + 1, j] == 1 && map[i, j - 1] == 1 && map[i, j + 1] == 0)
        {
            rot = Vector3.up * 180;
            return uPrefab;
        }
        else if (map[i - 1, j] == 1 && map[i + 1, j] == 1 && map[i, j - 1] == 0 && map[i, j + 1] == 1)
        {
            return uPrefab;
        }
        else if (map[i - 1, j] == 1 && map[i + 1, j] == 0 && map[i, j - 1] == 1 && map[i, j + 1] == 1)
        {
            rot = Vector3.up * 270;
            return uPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 1 && map[i, j - 1] == 1 && map[i, j + 1] == 1)
        {
            rot = Vector3.up * 90;
            return uPrefab;
        }
        else if (map[i - 1, j] == 1 && map[i + 1, j] == 1 && map[i, j - 1] == 0 && map[i, j + 1] == 0)
        {
            rot = Vector3.up * 90;
            return oPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 0 && map[i, j - 1] == 1 && map[i, j + 1] == 1)
        {
            return oPrefab;
        }

        return floorPrefab;
    }

    void CombineMesh(FloorMaster floor)
    {
        MeshFilter[] meshFilters = floor.GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> combine = new List<CombineInstance>();
        int i = 0;
        MeshFilter mf = meshFilters[i];
        int verts = 0;
        Mesh m;
        while (i < meshFilters.Length)
        {
            if(verts+ meshFilters[i].sharedMesh.vertexCount > 60000)
            {
                m = new Mesh();
                m.CombineMeshes(combine.ToArray());
                AutoWeld(m, weldDistance, weldBucket);
                BumpWalls(m);
                mf.mesh = m;
                mf.gameObject.SetActive(true); mf.transform.localPosition = mf.transform.localEulerAngles = Vector3.zero;
                mf = meshFilters[i];
                verts = 0;
                combine = new List<CombineInstance>();
            }
            verts += meshFilters[i].sharedMesh.vertexCount;
            CombineInstance c = new CombineInstance();
            c.mesh = meshFilters[i].sharedMesh;
            c.transform = meshFilters[i].transform.localToWorldMatrix;
            combine.Add(c);
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }

        m = new Mesh();
        m.CombineMeshes(combine.ToArray());
        AutoWeld(m, weldDistance, weldBucket);
        BumpWalls(m);
        mf.mesh = m;
        mf.gameObject.SetActive(true);
        mf.transform.localPosition = mf.transform.localEulerAngles = Vector3.zero;

        for (int j = 0; j < meshFilters.Length; j++)
        {
            if (!meshFilters[j].gameObject.activeSelf)
            {
                Destroy(meshFilters[j].GetComponent<MeshRenderer>());
                Destroy(meshFilters[j]);
                meshFilters[j].gameObject.SetActive(true);
            }
        }

    }

    public static void AutoWeld(Mesh mesh, float threshold, float bucketStep)
    {
        Vector3[] oldVertices = mesh.vertices;
        Vector3[] newVertices = new Vector3[oldVertices.Length];
        int[] old2new = new int[oldVertices.Length];
        int newSize = 0;

        // Find AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
            if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
            if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
            if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
            if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
            if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
        }

        // Make cubic buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;

        List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < oldVertices.Length; i++)
        {
            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
            int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
            int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

            // Check to see if it's already been added
            if (buckets[x, y, z] == null)
                buckets[x, y, z] = new List<int>(); // Make buckets lazily

            for (int j = 0; j < buckets[x, y, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2new[i] = buckets[x, y, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = oldVertices[i];
            buckets[x, y, z].Add(newSize);
            old2new[i] = newSize;
            newSize++;

        skip:;
        }

        // Make new triangles
        int[] oldTris = mesh.triangles;
        int[] newTris = new int[oldTris.Length];
        for (int i = 0; i < oldTris.Length; i++)
        {
            newTris[i] = old2new[oldTris[i]];
        }

        Vector3[] finalVertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
            finalVertices[i] = newVertices[i];

        mesh.Clear();
        mesh.vertices = finalVertices;
        mesh.triangles = newTris;
        mesh.RecalculateNormals();
    }

    void BumpWalls(Mesh m)
    {

        List<Vector3> verts = new List<Vector3>();
        Vector3[] old = m.vertices;
        Vector3[] nor = m.normals;
        int count = m.vertices.Length;
        for (int j = 0; j < count; j++)
        {
            Vector3 v = old[j];
            v += Vector3.Scale(nor[j]*Random.value, bumpDistance);
            verts.Add(v);
        }
        m.SetVertices(verts);
        m.RecalculateNormals();
    }

    void PlaceStairsUp(FloorMaster floor)
    {
        Vector3 pos = floor.FindEmptyNoWall(false);
       // pos += Vector3.one ;
        floor.stairsUp = Instantiate<StairsController>(stairsUpPrefab);
        floor.stairsUp.player = player;
        floor.stairsUp.gen = this;
        floor.stairsUp.transform.parent = floor.transform;
        floor.stairsUp.transform.localPosition = pos;
        if (floor.floorNumber != 0)
            floor.stairsUp.transform.localEulerAngles = floors[floor.floorNumber - 1].stairsUp.transform.localEulerAngles;
    }

    void PlaceStairsDown(FloorMaster floor, int tries = 20)
    {
        Vector3 pos = floor.stairsUp.transform.localPosition;
        //pos += Vector3.one ;
        float dist = 0;
        while (dist == 0)
        {
            for (int i = 0; i < tries; i++)
            {
                Vector3 v = floor.FindEmptyNoWall(false);
                float d = Vector3.Distance(v, pos);
                if (d > dist)
                {
                    pos = v;
                    dist = d;
                }
            }
        }
        floor.stairsDown = Instantiate<StairsController>(stairsDownPrefab);
        floor.stairsDown.player = player;
        floor.stairsDown.gen = this;
        floor.stairsDown.transform.parent = floor.transform;
        pos.x = Mathf.FloorToInt(pos.x);
        pos.z = Mathf.FloorToInt(pos.z);
        floor.stairsDown.transform.localPosition = pos;
        floor.stairsDown.transform.localEulerAngles = Vector3.up * Mathf.Floor(Random.value*4)*90;
    }

    void PlaceGems(FloorMaster floor)
    {
        SpawnInfo info = spawnInfo[floor.floorNumber];
        for (int i = 0; i < info.floorItems.Length; i++)
        {
            SpawnItem item = info.floorItems[i];
            for (int j = 0; j < item.count; j++)
            {
                Spawnable g = Instantiate<Spawnable>(item.prefab);
                g.Spawn(floor);
            }
        }
    }

}

[System.Serializable]
public struct SpawnInfo
{
    public SpawnItem[] floorItems;
}

[System.Serializable]
public struct SpawnItem
{
    public Spawnable prefab;
    public int count;
}
