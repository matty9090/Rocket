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
    private Target Target = null;

    [SerializeField]
    private Sprite RocketTipMask = null;

    [SerializeField]
    private Sprite RocketBodyMask = null;

    [SerializeField]
    private PowerBar PowerBar = null;

    [SerializeField]
    private Transform LaunchOrigin = null;

    [SerializeField]
    private AnimationCurve LaunchCurve = null;

    [SerializeField]
    private AnimationCurve PowerCurve = null;

    [SerializeField]
    Transform PointsTxt;

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

    // Fly state
    bool HasSeparated = false;

    void Start()
    {
        PointsTxt.gameObject.SetActive(false);
        Rocket.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
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
            float rot = LaunchCurve.Evaluate(AimTimer) * 160.0f * Time.deltaTime;

            Rocket.RotateAround(LaunchOrigin.position, new Vector3(0.0f, 0.0f, 1.0f), rot);
        }
        else if(AimState == EAimSubState.Power)
        {
            AimTimer += Time.deltaTime * PowerSpeed;
            float power = PowerCurve.Evaluate(AimTimer);
            PowerBar.transform.localScale = new Vector3(1.0f, power, 1.0f);

            if (Input.GetMouseButtonDown(0))
            {
                var rot = Rocket.transform.rotation.eulerAngles * Mathf.Deg2Rad;
                Vector2 vel = new Vector2(Mathf.Cos(rot.z), Mathf.Sin(rot.z));
                vel *= power * 16;

                AimTimer = 0.0f;
                AimState = EAimSubState.Angle;
                CurrentState = EState.Fly;
                Rocket.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                Rocket.GetComponent<Rigidbody2D>().velocity = vel;
                Rocket.GetComponent<Animator>().SetTrigger("Spin");
                return;
            }
        }
    }

    void TickFlyState()
    {
        if (Rocket.transform.position.x > 0.0f)
        {
            Cam.transform.position = new Vector3(Rocket.transform.position.x, Cam.transform.position.y, Cam.transform.position.z);
            Rocket.GetComponent<Animator>().SetFloat("SpinSpeed", Rocket.GetComponent<Rigidbody2D>().velocity.magnitude / 20.0f);
        }

        if (!HasSeparated && Rocket.GetComponent<Rigidbody2D>().velocity.y < 0.0f)
        {
            var clone = Instantiate(Rocket);
            clone.GetComponent<Rigidbody2D>().velocity = Rocket.GetComponent<Rigidbody2D>().velocity * 0.8f;
            clone.GetComponent<SpriteMask>().sprite = RocketBodyMask;
            clone.GetComponent<Rocket>().TipCollider.enabled = false;
            Rocket.GetComponent<SpriteMask>().sprite = RocketTipMask;
            Rocket.GetComponent<Rocket>().BodyCollider.enabled = false;
            HasSeparated = true;
        }

        if (Rocket.GetComponent<Rigidbody2D>().velocity.sqrMagnitude < 0.1f)
        {
            int points = 0;
            var rocketCollider = Rocket.GetComponent<Rocket>().TipCollider;

            foreach (var collider in Target.Colliders)
            {
                if (collider.Collider.IsTouching(rocketCollider) && collider.Points > points)
                {
                    points = collider.Points;
                }
            }

            PointsTxt.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = points + " points";
            PointsTxt.gameObject.SetActive(true);
            PointsTxt.position = Cam.WorldToScreenPoint(Rocket.transform.position);
            PointsTxt.GetComponentInChildren<Animator>().SetTrigger("Drift");

            CurrentState = EState.Land;
        }
    }

    void TickLandState()
    {
        
    }
}
