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

    public int EnemiesTotal { get; set; }
    public int EnemiesLeft { get; set; }
    public float RefreshRate { get; set; }
    public float BoundMinX;
    public float BoundMaxX;
    public float BoundMinY;
    public float BoundMaxY;
    public float BoundMinZ;
    public float BoundMaxZ;
    public int Lives { get; set; } = 4;

    private DateTime _lastBeatPlayed = DateTime.MinValue;
    private AudioSource _nextBeat;

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
        EnemiesTotal = CalculateEnemies(4);
        EnemiesLeft = EnemiesTotal;
        _nextBeat = Beat1;
    }

    private int CalculateEnemies(int startingAsteroids)
    {
        return startingAsteroids + (startingAsteroids * 2) + (startingAsteroids * 2 * 2);
    }
    
    void FixedUpdate()
    {
        RefreshRateCheck();
        BeatCheck();
    }

    private float BeatGap()
    {
        return (float)EnemiesLeft / (float)EnemiesTotal + 0.1f;
    }

    private void BeatCheck()
    {
        if (EnemiesLeft > 0 && (DateTime.Now - _lastBeatPlayed).TotalSeconds > BeatGap())
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

    public Vector3 GetRandomVector3(float min, float max)
    {
        return new Vector3(GetRandomFloat(min,max), GetRandomFloat(min, max), GetRandomFloat(min, max));
    }

    public float GetRandomFloat(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public void CheckObjectForWarp(Transform transform, bool blnDestroyIfOutside = false)
    {
        if (transform.position.x > BoundMaxX)
        {
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
            if (blnDestroyIfOutside)
            {
                Destroy(transform.gameObject);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, BoundMaxZ);
            }
        }
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
    }
}
