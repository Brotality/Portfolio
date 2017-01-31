using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class MazeGenerator : MonoBehaviour {

    public bool generateOnStart;
    public int startSize;

    [SerializeField] private GameObject basicWallPrefab, pillarPrefab;
    [SerializeField] private LayerMask wallMask;

    private int gridSize;
    private bool backtracking;
    private List<MazeNode> MazeNodes;
    public List<MazeNode> spawnPoints {get; private set;}
    private GameObject floor;

    public void Start(){
        if(generateOnStart)
            newMaze(startSize);
    }

    public void newMaze(int size){
        MazeNodes = new List<MazeNode>();
        spawnPoints = new List<MazeNode>();
        backtracking = false;
        gridSize = size;
        scaleFloor();
        createGrid();
        createMazeNodes();
        generateMaze(MazeNodes[0]);
    }

    private void generateMaze(MazeNode node){
        node.visited = true;
        if (node.getVisistedNeighbours() == node.neighbours.Count) {
            if(!backtracking){
                spawnPoints.Add (node);
                node.spawnPoint = true;
                GameObject go = new GameObject("DeadEnd " + node.position.x + ", " + node.position.y);
                go.transform.position = node.position;
                go.transform.parent = transform;
                go.tag = "DeadEnd";
                if(allVisited())
                    return;
                backtracking = true;
            }
            generateMaze(node.previous);
        } else {
            MazeNode n = node.getRandomNeighbour();
            if(n == null)
                return;
            RaycastHit hit;
            if (Physics.Linecast (node.position, n.position, out hit)) {
                if (hit.collider.gameObject.tag == "Wall") {
                    Destroy (hit.collider.gameObject);
                    backtracking = false;
                    n.previous = node;
                    generateMaze(n);
                }
            }
        }
    }

    private bool allVisited(){
        foreach(MazeNode n in MazeNodes){
            if(!n.visited){
                return false;
            }
        }
        return true;
    }

    public IEnumerator removeRandomWalls(){
        int num = gridSize/2;
        List<MazeNode> nodes = new List<MazeNode>(MazeNodes);
        while(num > 0 && nodes.Count > 0){
            MazeNode node = nodes[Random.Range (0, nodes.Count)];
            if(!node.spawnPoint){
                GameObject wall = null;
                foreach(MazeNode n in node.neighbours){
                    wall = n.validNeighbour(node, wallMask);
                    if(wall != null){
                        Destroy(wall);
                        num--;
                        yield return new WaitForEndOfFrame();
                        break;
                    }
                }
            }
            nodes.Remove(node);
        }
    }

    private void scaleFloor(){
        floor = transform.Find("FloorOffset/Floor").gameObject;
        floor.transform.parent.localScale = new Vector3(gridSize, gridSize, gridSize);
        Vector2 scale = new Vector2(gridSize, gridSize);
        floor.GetComponent<Renderer>().material.SetTextureScale("_DiffuseMain", scale);
        floor.GetComponent<Renderer>().material.SetTextureScale("_NormalMain", scale);
        floor.GetComponent<Renderer>().material.SetTextureScale("_DiffuseDetail", scale);
        floor.GetComponent<Renderer>().material.SetTextureScale("_NormalDetail", scale);
        floor.GetComponent<Renderer>().material.SetTextureScale("_CustomAO", scale);
    }

    private void createGrid(){
        float multi = 5F, offset = 2.5F;
        for(int a = 0; a < (gridSize+1); a++){
            for(int i = 0; i < (gridSize+1); i++){
                string name = a + ", " + i;
                if(i < gridSize){
                    createWall(new Vector3(((i*multi)+offset), offset, (a*multi)), ("Wall " + name + " X"), 0);
                    createWall(new Vector3(((a*multi)), offset, ((i*multi)+offset)), ("Wall " + name + " Y"), 90);
                }
                createPillar(new Vector3(((i*multi)), 0, (a*multi)), ("Pillar " + name));
            }
        }
    }

    private void createMazeNodes(){
        float multi = 5F, offset = 2.5F;
        for(int z = 0; z < gridSize; z++){
            for(int x = 0; x < gridSize; x++){
                MazeNode node = new MazeNode(((x*multi)+offset), offset, ((z*multi)+offset));
                if(z > 0 && x != 0){
                    node.neighbours.Add(MazeNodes[((z*gridSize) + (x-1))]);
                    node.neighbours.Add(MazeNodes[(((z-1)*gridSize) + (x))]);
                    MazeNodes[((z*gridSize) + (x-1))].neighbours.Add(node);
                    MazeNodes[(((z-1)*gridSize) + (x))].neighbours.Add(node);
                }
                else if (z > 0 && x == 0){
                    node.neighbours.Add(MazeNodes[(((z-1)*gridSize) + (x))]);
                    MazeNodes[(((z-1)*gridSize) + (x))].neighbours.Add(node);
                }
                else if(z == 0 && x != 0){
                    node.neighbours.Add(MazeNodes[((z*gridSize) + (x-1))]);
                    MazeNodes[((z*gridSize) + (x-1))].neighbours.Add(node);
                }
                MazeNodes.Add(node);
            }
        }
    }
    
    private void createWall(Vector3 pos, string name, float rotation){
        GameObject wall = (GameObject) Instantiate(basicWallPrefab, pos, transform.rotation);
        wall.name = name;
        wall.transform.parent = transform;
        wall.transform.eulerAngles = new Vector3(0, rotation, 0);
    }

    private void createPillar(Vector3 pos, string name){
        GameObject p = (GameObject) Instantiate(pillarPrefab, pos, transform.rotation);
        p.name = name;
        p.transform.parent = transform;
    }

    /*  Bounds b = wall.collider.bounds;
        GraphUpdateObject guo = new GraphUpdateObject(b);
        AstarPath.active.UpdateGraphs (guo);
     */
}
