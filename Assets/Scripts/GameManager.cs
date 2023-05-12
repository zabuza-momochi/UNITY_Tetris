using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

// #####################
// #### TETRIS GAME ####
// #####################

public class GameManager : Singleton<GameManager>
{
    // ###############
    // Initialize vars
    // ###############

    // Block speed (decrease for increase difficulty)
    float time_speed = 0.6f;

    // Save last time set
    float time_prev;
    
    // Music pref
    public bool Music;

    // Arena limits
    public Vector2 X_limits;
    public Vector2 Y_limits;

    // Player's score
    int scorePoint;

    // Total clean rows
    int scoreRows;

    // Global Level
    int level;

    // UI Text
    public TMP_Text ScoreText;
    public TMP_Text LinesText;
    public TMP_Text LevelText;
    public RawImage Box;

    // Fade material
    public Material Fade_material;
    
    // Base material
    public Material Base_material;

    // Audio source component
    AudioSource mainSource;

    // Audio clips
    public AudioClip clip_mainTheme;
    public List<AudioClip> SFX_clips; 

    // Props
    public float current_time_speed { get => time_speed; }


    // Start is called before the first frame update
    void Start()
    {
        // Global Level
        level = 1;

        // Save current time speed
        time_prev = time_speed;

        // Get audio component
        mainSource = GetComponent<AudioSource>();

        // Set main music
        mainSource.clip = clip_mainTheme;

        // User setting
        MusicOnOff();
    }

    // Change speed for drop piece
    public void SetDropSpeed(Drop dropType)
    {
        // Save current time speed
        time_prev = time_speed;

        // CHeck drop type
        if (dropType == Drop.soft)
        {
            // Increase speed block
            time_speed *= 0.1f;
        }
        else
        {
            // Set block instantly
            time_speed = 0;
        }
    }

    // Reset speed after drop
    public void ResetSpeed()
    {
        // Reset speed from last set
        time_speed = time_prev;
    }

    // Update score
    public void UpdateScore(int rows = 1)
    {
        // Set point for cleared row
        scorePoint += 100 * rows;

        // Update clean rows
        scoreRows += rows;

        // Update level
        if(scorePoint >= 500)
        {
            // Increase level
            level++;

            // Increase blocks speed (need subtract, remember speed is time to move)
            time_speed -= level / 10;
        }

        // Update UI text
        ScoreText.text = scorePoint.ToString();
        LinesText.text = scoreRows.ToString();
        LevelText.text = level.ToString();
    }

    // Set Game Over
    public void GameOver()
    {
        // Stop music
        mainSource.Stop();

        // Show GAME OVER on UI
        Box.gameObject.SetActive(true);

        // Play clip
        mainSource.PlayOneShot(SFX_clips[(int)SFX.gameOver]);
    }

    // Play sound effect
    public void PlaySFX(SFX sound, float volume = 1f)
    {
        // Play clip one time
        mainSource.PlayOneShot(SFX_clips[(int)sound], volume);
    }

    // Set music pref
    void MusicOnOff()
    {
        if (!Music)
        {
            Music = true;
            mainSource.Stop();
        }
        else
        {
            Music = false;
            mainSource.Play();
        }
    }
}

// Enums
public enum Drop { normal, soft, hard }
public enum SFX { drop, hardDrop, rotate, fail, clear, gameOver }