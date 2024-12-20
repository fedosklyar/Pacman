using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    public GameObject wall;

    public GameObject pellet;

    public GameObject powerPellet;

    public GameObject Node;

    private List<GameObject> pellets;

    private int pelletCount;
    //public GameObject Pacman;

    // public List 
    //[SerializeField] private Ghost[] ghosts;

    public static int width = 28;

    public static int height = 31;

    public int startX = -14;
    public int startY = -16;

    private struct Branch
    {   
        //Vector2 struct already has neccessary members (up, down, left, right)
        public Vector2 direction;
        public Vector2 start;
        public Vector2 end;


        public Branch(Vector2 start, Vector2 end, Vector2 direction)
        {
            this.start = start;
            this.end = end;
            this.direction = direction;
        }

    }

    private struct SubBranch
    {
        public Branch parentBranch;

        public Vector2 start;

        public Vector2 direction;

        public Vector2 rotationCoordinates;

        public Vector2 rotationDirection;


        public SubBranch(Branch parent)
        {
            this.parentBranch = parent;
            this.start = parent.start;
            this.direction = Vector2.zero;
            this.rotationCoordinates = Vector2.zero;
            this.rotationDirection = Vector2.zero;
        }

        public SubBranch(Branch parent, Vector2 direction, Vector2 rotationCoord, Vector2 rotationDir)
        {
            this.parentBranch = parent;
            this.start = parent.start;
            this.direction = direction;
            this.rotationCoordinates = rotationCoord;
            this.rotationDirection = rotationDir;
        }
    }

    private int generatedBranches;
    private List<Branch> Branches;

    private List<SubBranch> SubBranches;

    //matrix representation of the maze; bool type or int type???
    private int [,] maze = new int [width, height];

    //the variable which keeps track of offset for both axis for render of the tile

    // private void Start()
    // {
    //     //StopAllCoroutines();
    //     GenerateLevel();
    // }

    // private void Update()
    // {
    //     //need to read how to correctly restart the component
    //     if(Input.GetKeyDown(KeyCode.P))
    //         GenerateLevel();
    // }

    public void GenerateLevel()
    {

        Branches = new List<Branch>();
        SubBranches = new List<SubBranch>();
        pelletCount = 0;
        generatedBranches = 0;
        //StartCoroutine(
        GenerateMazeData();
        //);
    }

    private void GenerateMazeData()
    {
        InstantiateMaze();
        GenerateGhostHome();
        GenerateBranches();
        GenerateSubBranches();
        
        
        //WaitForSeconds delay = new WaitForSeconds(3.0f);

        string MazeMatrix = String.Empty;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                MazeMatrix += maze[i,j] + " ";
                //Changed the condition to > 0, because the positive value will represent the wall,
                //either modifiable or   not
                if (maze[i, j] > 0)
                    //yield return 
                    Instantiate(wall, new Vector3(startX + i, startY + j, 0), Quaternion.identity);
                
                //If maze[i,j] <= 0, we instantiate nodes and pellets
                //maze[i,j] - 3 for the inners of the ghost home, where i do not need any nodes or pellets
                else if(maze[i, j] <= 0 && maze[i, j] > -3)
                {
                    //If maze[i,j] <= 0, we instantiate nodes
                    if(checkTurn(i, j))
                    {
                        Debug.Log("I = " + i.ToString() + " J = " + j.ToString());
                        Instantiate(Node, new Vector3(startX + i, startY + j, -1), Quaternion.identity);
                    }

                    //for that case, we need instantiate only pellets 
                    if (maze[i, j] == 0)
                    {
                        //if we have the corner of the level, instantiate a power pellt
                        if((i == 1 || i == width - 2) && (j == 1 || j == height - 2))
                        {
                            //Instantiate(powerPellet, new Vector3(startX + i, startY + j, 0), Quaternion.identity);
                            var pellet = Instantiate(powerPellet, new Vector3(startX + i, startY + j, 0), Quaternion.identity);
                            //pellets.Add(pellet);
                            ++pelletCount;                
                        }
                        //if not the case, just the basic pellet
                        else
                        {
                            var pellet = Instantiate(this.pellet, new Vector3(startX + i, startY + j, 0), Quaternion.identity);              
                            ++pelletCount;
                            //pellets.Add(pellet);
                        }
                    }
                }
            }
            MazeMatrix += "\n";
        }


        //Instantiate(Pacman, new Vector3(0.0f, -3.5f, -3), Quaternion.identity);
        //Pacman.transform.position = new Vector3(0.0f, -3.5f, -3);
        Debug.Log(MazeMatrix);
    }

    public int GetPelletsCount()
    {
        return pelletCount;
    }

    public void DestroyLevel()
    {
        ClearLayer("Obstacle");
        ClearLayer("Pallets");
        ClearLayer("Node");
    }

    private void ClearLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        GameObject[] objects = FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            if (obj.layer == layer)
            {
                Destroy(obj);
            }
        }
    }

    

    private bool checkTurn(int i, int j)
    {
        //we iterate through maze from bottom to top
        //if we have the path in right direction, we can place the node
        //for further resolution of the direction for ghost
        if((maze[i,j] < 1 && maze[i + 1, j] < 1 && maze[i, j + 1] < 1) ||
            //added check for the left turn
            (maze[i,j] < 1 && maze[i - 1, j] < 1 && maze[i, j + 1] < 1) || 
            //added check for the left direction and turn to the bottom
            (maze[i,j] < 1 && maze[i - 1, j] < 1 && maze[i, j - 1] < 1) ||
            //added check for the right direction and turn to the bottom
            (maze[i,j] < 1 && maze[i + 1, j] < 1 && maze[i, j - 1] < 1)

            //|| 
            //check for corners except the bottom left
            //(i == width - 2 && (j == 1 || j == height - 2)) || (i == 1 && j == height - 2)
            )
            return true;
        return false;
        
    }

    private void GenerateBranches()
    {
        //Probably, will need to make is a field of the class for futher operation in other methods
        int branchesQuan = UnityEngine.Random.Range(10, 13);
        this.generatedBranches = 0;

        Debug.Log("Quan of branches = " + branchesQuan.ToString());

        //The quantity of elements in list update during the runtime as needed
        //and the loop will be executed until list will not include the necessary count of branches
        while(this.generatedBranches < branchesQuan)
        //for(int i = 0; i < branchesQuan; i++)
        {
            Debug.Log("Quantity of Branches in list" + Branches.Count.ToString());
            Debug.Log("Quantity of Branches in counter" + generatedBranches.ToString());
            int mazeY, mazeX;
            Vector2 direction;
            //int mazeX = Random.Range(9, 18);
            
            //Added this random to make the possibility of spawning of the path in all directions
            //more equal
            if (UnityEngine.Random.value < 0.5)
                mazeX = UnityEngine.Random.Range(10, 17);
            else
                mazeX = UnityEngine.Random.value < 0.5? 9 : 18;


            if(mazeX == 9 || mazeX == 18)
            {
                mazeY = UnityEngine.Random.Range(12, 18);

                //if I have bottom row, I can go any direction exept above
                if(mazeY < 13)
                {
                    //If I have left bottom cell, I can go left or down
                    if(mazeX == 9)
                        direction = UnityEngine.Random.value < 0.5? Vector2.left : Vector2.down;
                    //If I have right bottom cell, I can go right or down
                    else
                        direction = UnityEngine.Random.value < 0.5? Vector2.right : Vector2.down;
                }
                //if I have top row, I can go any direction exept down
                else if (mazeY > 17)
                {
                    //If I have left top cell, I can go left or up
                    if(mazeX == 9)
                        direction = UnityEngine.Random.value < 0.5? Vector2.left : Vector2.up;
                    //If I have right top cell, I can go right or up
                    else
                        direction = UnityEngine.Random.value < 0.5? Vector2.right : Vector2.up;
                }

                //if I have cell in on of the columns, I can go only left or right
                //leaning on the column location
                else
                    direction = mazeX == 9 ? Vector2.left : Vector2.right;
            }

            //The scenario for rows, boundary cells have been already checked
            else
            {
                mazeY = UnityEngine.Random.value < 0.5 ? 12 : 18;

                direction = mazeY == 12 ? Vector2.down: Vector2.up; 

            }

            Debug.Log("Maze X = " + mazeX.ToString() + " Maze Y = " + mazeY.ToString());
            //Generating the branch leaning on the obtained coordinates and direction
            GenerateBranch(new Vector2(mazeX, mazeY), direction);
        }
    }

    private void GenerateBranch(Vector2 start, Vector2 direction)
    {
        //CheckBranchValidity();

        //If we go right or left, we need to delete 8 tiles
        //if we go top or bottow - 11 tiles;
        int numSpacesMove = 
            direction == Vector2.left || direction == Vector2.right ? 8 : 11;
        
        //
        int x = (int) start.x;
        int y = (int) start.y;

        var newBranch = new Branch( start,
        //For the sake of simplicity, I will place the end as zero,
        //cause I do not need it for the check of validness, but,
        //probably, that piece of data will be necessaty further
                                    Vector2.zero,
                                    direction);

        //would be good to check the validness of generated branch
        //because we do not want the neighbouring or overlaping of branches 
        if (BranchIsValid(newBranch))
        {
            Branches.Add(newBranch);
            this.generatedBranches++;

            for(int i = 0; i < numSpacesMove; i++)
            {
                //Increase the value of x and y to go further in path
                x += (int) direction.x;
                y += (int) direction.y;

                //Make the path, going through all necessary tiles and make them empty
                maze[x, y] = 0;
            }  
        }
    }

    private bool BranchIsValid(Branch newBranch)
    {
        //If the list is empty, we just add a Branch
        if(Branches.Count > 0)
        {
            for(int i = 0; i < Branches.Count; i++)
            {
                var currentBranch = Branches[i];
                
                if (math.abs(newBranch.start.x -  currentBranch.start.x) < 2 && //changed or to and 
                    math.abs(newBranch.start.y -  currentBranch.start.y) < 2 && 
                    newBranch.direction == currentBranch.direction)
                  {
                    return false;
                  }
            }
        }

        
        return true;
            
    }

    private void GenerateSubBranches()
    {   
        //works for 1
        int branchesQuan = UnityEngine.Random.Range(6, 9);

        for(int i = 0; i < branchesQuan; i++)
        {
            //Need to decide, whether I want to make possible the generation
            //of several subBranches for a single or I should somehow
            //Advance the random nature of choosing of the branch and also avoid the possibility of
            //generate 2 subbranches from single branch 
            int parentBranchIndex = UnityEngine.Random.Range(1, Branches.Count) - 1;
            var parentBranch = Branches[parentBranchIndex];
            GenerateSubBranch(parentBranch);
        }
    }

    private void GenerateSubBranch(Branch parentBranch)
    {
        Debug.Log("parent X = " + parentBranch.start.x.ToString() + "parent Y = " + parentBranch.start.y.ToString());
        var newSubBranch = new SubBranch(parentBranch);

        int offset;

        //Generate the offset relative to the start position of the parent's Branch direction 
        if (parentBranch.direction.x != 0)
        {
            offset = UnityEngine.Random.Range(2, 7);

            //if we have left direction for branch, we need to offset in left direction
            if (parentBranch.direction == Vector2.left)
                newSubBranch.start.x -= offset;
            //right for the right
            else
                newSubBranch.start.x += offset; 
        }
        else
        {
            //Probably, better to define the lower bound to two in the sake of avoidance
            //of intersection with path near ghost home
            offset = UnityEngine.Random.Range(2, 10);

            //The same approach applied for up and down directions
            if (parentBranch.direction == Vector2.up)
                newSubBranch.start.y += offset;

            else
                newSubBranch.start.y -= offset;
        }
           

        //if we have left or right direction, then subBranch must go up or down
        if (parentBranch.direction.x != 0)
            newSubBranch.direction = UnityEngine.Random.value < 0.5 ? Vector2.up : Vector2.down;

        //else we have up of down direction, so subBranch must go left or right
        else
            newSubBranch.direction = UnityEngine.Random.value < 0.5 ? Vector2.left : Vector2.right;

        //If subBranch not valid from the start, no need to check the further possibility of the rotation
        //NOTE: Such approach also can affect on the frequency of the generating of more complicated subBranches
        if(SubBranchIsValid(newSubBranch))
        {
            //Make the opposite variables for their modification in loop
            int startX = (int)newSubBranch.start.x;
            int startY = (int)newSubBranch.start.y;
            while(true)
            {
                startX += (int)newSubBranch.direction.x;
                startY += (int)newSubBranch.direction.y;

                //if we have a wall on our path, delete it
                if(maze[startX, startY] == 1)
                    maze[startX, startY] = 0;
                //if we faced in other path, break the making of the subBranch
                else
                    break;

            }
            //int length = UnityEngine.Random.Range(4, 7);

            //TODO: Logic for rotation for subBranch
        }

        Debug.Log("SubBranch X = " + newSubBranch.start.x.ToString() + "SubBranch Y = " + newSubBranch.start.y.ToString());
        Debug.Log("Subranch direction = " + newSubBranch.direction.x.ToString() + " " + newSubBranch.direction.y.ToString());
    }

    private bool SubBranchIsValid(SubBranch newSubBranch)
    {
        if(SubBranches.Count > 0)
        {
            for(int i = 0; i < SubBranches.Count; i++)
            {
                var currentSubBranch = SubBranches[i];
                
                if (math.abs(newSubBranch.start.x - currentSubBranch.start.x) < 2 && //changed or to and 
                    math.abs(newSubBranch.start.y - currentSubBranch.start.y) < 2 && 
                    currentSubBranch.direction == newSubBranch.direction)
                  {
                    return false;
                  }
            }
        }

        
        return true;
    }

    private void GenerateGhostHome()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Outer Rectangle Border (10x7, empty border)
                if ((i == 9 || i == 18) && j >= 12 && j <= 18 || 
                    (j == 12 || j == 18) && i >= 9 && i <= 18)   
                {
                    //make them as -2 to restrict the changes for generation algorithm of paths
                    //Probaly, can delete that, but than it can make the generating path process
                    //a bit inaffective, cause the algoritm will try to make the path where it is already exists 
                    maze[i, j] = -2;
                }

                // Inner Rectangle (solid border, 8x5)
                else if ((i == 10 || i == 17) && j >= 13 && j <= 17 || 
                        (j == 13 || j == 17) && i >= 10 && i <= 17)
                {
                    //Mark the border of the house by beside value to represent is as obstacle, but ummodifiable
                    maze[i, j] = 2;
                }
                
                // Inner Empty Area (inside the inner rectangle, adjusted column range)
                else if (i > 10 && i < 17 && j > 13 && j < 17)
                {
                    //Same as for the path near border
                    maze[i, j] = -3; 
                }
            }
        }
    }

    private void InstantiateMaze()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                maze[i, j] = 1;
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //The condition to draw the path inside near the edge
                //Must be handy for ghost spawner render
                if (((i == 1 || i == width - 2) &&  j > 0 && j < height - 1) 
                || ((j == 1 || j == height - 2) && i > 0 && i < width - 1))
                    //Mark it with -2 either because that path is predifined, not generated
                    maze[i, j] = 0;
            }
        }
    }
}
