using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Game : MonoBehaviour
{
    public AudioSource Beat1;
    public AudioSource Beat2;
    public Ship Ship;
    public HUD HUD;
    public Asteroid Asteroid1;
    public UFO UFO;
    public RenderType RenderType = RenderType.Classic;

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
        _nextBeat = Beat1;

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

                    foreach (Asteroid item in GameObject.FindObjectsOfType<Asteroid>())
                    {
                        item.SwitchRenderer(RenderType);
                    }

                    Asteroid1.SwitchRenderer(RenderType);

                    Ship.SwitchRenderer(RenderType);
                }
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
        return (float)EnemiesLeft / (float)EnemiesTotal + 0.1f;
    }

    private void BeatCheck()
    {
        if ((DateTime.Now - _lastBeatPlayed).TotalSeconds > BeatGap())
        {
            _lastBeatPlayed = DateTime.Now;
            _nextBeat.Play();

            if (_nextBeat == Beat1)
            {
                _nextBeat = Beat2;
            }
            else
            {
                _nextBeat = Beat1;
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

    public bool CheckObjectForWarp(Transform transform, bool blnDestroyIfOutside = false)
    {
        bool warped = false;
        if (transform.position.x > BoundMaxX)
        {
            warped = true;
            if (blnDestroyIfOutside)
            {
                Destroy(transform.gameObject);
            }
            else
            {
                transform.position = new Vector3(BoundMinX, transform.position.y, transform.position.z);
            }            
        }
        else if (transform.position.x < BoundMinX)
        {
            warped = true;
            if (blnDestroyIfOutside)
            {
                Destroy(transform.gameObject);
            }
            else
            {
                transform.position = new Vector3(BoundMaxX, transform.position.y, transform.position.z);
            }
        }

        if (transform.position.y > BoundMaxY)
        {
            warped = true;
            if (blnDestroyIfOutside)
            {
                Destroy(transform.gameObject);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, BoundMinY, transform.position.z);
            }
        }
        else if (transform.position.y < BoundMinY)
        {
            warped = true;
            if (blnDestroyIfOutside)
            {
                Destroy(transform.gameObject);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, BoundMaxY, transform.position.z);
            }
        }

        if (transform.position.z > BoundMaxZ)
        {
            warped = true;
            if (blnDestroyIfOutside)
            {
                Destroy(transform.gameObject);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, BoundMinZ);
            }
        }
        else if (transform.position.z < BoundMinZ)
        {
            warped = true;
            if (blnDestroyIfOutside)
            {
                Destroy(transform.gameObject);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, BoundMaxZ);
            }
        }

        return warped;
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