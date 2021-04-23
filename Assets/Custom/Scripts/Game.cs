using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class Game : MonoBehaviour
{
    public AudioSource ClassicBeat1;
    public AudioSource ClassicBeat2;
    public AudioSource EnhancedBeat1;
    public AudioSource EnhancedBeat2;
    public Ship Ship;
    public HUD HUD;
    public Asteroid Asteroid1;
    public UFO UFO;
    public RenderType RenderType = RenderType.Classic;
    public Light[] Lights;    

    public int EnemiesWave { get; set; }
    public int EnemiesTotal { get; set; }
    public int EnemiesLeft { get; set; }
    public float RefreshRate { get; set; }
    public bool IsStarted { get; set; }
    public bool IsScoreEntry { get; set; }

    public bool IsUFOPresent { get; set; }
    public DateTime UFODeathTime { get; set; } = DateTime.MinValue;
    public float BoundMinX;
    public float BoundMaxX;
    public float BoundMinY;
    public float BoundMaxY;
    public float BoundMinZ;
    public float BoundMaxZ;
    public int Lives { get; set; } = 4;

    public dreamloLeaderBoard dl; //http://dreamlo.com/lb/olXohuxYZkG4akYywjEznARxcAmwowfkWMf3FqGdOGPw

    public bool DebugStart;

    private DateTime _lastBeatPlayed = DateTime.MinValue;
    private AudioSource _nextBeat;
    private DateTime _lastRenderSwitch = DateTime.MinValue;

    private static Game _instance;
    public static Game Instance
    {
        get
        {
            return _instance;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        _instance = this;

        this.dl = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
    }

    private void Start()
    {
        StartCoroutine(HUD.ShowScores());

        ApplyRenderType();

        if (DebugStart)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (!IsStarted)
        {
            IsStarted = true;           

            HUD.SetMainMenuVisible(false);           

            HUD.ResetScore();

            ResetLivesLeft();

            InitialiseEnemies(4);            

            Ship.Reset();            
        }
    }

    public void StopGame()
    {
        IsStarted = false;

        foreach (Asteroid item in GameObject.FindObjectsOfType<Asteroid>())
        {
            Destroy(item.ActiveMeshRenderer.gameObject);
        }

        Ship.Reset();

        HUD.SetScoreEntryVisible(true);
    }

    public void InitialiseEnemies(int totalEnemies)
    {
        EnemiesWave = totalEnemies;
        EnemiesTotal = CalculateEnemies(totalEnemies);
        EnemiesLeft = EnemiesTotal;
        _nextBeat = (RenderType == RenderType.Classic ? ClassicBeat1 : EnhancedBeat1);

        for (int i = 0; i < totalEnemies; i++)
        {
            Instantiate(Asteroid1);
        }
    }

    public void ReduceEnemiesLeft()
    {
        EnemiesLeft--;

        if (EnemiesLeft <= 0 && !IsUFOPresent)
        {
            InitialiseEnemies(EnemiesWave + 1);
        }        
    }

    private int CalculateEnemies(int startingAsteroids)
    {
        return startingAsteroids + (startingAsteroids * 2) + (startingAsteroids * 2 * 2);
    }
    
    void FixedUpdate()
    {
        RefreshRateCheck();

        RenderTypeCheck();

        if (IsStarted)
        {
            BeatCheck();

            UFOCheck();
        }
    }

    private void RenderTypeCheck()
    {
        InputDevice inputDevice;
        bool isSecondaryPressed;

        if ((DateTime.Now - _lastRenderSwitch).TotalMilliseconds > 200)
        {
            if (Ship.GetRightController(out inputDevice))
            {
                inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out isSecondaryPressed);

                if (isSecondaryPressed)
                {
                    _lastRenderSwitch = DateTime.Now;

                    if (RenderType == RenderType.Classic)
                    {
                        RenderType = RenderType.Enhanced;
                    }
                    else
                    {
                        RenderType = RenderType.Classic;
                    }

                    ApplyRenderType();
                }
            }
        }        
    }

    public void ApplyRenderType()
    {
        foreach (Asteroid item in GameObject.FindObjectsOfType<Asteroid>())
        {
            item.SwitchRenderer(RenderType);
        }        

        Ship.SwitchRenderer(RenderType);

        foreach (var light in Lights)
        {
            light.enabled = (RenderType == RenderType.Enhanced);
        }

        foreach (UFO item in GameObject.FindObjectsOfType<UFO>())
        {
            item.SwitchRenderer(RenderType);
        }

        if (RenderType == RenderType.Classic)
        {
            if (_nextBeat == EnhancedBeat1)
            {
                _nextBeat = ClassicBeat1;
            }
            else
            {
                _nextBeat = ClassicBeat2;
            }
        }
        else if (RenderType == RenderType.Enhanced)
        {
            if (_nextBeat == ClassicBeat1)
            {
                _nextBeat = EnhancedBeat1;
            }
            else
            {
                _nextBeat = EnhancedBeat2;
            }
        }
    }

    private void UFOCheck()
    {
        if ((float)EnemiesLeft / (float)EnemiesTotal < 0.2f)        
        {
            if (!IsUFOPresent && (DateTime.Now - UFODeathTime).TotalSeconds > 10)
            {
                SpawnUFO();
            }
        }

        //debug
        //if (!IsUFOPresent)
        //{
        //    SpawnUFO();
        //}
    }

    private void SpawnUFO()
    {
        Instantiate(UFO);
        IsUFOPresent = true;
    }

    private float BeatGap()
    {
        return (float)EnemiesLeft / (float)EnemiesTotal + 0.15f;
    }

    private void BeatCheck()
    {
        if ((DateTime.Now - _lastBeatPlayed).TotalSeconds > BeatGap())
        {
            _lastBeatPlayed = DateTime.Now;
            _nextBeat.Play();

            if (RenderType == RenderType.Classic)
            {
                if (_nextBeat == ClassicBeat1)
                {
                    _nextBeat = ClassicBeat2;
                }
                else
                {
                    _nextBeat = ClassicBeat1;
                }
            }
            else if (RenderType == RenderType.Enhanced)
            {
                if (_nextBeat == EnhancedBeat1)
                {
                    _nextBeat = EnhancedBeat2;
                }
                else
                {
                    _nextBeat = EnhancedBeat1;
                }
            }
        }
    }

    public Vector3 GetRandomVector3(float min = -180f, float max = 180f)
    {
        return new Vector3(GetRandomFloat(min,max), GetRandomFloat(min, max), GetRandomFloat(min, max));
    }

    public float GetRandomFloat(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    

    private void RefreshRateCheck()
    {
        if (XRDevice.refreshRate != RefreshRate)
        {
            RefreshRate = XRDevice.refreshRate;
            Time.fixedDeltaTime = (float)Math.Round(1 / XRDevice.refreshRate, 8);
        }
    }

    public void ReduceLivesLeft()
    {
        Lives--;
        HUD.ReduceLives();

        if (Lives < 0)
        {
            StopGame();
        }
    }

    public void ResetLivesLeft()
    {
        Lives = 3;
        HUD.ResetLives();        
    }
}

public enum RenderType
{
    Classic,
    Enhanced
}