using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour {

    [SerializeField] private FMODAsset ambience, win, lose;
    [SerializeField] private FMODAsset[] chase;
    
    [HideInInspector] public Difficulty difficulty;
    [HideInInspector] public bool loading;

    public int keysOnMap {get; private set;}
    public bool GameOver {get; private set;}
    public AudioControl audioControl {get; private set;}
    public MenuControl menuControl {get; private set;}
    public SpawnControl spawnControl {get; private set;}
    public Player player {get; private set;}
    public List<GameObject> enemies {get; private set;}
    private List<GameObject> enemiesChasing;
    private GameObject mainLight, mainCamera;

    void Awake () {
        DontDestroyOnLoad(gameObject);
        if(PlayerPrefs.GetInt("initialisedPlayerPrefs") != 1)
            initPlayerPrefs();
        difficulty = (Difficulty) PlayerPrefs.GetInt("difficulty");
        mainCamera = transform.GetChild(0).gameObject;
        audioControl = GetComponent<AudioControl>();
        menuControl = GetComponent<MenuControl>();
        spawnControl = GetComponent<SpawnControl>();
    }

    void OnLevelWasLoaded(int level){
        if(level != 0){
            switch(Application.loadedLevelName){
                case "Maze" : 
                    StartCoroutine("newMazeGame");
                    float mazeWidth = (25*(((int) difficulty) + 1));
                    transform.position = new Vector3(mazeWidth/2, (mazeWidth/2)+5, -(5 * (((int) difficulty) + 1)));
                    transform.eulerAngles = new Vector3(45, 0, 0);
                    break;
                case "Dungeon" : 
                    break;
                default : 
                    Debug.LogError("Level not handled"); break;
            }
        } else if(loading)
            Destroy(gameObject);
    }

    private IEnumerator newMazeGame(){
        mainLight = GameObject.FindWithTag("MainLight");

        menuControl.loadingState = "Generating Maze...";
        MazeGenerator mazeGenerator = GameObject.FindWithTag("Maze").GetComponent<MazeGenerator>();
        yield return new WaitForEndOfFrame();
        mazeGenerator.newMaze(5 * (((int) difficulty) + 1));
        yield return new WaitForEndOfFrame();

        menuControl.loadingState = "Funerizing Maze...";
        yield return mazeGenerator.StartCoroutine("removeRandomWalls");

        menuControl.loadingState = "Generating navmesh...";
        Pathfinding.GridGraph graph = (Pathfinding.GridGraph) AstarPath.active.graphs[0];
        int graphSize = 50 * ((((int) difficulty) + 1));
        float pos = graphSize/2 * graph.nodeSize;
        graph.width = graphSize;
        graph.depth = graphSize;
        graph.center = new Vector3(pos, 0, pos);
        graph.UpdateSizeFromWidthDepth();
        AstarPath.active.Scan();
        yield return new WaitForEndOfFrame();

        menuControl.loadingState = "Generating keys and exit...";
        keysOnMap = (((int) difficulty) + 1) * 2;
        spawnControl.spawnPoints = new List<MazeNode>(mazeGenerator.spawnPoints);
        spawnControl.spawnKeys(keysOnMap);
        spawnControl.spawnDoor();
        yield return new WaitForEndOfFrame();

        menuControl.loadingState = "Spawning player...";
        player = spawnControl.spawnPlayer();
        yield return new WaitForEndOfFrame();

        menuControl.loadingState ="Spawning enemy...";
        enemies = new List<GameObject>();
        enemiesChasing = new List<GameObject>();
        spawnControl.spawnPoints = new List<MazeNode>(mazeGenerator.spawnPoints);
        for(int i = 0; i < ((int) difficulty); i++)
            spawnEnemy();
        yield return new WaitForEndOfFrame();

        menuControl.loadingState = "Initialising game...";
        audioControl.playAsset(ambience);
        StartCoroutine(updateParameterWithList(enemies, true));
        loading = false;
        setGameOver(false);
    }

    public float getNearestDistance(List<GameObject> list){
        float dist = 100;
        foreach(GameObject go in list)
            dist = Mathf.Min(dist, Vector3.Distance(player.transform.position, go.transform.position));
        return dist;
    }
    
    public void spawnEnemy(){
        enemies.Add(spawnControl.spawnEnemy());
    }

    public void addChasingEnemy(GameObject enemy){
        if(!enemiesChasing.Contains(enemy)){
            enemiesChasing.Add(enemy);
            if(enemiesChasing.Count == 1){
                StopAllCoroutines();
                audioControl.playAsset(chase[Random.Range(0, chase.Length)]);
                StartCoroutine(updateParameterWithList(enemiesChasing, false));
            }
        }
    }
    
    public void removeChasingEnemy(GameObject enemy){
        if(enemiesChasing.Contains(enemy)){
            enemiesChasing.Remove(enemy);
            if(enemiesChasing.Count == 0){
                StopAllCoroutines();
                StartCoroutine(returnToAmbience());
            }
        }
    }

    public int getNumberOfChasingEnemies(){
        return enemiesChasing.Count;
    }

    public IEnumerator playerDied(bool success, int keys){
        menuControl.playerDied((success) ? "Congratulations, you collected all the keys and escaped!" 
                                         : "You did not survive.\n" + keys + "/" + keysOnMap + " keys collected");
        removeEnemies();
        yield return new WaitForEndOfFrame();
        audioControl.playAsset((success) ? win : lose);
        audioControl.setParameter(0);
        setGameOver(true);
        Destroy(mainCamera.GetComponent<DepthOfFieldScatter>());
    }

    public void setGameOver(bool status){
        GameOver = status;
        mainCamera.SetActive(status);
        GetComponent<FMOD_Listener>().enabled = status;
        GetComponent<AudioListener>().enabled = status;
        RenderSettings.fog = !status;
        if(mainLight != null)
            mainLight.SetActive(status);
    }

    private void removeEnemies(){
        enemiesChasing.Clear();
        foreach(GameObject enemy in enemies)
            Destroy(enemy);
    }

    private IEnumerator returnToAmbience(){
        yield return StartCoroutine(audioControl.fadeVolume(0, 1));
        audioControl.playAsset(ambience);
        StartCoroutine(updateParameterWithList(enemies, true));
    }

    private IEnumerator updateParameterWithList(List<GameObject> list, bool invert){
        while(!GameOver){ 
            float dist = getNearestDistance(list)/30;
            audioControl.setParameter((invert ? 1-dist : dist));
            yield return null;
        }
    }

    private void initPlayerPrefs(){
        PlayerPrefs.SetInt("difficulty", 1);
        PlayerPrefs.SetInt("initialisedPlayerPrefs", 1);
    }
}
