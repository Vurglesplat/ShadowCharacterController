using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


// a lot of this was made via help utilized from this Packt article - https://hub.packtpub.com/creating-simple-gamemanager-using-unity3d/
// as well as this Unity Forum thread - https://forum.unity.com/threads/help-how-do-you-set-up-a-gamemanager.131170/

// to keep track of where in the game we are
public enum GameState { INTRO_VIDEO, MAIN_MENU, OPTIONS_MENU, GAMEPLAY }

public enum LevelSceneValues
{
    ERROR_INVALID_LEVEL = -1,
    MAIN_MENU,
    GAME_OVER_SCREEN,
    TUTORIAL,
    FIRST_LEVEL,
    SECOND_LEVEL,
    VICTORY_SCREEN,
    CREDITS_SCREEN,
}

public delegate void OnStateChangeHandler();

public class GameManager
{
    // this prevents us from making a new instance, we only need one
    private protected GameManager()
    {
        
    }
    int previousSceneNumber = -1;

    public void InitializeGameManager()
    {
        currentGameState = GameState.MAIN_MENU;
        currentSceneNumber = 0;
    }
    private static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (GameManager.instance == null)
            {
                //DontDestroyOnLoad(GameManager.instance);
                GameManager.instance = new GameManager();
                instance.InitializeGameManager();
                Debug.Log("GameManager initialized later than normal (probably testing a scene), may cause some errors");
            }
            return GameManager.instance;
        }
    }
    // makes  it readOnly outside of the class
    public GameState currentGameState { get; private set; }
    public int currentSceneNumber { get; private set; }

    
    // returning the instance from whence it came
    public void OnApplicationQuit()
    {
        GameManager.instance = null;
    }

    public void LoadLevel(LevelSceneValues sceneToLoad)
    {
        if (sceneToLoad == LevelSceneValues.ERROR_INVALID_LEVEL)
            Debug.LogError("NO LEVEL GIVEN FOR BUTTON TO LOAD TO");
        else if (sceneToLoad < 0 || (int)sceneToLoad >= SceneManager.sceneCountInBuildSettings)
            Debug.LogError("LEVEL TO LOAD WAS OUT OF RANGE");
        else
        {
            Debug.Log("Switching to a new level now");

            previousSceneNumber = currentSceneNumber;
            currentSceneNumber = (int)sceneToLoad;

            if (PlayerPrefs.HasKey("FarthestUnlockedLevel"))
            {
                if (PlayerPrefs.GetInt("FarthestUnlockedLevel") < currentSceneNumber)
                    PlayerPrefs.SetInt("FarthestUnlockedLevel", currentSceneNumber);
            }
            else
                PlayerPrefs.SetInt("FarthestUnlockedLevel", currentSceneNumber);


            SceneManager.LoadScene((int)sceneToLoad);
        }
    }
    public void LoadNextLevel()
    {
        if ( currentSceneNumber+1 < SceneManager.sceneCountInBuildSettings )
        {
            previousSceneNumber = currentSceneNumber;
            LoadLevel((LevelSceneValues)(++currentSceneNumber));
        }
    }

    public int GetPreviousLevel()
    {
        return previousSceneNumber;
    }

    public void LoadPreviousLevel()
    {
        if (previousSceneNumber == -1)
            Debug.LogError("TRIED TO LOAD PREVIOUS SCENE WITHOUT A PREVIOUS SCENE");

        SceneManager.LoadScene(previousSceneNumber);
        int tempNumForSwap = currentSceneNumber;
        currentSceneNumber = previousSceneNumber;
        previousSceneNumber = tempNumForSwap;
    }

    public void SetGameState(GameState state)
    {
        this.currentGameState = state;
        OnStateChange();
    }
    public event OnStateChangeHandler OnStateChange;


    public void ResumeGameFromLastLevel()
    {
        if (PlayerPrefs.HasKey("RetryLevel"))
        {
            LoadLevel((LevelSceneValues)PlayerPrefs.GetInt("RetryLevel"));
        }
        else
            Debug.LogError("NO LEVEL SAVED TO RETRY FROM");
    }
}
