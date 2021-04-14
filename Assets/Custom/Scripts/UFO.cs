using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFO : WarpObject
{
    public Bullet Bullet;
    public AudioSource BangSmall;
    public AudioSource UFOSound;
    public ParticleSystem Explosion;
    public bool IsHit { get; set; } = false;
    private DateTime _lastShot = DateTime.MinValue;

    // Start is called before the first frame update
    void Start()
    {
        var rigidBody = GetComponent<Rigidbody>();
        int xWall = Math.Sign(Game.Instance.GetRandomFloat(-1f, 1f));

        if (xWall == 1)
        {
            //start on an xwall, z random
            transform.position = new Vector3(Math.Sign(Game.Instance.GetRandomFloat(-1f, 1f)) * 1.45f, Game.Instance.GetRandomFloat(0.5f, 2f), Game.Instance.GetRandomFloat(-1.5f, 1.5f));
            rigidBody.AddForce(Vector3.Scale(transform.position, new Vector3(1,0,1)) * 0.15f, ForceMode.Impulse);
        }
        else
        {
            //start on an zwall, x random
            transform.position = new Vector3(Game.Instance.GetRandomFloat(-1.5f, 1.5f), Game.Instance.GetRandomFloat(0.5f, 2f), Math.Sign(Game.Instance.GetRandomFloat(-1f, 1f)) * 1.45f);
            rigidBody.AddForce(Vector3.Scale(transform.position, new Vector3(1, 0, 1)) * 0.15f, ForceMode.Impulse);
        }

        UFOSound.Play();
    }    

    // Update is called once per frame
    void FixedUpdate()
    {
        if (CheckObjectForWarp(transform, (Game.Instance.EnemiesLeft == 0)) && Game.Instance.EnemiesLeft == 0)
        {
            Game.Instance.IsUFOPresent = false;
            CheckForRestart();            
        }

        if ((DateTime.Now - _lastShot).TotalMilliseconds > 500)
        {
            _lastShot = DateTime.Now;
            Shoot();
        }
    }

    private void CheckForRestart()
    {
        if (Game.Instance.EnemiesLeft == 0)
        {
            Game.Instance.InitialiseEnemies(Game.Instance.EnemiesWave + 1);
        }
    }

    private void Shoot()
    {
        //pew pew
        //Fire.Play();
        var newBullet = Instantiate(Bullet);
        newBullet.Source = BulletSource.UFO;

        var bulletDirection = Game.Instance.GetRandomVector3() * 0.01f;

        newBullet.transform.position = transform.position + bulletDirection * 0.04f;
        newBullet.GetComponent<Rigidbody>().AddForce(bulletDirection, ForceMode.Impulse);
    }

    public void Explode(bool destroyedByBullet)
    {
        if (!IsHit)
        {
            StartCoroutine(WaitThenDestroy(destroyedByBullet));
        }
    }

    IEnumerator WaitThenDestroy(bool destroyedByBullet)
    {
        Game.Instance.UFODeathTime = DateTime.Now;
        Game.Instance.IsUFOPresent = false;
        UFOSound.Stop();
        BangSmall.Play();
        if (destroyedByBullet)
        {
            Game.Instance.HUD.IncrementScore(200);
        }
        Explosion.Play();        

        IsHit = true;
        GetComponent<MeshRenderer>().enabled = false;

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);

        CheckForRestart();
    }

    private void OnCollisionEnter(Collision collision)
    {        
        Asteroid asteroid;
        asteroid = collision.gameObject.GetComponent<Asteroid>();
        if (asteroid != null)
        {
            Explode(false);
            asteroid.Explode();
        }
        else if (collision.gameObject.GetComponent<Ship>() != null)
        {
            Game.Instance.Ship.Explode();
        }
    }
}
