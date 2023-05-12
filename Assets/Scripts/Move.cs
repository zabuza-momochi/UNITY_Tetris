using UnityEngine;

public class Move : MonoBehaviour
{
    // ###############
    // Initialize vars
    // ###############

    // Array (used to contain the possible rotations of the blocks)
    public float[] angles;

    // Add offset to block position to match correctly with grid
    public Vector3 positionOffset;

    // Define block type
    public PieceType type;

    // Drop type
    Drop dropSpeed;

    // Angle used to rotate
    float angle;

    // Timer to control movement
    float timer;

    // Timer to wait to lock
    float waitTimer;

    // Check if block is fading
    bool isFading;

    // Check if block is moving
    public bool isMoving;

    // Check if block is blocked
    public bool isBlocked;

    // Start is called before the first frame update
    void Start()
    {
        // Set bools
        isMoving = false;
        isBlocked = false;

        // Set drop speed to normal
        dropSpeed = Drop.normal;

        // Reset speed
        GameManager.Instance.ResetSpeed();

        // Set timers
        timer = 0f;
        waitTimer = 1f;
    }

    // Init block on spawn
    public void InitBlock()
    {
        // Set drop speed to normal
        dropSpeed = Drop.normal;

        // Reset speed
        GameManager.Instance.ResetSpeed();

        // Set timers
        timer = 0f;
        waitTimer = 1f;

        // Add offset to block position
        transform.position += positionOffset;

        // Set bools
        isMoving = true;




        //// Get move component script of new block spawned
        //Move moveBlock = newBlock.GetComponent<Move>();

        //// Get a random rotation from angles list of new block spawned
        //Vector3 rotation = new Vector3(0, 0, moveBlock.angles[Random.Range(0, moveBlock.angles.Length)]);

        //// Set a random rotation to new block spawned
        //newBlock.transform.Rotate(rotation);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if block is moving
        if (isMoving)
        {
            // Decrease timer
            timer -= Time.deltaTime;

            // Check for moving
            MoveBlock();

            // Check for rotation
            RotateBlock();

            // Check for falling
            DownBlock();

            // Check for drop
            DropBlock();

            // Check for hold
            if (!isBlocked)
            {
                // Get user input
                if (Input.GetKeyUp(KeyCode.C))
                {
                    // Call Hold Block
                    SpawnMngr.Instance.HoldBlock();
                }
            }
        }
    }

    // Manage block fall
    void DownBlock()
    {
        // Check if timer is over
        if (timer <= 0f)
        {
            // Check if block reached y limit [y <= 0] [false -> limit reached]
            if (!CheckDown())
            {
                // Lock block
                isBlocked = true;

                // Deacrease wait timer
                WaitTime();

                // Exit
                return;
            }

            // Check if grid block is empty [false -> block can move down]
            if (!GridMngr.Instance.CheckRow(gameObject))
            {
                // Moves the block down by one unit
                transform.position += Vector3.down;

                // Reset timer
                timer = GameManager.Instance.current_time_speed;

                // Remove fade effect
                isFading = false;

                // Reset wait timer
                waitTimer = 2f;

                // Unlock block
                isBlocked = false;

                // Fade out
                PieceFadeOut();
            }
            // [true -> block can't move down]
            else
            {
                // Lock block
                isBlocked = true;

                // Decrease timer
                WaitTime();
            }
        }
    }

    // Manage wait state
    void WaitTime()
    {
        // Decrease timer
        waitTimer -= Time.deltaTime;

        // Check is already fading
        if (!isFading)
        {
            // Set fade in
            isFading = true;

            // Fade in
            PieceFadeIn();
        }

        // Check if drop is hard
        if (dropSpeed == Drop.hard)
        {
            // Ends the timer immediately
            waitTimer = 0f;
        }

        // Check if timer is over
        if (waitTimer <= 0f)
        {
            // Check if block reached y limit [y <= 0] [true -> limit not reached]
            if (CheckDown())
            {
                // Reset wait timer
                waitTimer = 2f;

                // Unlock block
                isBlocked = false;
            }

            // Check if grid block is empty [false -> block can move down]
            if (!GridMngr.Instance.CheckRow(gameObject))
            {
                // Reset wait timer
                waitTimer = 2f;

                // Unlock block
                isBlocked = false;
            }
            // [true -> block can't move down]
            else
            {
                // Lock block
                isBlocked = true;
            }

            // Check block state
            if (isBlocked)
            {
                // Check drop speed type
                if (dropSpeed == Drop.hard)
                {
                    // Play clip
                    GameManager.Instance.PlaySFX(SFX.hardDrop);
                }
                else
                {
                    // Play clip
                    GameManager.Instance.PlaySFX(SFX.drop, 5f);
                }

                // Set bools
                isMoving = false;
                isFading = false;

                // Fade out
                PieceFadeOut();

                // Store block inside grid
                GridMngr.Instance.SetRow(gameObject);
            }
        }
    }

