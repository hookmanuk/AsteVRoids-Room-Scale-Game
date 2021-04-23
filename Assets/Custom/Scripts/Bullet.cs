using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : WarpObject
{    
    public BulletSource Source { get; set; }

    // Start is called before the first frame update
    void Start()
    {     
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckObjectForWarp(transform, true);
    }

    private void OnTriggerEnter(Collider other)
    {
        Asteroid asteroid;
        UFO UFO = null;
        asteroid = other.gameObject.GetComponentInParent<Asteroid>();
        if (asteroid == null)
        {
            UFO = other.gameObject.GetComponentInParent<UFO>();
        }
        if (asteroid != null)
        {
            Destroy(this.gameObject);
            asteroid.Explode();
        }       
        else if (UFO != null && Source != BulletSource.UFO)
        {
            Destroy(this.gameObject);
            UFO.Explode(true);
        }
        else if (Game.Instance.IsScoreEntry && other.gameObject.CompareTag("ScoreLetter"))
        {
            if (other.name == "Del")
            {
                if (Game.Instance.HUD.Name.text.Length == 1)
                {
                    Game.Instance.HUD.Name.UpdateText("");
                }
                else if (Game.Instance.HUD.Name.text.Length > 1)
                {
                    Game.Instance.HUD.Name.UpdateText(Game.Instance.HUD.Name.text.Substring(0, Game.Instance.HUD.Name.text.Length - 1));
                }
            }
            else if (other.name == "OK")
            {
                Game.Instance.HUD.SaveScore();
            }
            else
            {
                Game.Instance.HUD.Name.UpdateText(Game.Instance.HUD.Name.text + other.name);
            }
        }

    }
}

public enum BulletSource
{
    Player,
    UFO
}