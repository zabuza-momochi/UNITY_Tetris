using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnMngr : Singleton<SpawnMngr>
{
    // ###############
    // Initialize vars
    // ###############

    // Current block spawn point list
    public List<Transform> SpawnPoints;

    // Next block spawn point list
    public List<Transform> BaseSpawnPoint;

    // Hold block spawn point
    public Transform HoldSpawnPoint;

    // Block container
    List<GameObject> blocksContainer = new List<GameObject>();

    // Block type list
    public List<GameObject> Blocks;

    // Colort type list
    public List<Color> colors;

    // Check to spawn next block
    public bool canSpawn;

    // Check to spawn next block
    bool canHold;

    // Hold block
    GameObject holdBlock;

    // Current block
    GameObject currentBlock;

    // Time to spawn
    float timeSpawn = 0.1f;

    // Timer for spawner
    float timerToSpawn;

    // Default spawn position on hold
    Vector3 centerPosition = new Vector3(5, 18, 0);

    // Start is called before the first frame update
    void Start()
    {
        // Assign time to timer
        timerToSpawn = timeSpawn;

        // Generate next three blocks
        for (int i = 0; i < BaseSpawnPoint.Count; i++)
        {
            // Generate next block
            blocksContainer.Add(GenerateBlock(BaseSpawnPoint[i]));
        }

        // Set bool
        canSpawn = true;
        canHold = true;

        holdBlock = null;

    }

    // Update is called once per frame
    void Update()
    {
        // Check to spawn next block
        if (canSpawn)
        {
            // Decrease timer
            timerToSpawn -= Time.deltaTime;

            // Check timer is over
            if (timerToSpawn <= 0)
            {
                // Reset timer
                timerToSpawn = timeSpawn;

                // Reset bool
                canSpawn = false;

                // Check if hold is used
                if (!canHold)
                {
                    // Reset bool
                    canHold = true;
                }

                // Call spawner
                SpawnBlock();
            }
        }
    }

    // Spawn new block
    public void SpawnBlock()
    {
        // Get current block from queue
        currentBlock = blocksContainer[0];

        // Scroll blocks list
        for (int i = 0; i < blocksContainer.Count - 1; i++)
        {
            // Shift index array
            blocksContainer[i] = blocksContainer[i + 1];

            // Shift block position
            blocksContainer[i].transform.position = BaseSpawnPoint[i].position;
        }

        // Get spawn position from list
        Transform spawnPosition = SpawnPoints[Random.Range(0, SpawnPoints.Count)];

        // Set position to block
        currentBlock.transform.position = new Vector3(spawnPosition.position.x, spawnPosition.position.y, 0);

        // Spawn current block
        currentBlock.GetComponent<Move>().InitBlock();

        // Generate next block
        blocksContainer[blocksContainer.Count - 1] = GenerateBlock(BaseSpawnPoint[BaseSpawnPoint.Count - 1]);
    }

    GameObject GenerateBlock(Transform spawnPosition)
    {
        // Set random index to generate block type
        int indexBlock = Random.Range(0, (int)PieceType.LAST);

        // Get color from list
        Color pieceColor = colors[Random.Range(0, colors.Count)];

        // Instantiate new block
        GameObject newBlock = Instantiate(Blocks[indexBlock], spawnPosition.position, Blocks[indexBlock].transform.rotation);

        // Get all sprite renderer 
        SpriteRenderer[] spriteRenderers = newBlock.GetComponentsInChildren<SpriteRenderer>();

        // Scroll sprite's array
        for (int i = 0; i < newBlock.transform.childCount; i++)
        {
            // Set color to each piece of block
            spriteRenderers[i].color = pieceColor;
        }

        return newBlock;
    }

    // Hold current block and spawn next
    public void HoldBlock()
    {
        // Check if hold is empty
        if (canHold)
        {
            // Check if already holded
            if (holdBlock == null)
            {
                // Exchange blocks
                holdBlock = currentBlock;

                SpawnBlock();
            }
            // Exchange blocks
            else
            {
                // Exchange blocks 
                GameObject swap = currentBlock;
                currentBlock = holdBlock;
                holdBlock = swap;

                // Set new current block position to center
                currentBlock.transform.position = centerPosition;

                // Spawn new current block
                currentBlock.GetComponent<Move>().InitBlock();
            }

            // Play clip
            GameManager.Instance.PlaySFX(SFX.rotate);

            // Lock new hold block
            holdBlock.GetComponent<Move>().isMoving = false;

            // Display in UI
            holdBlock.transform.position = new Vector3(HoldSpawnPoint.position.x, HoldSpawnPoint.transform.position.y, 0);

            // Set bool
            canHold = false;
        }
    }
}

// Enums
public enum PieceType
{
    I, J, L, O, S, T, Z, LAST
}