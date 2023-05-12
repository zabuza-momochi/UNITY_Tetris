using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class GridMngr : Singleton<GridMngr>
{
    // ###############
    // Initialize vars
    // ###############

    // Two dimensional array (used as a matrix to save piece info)
    // [Y,X] Y rows = 20, X columns = 10
    GameObject[,] grid = new GameObject[20, 10];

    // Two dimensional array (used to count piece for each row)
    // [Y,X] Y rows = 20, X total_pieces = (0 - 10)
    int[,] piece_counter = new int[20, 1];

    // Index array
    int index_x;
    int index_y;

    // Particle effect on clear
    public ParticleSystem ClearParticle;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize grid
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                // Set as empty a single block of grid
                grid[i, j] = null;
            }
        }
    }

    // Check if a block is already occupied
    public bool CheckRow(GameObject piece)
    {
        // Scroll each single piece of block's container
        for (int i = 0; i < piece.transform.childCount; i++)
        {
            // Get index for check inside grid
            index_x = (int)Mathf.Abs(piece.transform.GetChild(i).transform.position.x);
            index_y = (int)Mathf.Abs(piece.transform.GetChild(i).transform.position.y) - 1;

            // Check is first row
            if (index_y < 0)
            {
                // Limit reached - exit
                return true;
            }

            // Clamp to avoid index out of range
            index_y = Mathf.Clamp(index_y, 0, grid.GetLength(0) - 1);
            index_x = Mathf.Clamp(index_x, 0, grid.GetLength(1) - 1);

            // Check if block inside grid is busy
            if (grid[index_y, index_x] != null)
            {
                // Block already busy - exit
                return true;
            }
        }

        // Block is empty
        return false;
    }


    // Set block inside grid
    public void SetRow(GameObject piece)
    {
        // Scroll each single piece of block's container
        for (int i = 0; i < piece.transform.childCount; i++)
        {
            // Get index for store piece inside grid
            index_x = (int)Mathf.Abs(piece.transform.GetChild(i).transform.position.x);
            index_y = (int)Mathf.Abs(piece.transform.GetChild(i).transform.position.y);

            // Clamp to avoid index out of range
            index_y = Mathf.Clamp(index_y, 0, grid.GetLength(0) - 1);
            index_x = Mathf.Clamp(index_x, 0, grid.GetLength(1) - 1);

            // Set block
            grid[index_y, index_x] = piece.transform.GetChild(i).gameObject;

            // Check for game over (if the block is above the last row)
            if (piece.transform.GetChild(i).transform.position.y > GameManager.Instance.Y_limits.x)
            {
                // Set game over
                GameManager.Instance.GameOver();

                // Exit
                return;
            }

            // Update current row for piece counter
            piece_counter[index_y, 0]++;
        }

        // Check if any rows are complete
        ClearRow();
    }

    // Clear row
    public void ClearRow()
    {
        // List used to save index of completed row (bottom to top)
        List<int> index_clean = new List<int>();

        // Scroll array of piece counter
        for (int i = 0; i < piece_counter.GetLength(0); i++)
        {
            // Check if row is completed
            if (piece_counter[i, 0] >= 10)
            {
                // Scroll grid
                for (int m = 0; m < piece_counter[i, 0]; m++)
                {
                    // Detach child (single piece) from parent (block's container) 
                    grid[i, m].gameObject.transform.SetParent(null, true);

                    // Destroy piece
                    Destroy(grid[i, m].gameObject);

                    // Resee block of grid - again empty
                    grid[i, m] = null;
                }

                // Reset row piece counter to 0
                piece_counter[i, 0] = 0;

                // Add index of cleaned row to list
                index_clean.Add(i);
            }
        }

        // Check if at least one row has been cleaned
        if (index_clean.Count > 0)
        {
            // Play particle effect
            ClearParticle.transform.position = new Vector3(ClearParticle.transform.position.x, index_clean[0] + 0.3f, 0);

            // Play particle effect
            ClearParticle.Play();

            // Play clip
            GameManager.Instance.PlaySFX(SFX.clear);

            // Move the blocks down
            ScrollDown(index_clean);

            // Update player'score
            GameManager.Instance.UpdateScore(index_clean.Count);
        }

        // Set bool for spawn new block
        SpawnMngr.Instance.canSpawn = true;
    }

    // Move the blocks down
    void ScrollDown(List<int> list_index_clean)
    {
        // Start from last cleaned row
        int index_row = list_index_clean.Count - 1;

        // Scroll index of cleaned rows -  for each cleaned rows scroll down
        for (int k = 0; k < list_index_clean.Count; k++)
        {
            // Scroll grid rows
            for (int i = list_index_clean[index_row]; i < grid.GetLength(0); i++)
            {
                // Scroll grid columns
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    // Check if block is busy
                    if (grid[i, j] != null)
                    {
                        // Move piece by the number of n_clean rows (set only new piece position)
                        grid[i, j].transform.position -= new Vector3(0, 1, 0);

                        // Move piece inside grid by the number of n_clean rows (change piece inside grid)
                        grid[i - 1, j] = grid[i, j];

                        // Set previous block position again as empty
                        grid[i, j] = null;
                    }
                }
            }

            // Scroll array of piece counter
            for (int m = list_index_clean[index_row]; m < piece_counter.GetLength(0) - 1; m++)
            {
                // Shift index array
                int swap = piece_counter[m + 1, 0];
                piece_counter[m + 1, 0] = piece_counter[m, 0];
                piece_counter[m, 0] = swap;
            }

            // Decrease index for next cleaned row (top to bottom)
            index_row--;
        }


        // Scroll all block's container
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Container"))
        {
            // Check if container has no child - empty block
            if (obj.transform.childCount == 0)
            {
                // Destroy container
                Destroy(obj);
            }
        }
    }

    // Check side free
    public bool CheckSide(GameObject piece, string dir = "left")
    {
        // Set default direction check to left
        int value = -1;

        // Check for right
        if (dir == "right")
        {
            // Set direction check to right
            value = 1;
        }

        // Scroll each single piece of block's container 
        for (int i = 0; i < piece.transform.childCount; i++)
        {
            // Get index for check piece inside grid
            index_x = (int)Mathf.Abs(piece.transform.GetChild(i).transform.position.x) + value;
            index_y = (int)Mathf.Abs(piece.transform.GetChild(i).transform.position.y);

            // Clamp to avoid index out of range
            index_y = Mathf.Clamp(index_y, 0, grid.GetLength(0) - 1);
            index_x = Mathf.Clamp(index_x, 0, grid.GetLength(1) - 1);

            // Check if block inside grid is busy
            if (grid[index_y, index_x] != null)
            {
                // Block already busy - exit
                return true;
            }
        }

        // Block is empty - side free
        return false;
    }

    // ##########################################################################################
    // UNDER CONSTRUCTION #######################################################################
    // ##########################################################################################

    // Check rotation free
    public bool CheckRotation(GameObject block)
    {
        // Check block if is over limit
        CheckRotationLimitLeft(block);
        CheckRotationLimitRight(block);
        CheckRotationLimitDown(block);

        // Counter for piece
        int pieceChecked = 0;
        int pieceFree = 0;

        // Scroll each single piece of block's container 
        for (int i = 0; i < block.transform.childCount; i++)
        {
            // Get index for check piece inside grid
            index_x = (int)Mathf.Abs(block.transform.GetChild(i).transform.position.x);
            index_y = (int)Mathf.Abs(block.transform.GetChild(i).transform.position.y);

            // Clamp to avoid index out of range
            index_y = Mathf.Clamp(index_y, 0, grid.GetLength(0) - 1);
            index_x = Mathf.Clamp(index_x, 0, grid.GetLength(1) - 1);

            // Check if block inside grid is empty
            if (grid[index_y, index_x] == null)
            {
                // Update counter
                pieceFree++;
            }
            // Check if block inside grid is busy
            else
            {
                // Check if previous up block inside grid is free
                if (grid[index_y + 1, index_x] == null)
                {
                    // Update counter
                    pieceChecked++;
                }
            }
        }

        // Check if all pieces of block are free
        if (pieceFree == 4)
        {
            Debug.Log("PIECE FREE");

            // Can rotate - all blocks's grid are empty
            return true;
        }

        // Checks if all pieces on grid are free (nearest up)
        if (pieceChecked == 4)
        {
            Debug.Log("PIECE CHECKED");

            // Move block position up
            block.transform.position += Vector3.up;

            // Can rotate - all blocks in grid are empty
            return true;
        }
        else
        {
            Debug.Log("PIECE FAILED");
            Debug.Log("PIECE FREE: " + pieceFree);

            // Can't rotate - some block in grid are already busy
            return false;
        }
    }

    // Check if block is over grid limit on x [< 0]
    void CheckRotationLimitLeft(GameObject block)
    {
        // Counter to fix 
        int fix_times = 0;

        // Scroll each single piece of block's container 
        for (int i = 0; i < block.transform.childCount; i++)
        {
            // Check if piece is over grid limit
            if (block.transform.GetChild(i).transform.position.x < GameManager.Instance.X_limits.x)
            {
                // Update counter
                fix_times += 1;
            }
        }

        // Check if at least one piece is over limit
        if (fix_times > 0)
        {
            // Reset block position inside grid
            Vector3 right = new Vector3(fix_times, 0, 0);
            block.transform.position += right;
        }
    }

    // Check if block is over grid limit on x [> 10]
    void CheckRotationLimitRight(GameObject block)
    {
        // Counter to fix
        int fix_times = 0;

        // Scroll each single piece of block's container 
        for (int i = 0; i < block.transform.childCount; i++)
        {
            // Check if piece is over grid limit
            if (block.transform.GetChild(i).transform.position.x > GameManager.Instance.X_limits.y)
            {
                // Update counter
                fix_times += 1;
            }
        }

        // Check if at least one piece is over limit
        if (fix_times > 0)
        {

            // Reset block position inside grid
            Vector3 left = new Vector3(fix_times, 0, 0);
            block.transform.position -= left;
        }
    }

    // Check if block is over grid limit on y [< 0]
    void CheckRotationLimitDown(GameObject block)
    {
        // Counter to fix
        int fix_times = 0;

        // Scroll each single piece of block's container 
        for (int i = 0; i < block.transform.childCount; i++)
        {
            // Check if piece is over grid limit
            if (block.transform.GetChild(i).transform.position.y < GameManager.Instance.Y_limits.y)
            {
                // Update counter
                fix_times += 1;
            }
        }

        // Check if at least one piece is over limit
        if (fix_times > 0)
        {
            // Reset block position inside grid
            Vector3 up = new Vector3(0, fix_times, 0);
            block.transform.position += up;
        }
    }

    // ##########################################################################################
    // UNDER CONSTRUCTION #######################################################################
    // ##########################################################################################
}
