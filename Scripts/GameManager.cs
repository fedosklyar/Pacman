using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Ghost[] ghosts;
    [SerializeField] private Pacman pacman;
    //[SerializeField] private Transform pellets;
    [SerializeField] private GameObject pellets;
    //[SerializeField] private Text gameOverText;
    //[SerializeField] private Text scoreText;
    //[SerializeField] private Text livesText;

    private int pelletCount;

    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 3;

    private int ghostMultiplier = 1;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        //Made it on synchronous manner
        //int pellets = GameObject.FindObjectsOfType(typeof(Pellet)).Length;
        //this.pelletCount = GetComponent<LevelGenerator>().GetPelletsCount();
        //int pellets = 30;
        //Debug.Log(pelletCount);
        NewGame();
    }

    private void Update()
    {
        if (lives <= 0 && Input.anyKeyDown) {
            NewGame();
        }
    }

    private void NewGame()
    {
        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        //create a level synchronously
        var generator = GetComponent<LevelGenerator>();
        generator.GenerateLevel();

        //receive the quantity of pellets
        this.pelletCount = GetComponent<LevelGenerator>().GetPelletsCount();
        Debug.Log(pelletCount);
        //gameOverText.enabled = false;

        //foreach (Transform pellet in pellets) {
        foreach (Transform pellet in pellets.transform) {
            pellet.gameObject.SetActive(true);
        }

        ResetState();
    }

    private void ResetState()
    {
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].ResetState();
        }

        pacman.ResetState();
    }

    private void GameOver()
    {
        //gameOverText.enabled = true;

        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].gameObject.SetActive(false);
        }

        pacman.gameObject.SetActive(false);
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        //livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int score)
    {
        this.score = score;
        //scoreText.text = score.ToString().PadLeft(2, '0');
    }

    public void PacmanEaten()
    {
        //pacman.DeathSequence();
        this.pacman.gameObject.SetActive(false);
        
        SetLives(lives - 1);

        if (lives > 0) {
            Invoke(nameof(ResetState), 3f);
        } else {
            GameOver();
        }
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);

        ghostMultiplier++;
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);

        SetScore(score + pellet.points);

        --this.pelletCount;

        Debug.Log(this.pelletCount);
        
        

        if (!HasRemainingPellets())
        {
            Debug.Log("No pellets");
            pacman.gameObject.SetActive(false);
            //added the destroying of the level (removal of instantiated objects)
            GetComponent<LevelGenerator>().DestroyLevel();
            Invoke(nameof(NewRound), 3f);
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private bool HasRemainingPellets()
    {
        //foreach (Transform pellet in pellets)
        // foreach (Transform pellet in pellets.transform)
        // {
        //     if (pellet.gameObject.activeSelf) {
        //         return true;
        //     }
        // }

        if(this.pelletCount > 0)
            return true;

        return false;
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }

}
