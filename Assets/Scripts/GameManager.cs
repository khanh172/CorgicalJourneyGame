using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("")]
    public GameObject winParticlePrefab;

    [Header("")]
    public GameObject canvasHome;
    public GameObject canvasSelectLevel;
    public GameObject canvasHowToPlay;
    public GameObject canvasWin;

    [Header("")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public Button nextLevelButton;
    public Button winHomeButton;
    public Button winReplayButton;

    [Header("")]
    public GameObject[] levelPrefabs;

    [Header("")]
    public float levelTime = 100f;
    private float timeLeft;
    private bool isPlaying = false;

    private GameObject currentLevel;
    private int currentLevelIndex = 0;
    private int highScore = 0;
    private bool hasWon = false;

    private AudioSource musicSource;

    [Header("")]
    public AudioClip musicMenu;
    public AudioClip musicInGame;
    public AudioClip soundPlay;
    public AudioClip soundLevelStart;
    public AudioClip soundMove;
    public AudioClip soundWin; 
    public AudioClip soundWinCanvas;
    public AudioClip soundStickBlocked;
    public AudioClip soundButtonClick;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(transform);
        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    void Start()
    {
        PlayMusic(musicMenu);
        ShowHome();
    }

    void Update()
    {
        if (isPlaying && !hasWon)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
            {
                timeLeft = 0;
                EndLevel(false);
            }
        }
    }

    public void ShowHome()
    {
       

        canvasHome.SetActive(true);
        canvasSelectLevel.SetActive(false);
        canvasHowToPlay.SetActive(false);
        canvasWin.SetActive(false);

        PlayMusic(musicMenu);
        ResetGameState();
        UnloadCurrentLevel();
    }

    public void NewGame()
    {
        PlaySound(soundPlay);
        LoadLevel(0);
    }

    public void Continue()
    {
        PlaySound(soundButtonClick);
        canvasHome.SetActive(false);
        canvasSelectLevel.SetActive(true);
    }

    public void ShowHowToPlay()
    {
        PlaySound(soundButtonClick);
        canvasHome.SetActive(false);
        canvasHowToPlay.SetActive(true);
    }

    public void BackToHomeFromHowToPlay()
    {
        PlaySound(soundButtonClick);
        ShowHome();

    }

    public void BackToHomeFromSelectLevel()
    {
        PlaySound(soundButtonClick);
        ShowHome();
    }


    public void LoadLevel(int levelIndex)
    {
        UnloadCurrentLevel();

        if (levelIndex >= 0 && levelIndex < levelPrefabs.Length)
        {
            currentLevelIndex = levelIndex;
            currentLevel = Instantiate(levelPrefabs[levelIndex]);

            canvasHome.SetActive(false);
            canvasSelectLevel.SetActive(false);
            canvasHowToPlay.SetActive(false);
            canvasWin.SetActive(false);

            PlayMusic(musicInGame);
            PlaySound(soundLevelStart);

            SetupLevelUIButtons();
            ResetGameState();
            StartLevel();
        }

    }

    private void StartLevel()
    {
        timeLeft = levelTime;
        isPlaying = true;
        hasWon = false;
    }

    private void EndLevel(bool win)
    {
        isPlaying = false;

        if (win)
        {
            WinGame();
        }
        else
        {
            ShowHome();
        }
    }

    public void WinGame()
    {
        if (hasWon || timeLeft <= 0f) return;

        hasWon = true;
        isPlaying = false;

        PlaySound(soundWinCanvas);

        int score = GetCurrentScore();
        if (score > highScore) highScore = score;

        scoreText.text = "Score: " + score;
        highScoreText.text = "HScore: " + highScore;

        canvasWin.SetActive(true);
        canvasWin.transform.localPosition = new Vector3(0, 800, 0);
        canvasWin.transform.DOLocalMoveY(0, 0.8f).SetEase(Ease.OutBounce);

        nextLevelButton.onClick.RemoveAllListeners();
        nextLevelButton.onClick.AddListener(() =>
        {
            PlaySound(soundButtonClick);
            LoadLevel(currentLevelIndex + 1);
        });

        winHomeButton.onClick.RemoveAllListeners();
        winHomeButton.onClick.AddListener(() =>
        {
            PlaySound(soundButtonClick);
            ShowHome();
        });

        winReplayButton.onClick.RemoveAllListeners();
        winReplayButton.onClick.AddListener(() =>
        {
            PlaySound(soundButtonClick);
            LoadLevel(currentLevelIndex);
        });
    }

    public int GetCurrentScore()
    {
        float percent = timeLeft / levelTime;
        int score = Mathf.RoundToInt(percent * 100);
        return Mathf.Clamp(score, 0, 100);
    }

    private void SetupLevelUIButtons()
    {
        if (currentLevel == null) return;

        Button[] buttons = currentLevel.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            if (btn.CompareTag("ButtonHome"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    PlaySound(soundButtonClick);
                    ShowHome();
                });
            }
            else if (btn.CompareTag("ButtonReplay"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    PlaySound(soundButtonClick);
                    LoadLevel(currentLevelIndex);
                });
            }
        }
    }

    private void UnloadCurrentLevel()
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel);
            currentLevel = null;
        }
        GameObject[] sticks = GameObject.FindGameObjectsWithTag("Stick");
        foreach (GameObject stick in sticks)
        {
            Destroy(stick);
        }
    }


    private void ResetGameState()
    {
        isPlaying = false;
        hasWon = false;
        timeLeft = levelTime;

        scoreText.text = "Score: 0";
        highScoreText.text = "HScore: " + highScore;
    }

    public void SpawnWinEffect(Vector3 position)
    {
        if (winParticlePrefab != null)
        {
            Instantiate(winParticlePrefab, position, Quaternion.identity);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }

    public void PlayMusic(AudioClip music)
    {
        if (music == null || musicSource == null) return;

        if (musicSource.clip == music && musicSource.isPlaying) return;

        musicSource.Stop();
        musicSource.clip = music;
        musicSource.volume = 0.5f;
        musicSource.Play();
    }
    public void PlaySoundForSeconds(AudioClip clip, float duration)
    {
        if (clip == null) return;
        StartCoroutine(PlayAndStopRoutine(clip, duration));
    }

    private IEnumerator PlayAndStopRoutine(AudioClip clip, float duration)
    {
        GameObject temp = new GameObject("TempAudio");
        temp.transform.position = Camera.main.transform.position;

        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = clip;
        source.Play();

        yield return new WaitForSeconds(duration);

        source.Stop();
        Destroy(temp);
    }

 

}
