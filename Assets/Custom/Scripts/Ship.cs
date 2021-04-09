using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Ship : MonoBehaviour
{
    public Bullet Bullet;
    public AudioSource Fire;
    public AudioSource Thrust;
    public AudioSource Death;    
    public GameObject Thrusters;
    public ParticleSystem Explosion;    
    private InputDevice _rightController = default(InputDevice);
    private DateTime _lastShot = DateTime.MinValue;
    private bool IsHit = false;

    public Rigidbody Rigidbody { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SetShipRotation();

        ShootCheck();        

        if (!Game.Instance.IsScoreEntry && IsThrusting())
        {
            Thrusters.SetActive(true);
            if (!Thrust.isPlaying)
            {
                Thrust.Play();                
            }
            Rigidbody.AddForce(transform.forward * 0.5f, ForceMode.Acceleration);
        }
        else
        {
            Thrusters.SetActive(false);
        }

        Game.Instance.CheckObjectForWarp(transform);
    }

    private bool GetRightController(out InputDevice rightController)
    {
        if (!_rightController.isValid)
        {
            var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);

            if (rightHandDevices.Count > 0)
            {
                _rightController = rightHandDevices[0];
            }
        }

        rightController = _rightController;

        return (rightController.isValid);
    }

    private bool IsThrusting()
    {
        InputDevice inputDevice;
        float thrustAmount = 0f;
        bool isThrusting = false;
        bool isGripped = false;

        if (!IsHit)
        {
            if (GetRightController(out inputDevice))
            {
                inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out thrustAmount);
                inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out isGripped);
            }            

            if (thrustAmount >= 1f || thrustAmount == 0 && isGripped)
            {
                isThrusting = true;
            }
        }

        return isThrusting;
    }

    private void SetShipRotation()
    {
        InputDevice inputDevice;
        Quaternion rotation;

        if (GetRightController(out inputDevice))
        {
            inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation);

            transform.rotation = rotation;
        }
    }

    private void ShootCheck()
    {
        if (WillShoot())
        {
            Shoot();
        }
    }

    private bool WillShoot()
    {
        InputDevice inputDevice;
        bool isTriggerDown = false;

        if ((DateTime.Now - _lastShot).TotalMilliseconds > 200)
        {
            if (GetRightController(out inputDevice))
            {
                inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out isTriggerDown);

                if (isTriggerDown)
                {
                    _lastShot = DateTime.Now;
                }
            }            
        }

        return isTriggerDown;
    }

    private void Shoot()
    {
        //pew pew
        Fire.Play();
        var newBullet = Instantiate(Bullet);

        newBullet.transform.position = transform.position + transform.forward * 0.055f;
        newBullet.GetComponent<Rigidbody>().AddForce(transform.forward, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {        
        Asteroid asteroid;
        asteroid = other.gameObject.GetComponent<Asteroid>();
        if (asteroid != null)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (!IsHit)
        {
            StartCoroutine(WaitThenDestroy());
        }
    }

    IEnumerator WaitThenDestroy()
    {        
        Explosion.Play();
        Death.Play();

        IsHit = true;
        GetComponent<MeshRenderer>().enabled = false;        

        Game.Instance.ReduceLivesLeft();

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(0.5f);        

        Reset();
    }

    public void Reset()
    {
        GetComponent<MeshRenderer>().enabled = true;
        
        Thrusters.SetActive(true);
        transform.position = Vector3.zero + Vector3.up;
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;

        IsHit = false;
    }

    public void SetForScoreEntry()
    {
        transform.position = Vector3.zero + Vector3.up + Vector3.left * 2.5f;
    }
}
