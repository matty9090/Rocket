using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    private Camera Cam = null;

    [SerializeField]
    private Transform Rocket = null;

    [SerializeField]
    private PowerBar PowerBar = null;

    [SerializeField]
    private Transform LaunchOrigin = null;

    [SerializeField]
    private AnimationCurve LaunchCurve = null;

    [SerializeField]
    private AnimationCurve PowerCurve = null;

    [SerializeField]
    private float AimSpeed = 0.5f;

    [SerializeField]
    private float PowerSpeed = 0.5f;

    private enum EState { Aim, Fly, Land };
    private EState CurrentState = EState.Aim;

    // Aim state
    private enum EAimSubState { Angle, Power };
    private EAimSubState AimState = EAimSubState.Angle;
    private float AimTimer = 0.0f;

    void Start()
    {
        
    }

    void Update()
    {
        switch (CurrentState)
        {
            case EState.Aim: TickAimState(); break;
            case EState.Fly: TickFlyState(); break;
            case EState.Land: TickLandState(); break;
        }
    }

    void TickAimState()
    {
        if (AimState == EAimSubState.Angle)
        {
            if (Input.GetMouseButtonDown(0))
            {
                AimTimer = 0.0f;
                AimState = EAimSubState.Power;
                Rocket.GetComponentInChildren<TrailRenderer>().enabled = false;
                return;
            }

            AimTimer += Time.deltaTime * AimSpeed;
            float rot = LaunchCurve.Evaluate(AimTimer) * AimSpeed / 0.9f;

            Rocket.RotateAround(LaunchOrigin.position, new Vector3(0.0f, 0.0f, 1.0f), rot);
        }
        else if(AimState == EAimSubState.Power)
        {
            AimTimer += Time.deltaTime * PowerSpeed;
            float power = PowerCurve.Evaluate(AimTimer);
            PowerBar.transform.localScale = new Vector3(1.0f, power, 1.0f);

            if (Input.GetMouseButtonDown(0))
            {
                AimTimer = 0.0f;
                AimState = EAimSubState.Angle;
                CurrentState = EState.Fly;
                Rocket.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                return;
            }
        }
    }

    void TickFlyState()
    {
        Cam.transform.position += Vector3.right * Time.deltaTime * 2.0f;
    }

    void TickLandState()
    {
        
    }
}
