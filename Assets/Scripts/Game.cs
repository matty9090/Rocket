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
    private Transform LaunchOrigin = null;

    [SerializeField]
    private AnimationCurve LaunchCurve = null;

    private enum EState { Aim, Fly, Land };
    private EState CurrentState = EState.Aim;

    // Aim state
    private float AimTimer = 0.0f;

    void Start()
    {
        
    }

    void Update()
    {
        switch (CurrentState)
        {
            case EState.Aim: AimState(); break;
            case EState.Fly: FlyState(); break;
            case EState.Land: LandState(); break;
        }
    }

    void AimState()
    {
        AimTimer += Time.deltaTime * 0.5f;
        float rot = LaunchCurve.Evaluate(AimTimer) * 0.4f;
        Debug.Log(rot);

        Rocket.RotateAround(LaunchOrigin.position, new Vector3(0.0f, 0.0f, 1.0f), rot);
    }

    void FlyState()
    {
        Cam.transform.position += Vector3.right * Time.deltaTime * 2.0f;
    }

    void LandState()
    {
        
    }
}
