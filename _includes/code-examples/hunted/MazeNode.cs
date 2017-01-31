using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeNode {

    public Vector3 position;
    public List<MazeNode> neighbours;
    public bool visited, spawnPoint;
    public MazeNode previous;
    //public Transform transform;

    public MazeNode(){
    }

    public MazeNode (float x, float y, float z){
        position = new Vector3(x, y, z);
        neighbours = new List<MazeNode>();
        visited = false;
        spawnPoint = false;
    }

    public int getVisistedNeighbours(){
        int num = 0;
        foreach(MazeNode n in neighbours){
            if(n.visited){
                num++;
            }
        }
        return num;
    }
    
    public MazeNode getRandomNeighbour(){
        List<MazeNode> validNeighbours = new List<MazeNode>();
        foreach(MazeNode n in neighbours){
            if(!n.visited){
                validNeighbours.Add(n);
            }
        }
        return (validNeighbours.Count == 0) ? null : validNeighbours [Random.Range (0, validNeighbours.Count)];
    }

    public GameObject validNeighbour(MazeNode node, LayerMask mask){
        if(!spawnPoint){
            RaycastHit main;
            if(Physics.Linecast(node.position, position, out main, mask)){
                foreach(MazeNode n in neighbours){
                    if(n.spawnPoint) return null;
                    RaycastHit hit;
                    if(Vector3.Distance(node.position, n.position) < 7.5f && Physics.Linecast(n.position, node.position, out hit, mask))
                        if(hit.collider == main.collider) return null;
                }
                return main.collider.gameObject;
            } else return null;
        } else return null;
    }
}
