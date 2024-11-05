using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour
{
    #region Singleton

    public static MissionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    #endregion

    private const string RedKeyMarkerTag = "RedKeyMarker";
    private const string BlueKeyMarkerTag = "BlueKeyMarker";
    private const string BlackKeyMarkerTag = "BlackKeyMarker";

    private const string RedGemMarkerTag = "RedGemMarker";
    private const string BlueGemMarkerTag = "BlueGemMarker";
    private const string BlackGemMarkerTag = "BlackGemMarker";

    private const string LevelExitTag = "LevelExit";
    private const string VictoryScreenTag = "VictoryScreen";

    private Image _redKeyMarker;
    private Image _blueKeyMarker;
    private Image _blackKeyMarker;

    private Image _redGemMarker;
    private Image _blueGemMarker;
    private Image _blackGemMarker;

    private EndPortal _levelExit;
    private VictoryScreen _victoryScreen;

    private bool _hasRedKey;
    public bool HasRedKey
    {
        get => _hasRedKey;
        private set
        {
            _hasRedKey = value;
            _redKeyMarker.enabled = value;
        }
    }

    private bool _hasBlueKey;
    public bool HasBlueKey
    {
        get => _hasBlueKey;
        private set
        {
            _hasBlueKey = value;
            _blueKeyMarker.enabled = value;
        }
    }

    private bool _hasBlackKey;
    public bool HasBlackKey
    {
        get => _hasBlackKey;
        private set
        {
            _hasBlackKey = value;
            _blackKeyMarker.enabled = value;
        }
    }

    private bool _hasRedGem;
    public bool HasRedGem
    {
        get => _hasRedGem;
        private set
        {
            _hasRedGem = value;
            _redGemMarker.enabled = value;
        }
    }

    private bool _hasBlueGem;
    public bool HasBlueGem
    {
        get => _hasBlueGem;
        private set
        {
            _hasBlueGem = value;
            _blueGemMarker.enabled = value;
        }
    }

    private bool _hasBlackGem;
    public bool HasBlackGem
    {
        get => _hasBlackGem;
        private set
        {
            _hasBlackGem = value;
            _blackGemMarker.enabled = value;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += InitLevel;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= InitLevel;
    }

    private void InitLevel(Scene scene, LoadSceneMode sceneMode)
    {
        if (!scene.name.StartsWith("Level")) return;

        _redKeyMarker = GameObject.FindGameObjectWithTag(RedKeyMarkerTag).GetComponent<Image>();
        _blueKeyMarker = GameObject.FindGameObjectWithTag(BlueKeyMarkerTag).GetComponent<Image>();
        _blackKeyMarker = GameObject.FindGameObjectWithTag(BlackKeyMarkerTag).GetComponent<Image>();

        _redGemMarker = GameObject.FindGameObjectWithTag(RedGemMarkerTag).GetComponent<Image>();
        _blueGemMarker = GameObject.FindGameObjectWithTag(BlueGemMarkerTag).GetComponent<Image>();
        _blackGemMarker = GameObject.FindGameObjectWithTag(BlackGemMarkerTag).GetComponent<Image>();

        _levelExit = GameObject.FindGameObjectWithTag(LevelExitTag).GetComponentInChildren<EndPortal>();

        if (scene.name == "Level4")
        {
            _victoryScreen = GameObject.FindGameObjectWithTag(VictoryScreenTag).GetComponentInChildren<VictoryScreen>();
        }

        HasRedKey = false;
        HasBlueKey = false;
        HasBlackKey = false;

        HasRedGem = false;
        HasBlueGem = false;
        HasBlackGem = false;

        _levelExit.SetActive(false);
    }

    public void OnKeyObtained(ArtifactType type)
    {
        switch (type)
        {
            case ArtifactType.Red:
                HasRedKey = true;
                break;
            case ArtifactType.Blue:
                HasBlueKey = true;
                break;
            case ArtifactType.Black:
                HasBlackKey = true;
                break;
        }
    }

    public void OnGemObtained(ArtifactType type)
    {
        switch (type)
        {
            case ArtifactType.Red:
                HasRedGem = true;
                break;
            case ArtifactType.Blue:
                HasBlueGem = true;
                break;
            case ArtifactType.Black:
                HasBlackGem = true;
                break;
        }

        if (HasRedGem && HasBlueGem && HasBlackGem)
        {
            _levelExit.SetActive(true);
        }
    }

    public void OnLevelCompleted()
    {
        var curLvlName = SceneManager.GetActiveScene().name;
        if (!int.TryParse(curLvlName[5..], out var curLvlNum)) return;
        if (curLvlNum == 4)
            _victoryScreen.Show();
        else
            SceneManager.LoadScene($"Level" + ++curLvlNum);
    }
}

public enum ArtifactType
{
    Red,
    Blue,
    Black,
}