    // Manage block rotation
    void RotateBlock()
    {
        // Check user input
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            // Save current transform
            Vector3 currentPosition = transform.position;
            Quaternion currentRotation = transform.rotation;

            // Check angles block
            if (angles.Length > 1)
            {
                // Check angles block [S,Z block type - only two rotation allowed]
                if (angles.Length == 2)
                {
                    // Set angle rotation
                    angle = transform.rotation.eulerAngles.z == 0 ? 90 : -90;

                    // Set vector rotation
                    Vector3 rotation = new Vector3(0, 0, angle);

                    // Add rotation to block
                    transform.Rotate(rotation);
                }
                // [I,J,L,T block type - four rotation allowed]
                else
                {
                    // Play clip
                    GameManager.Instance.PlaySFX(SFX.rotate, 2f);

                    // Set angle rotation
                    angle = -90;

                    // Set vector rotation
                    Vector3 rotation = new Vector3(0, 0, angle);

                    // Add rotation to block
                    transform.Rotate(rotation);
                }

                // Check if rotation is possible [false -> rotation failed]
                if (!GridMngr.Instance.CheckRotation(gameObject))
                {
                    // Restore previous position
                    transform.position = currentPosition;

                    // Restore previous rotation
                    transform.rotation = currentRotation;

                    // Play clip
                    GameManager.Instance.PlaySFX(SFX.fail);
                }
                // [true -> rotation success]
                else
                {
                    // Play clip
                    GameManager.Instance.PlaySFX(SFX.rotate, 2f);
                }
            }
            // [O block type - no rotation allowed]
            else
            {
                GameManager.Instance.PlaySFX(SFX.fail);
            }
        }
    }

    // Set block fade in
    void PieceFadeIn()
    {
        // Scroll each single piece of block's container
        for (int i = 0; i < transform.childCount; i++)
        {
            // Set material for each piece
            transform.GetChild(i).GetComponent<SpriteRenderer>().material = GameManager.Instance.Fade_material;
        }
    }

    // Set block fade out
    void PieceFadeOut()
    {
        // Scroll each single piece of block's container
        for (int i = 0; i < transform.childCount; i++)
        {
            // Set material for each piece
            transform.GetChild(i).GetComponent<SpriteRenderer>().material = GameManager.Instance.Base_material;
        }
    }

    // Manage block movement
    void MoveBlock()
    {
        // Check user input
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            // Check if block is inside grid and grid is empty
            if (CheckLeft() && !GridMngr.Instance.CheckSide(gameObject))
            {
                // Moves the block left by one unit
                transform.position -= Vector3.right;
            }
        }

        // Check user input
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            // Check if block is inside grid and grid is empty
            if (CheckRight() && !GridMngr.Instance.CheckSide(gameObject, "right"))
            {
                // Moves the block left by one unit
                transform.position += Vector3.right;
            }
        }
    }

    // Manage block drop
    void DropBlock()
    {
        // Check user input
        if (Input.GetKeyDown(KeyCode.DownArrow) && dropSpeed == Drop.normal)
        {
            // Set drop speed
            dropSpeed = Drop.soft;
            GameManager.Instance.SetDropSpeed(dropSpeed);

            // Exit
            return;
        }

        // Check user input
        if (Input.GetKeyUp(KeyCode.DownArrow) && dropSpeed == Drop.soft)
        {
            // Set drop speed
            dropSpeed = Drop.normal;
            GameManager.Instance.ResetSpeed();

            // Exit
            return;

        }

        // Check user input
        if (Input.GetKeyUp(KeyCode.Space) && dropSpeed == Drop.normal)
        {
            // Set drop speed
            dropSpeed = Drop.hard;
            GameManager.Instance.SetDropSpeed(dropSpeed);

            // Exit
            return;
        }
    }

    // Check left limit
    bool CheckLeft()
    {
        // Scroll each single piece of block's container
        for (int i = 0; i < transform.childCount; i++)
        {
            // Check if each piece is over left limit [x < 0]
            if (transform.GetChild(i).transform.position.x - 1 <= GameManager.Instance.X_limits.x)
            {
                // Play clip
                GameManager.Instance.PlaySFX(SFX.fail);

                // Exit - check failed
                return false;
            }
        }

        // Exit - check success
        return true;
    }

    // Check right limit
    bool CheckRight()
    {
        // Scroll each single piece of block's container
        for (int i = 0; i < transform.childCount; i++)
        {
            // Check if each piece is over left limit [x > 10]
            if (transform.GetChild(i).transform.position.x + 1 >= GameManager.Instance.X_limits.y)
            {
                // Play clip
                GameManager.Instance.PlaySFX(SFX.fail);

                // Exit - check failed
                return false;
            }
        }

        // Exit - check success
        return true;
    }

    // Check down limit
    bool CheckDown()
    {
        // Scroll each single piece of block's container
        for (int i = 0; i < transform.childCount; i++)
        {
            // Check if each piece is over left limit [y < 0]
            if (transform.GetChild(i).transform.position.y - 1 <= GameManager.Instance.Y_limits.y)
            {
                // Exit - check failed
                return false;
            }
        }

        // Exit - check success
        return true;
    }
}
