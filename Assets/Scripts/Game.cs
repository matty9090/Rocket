using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    private Camera Cam = null;

    [SerializeField]
    private GameOver GameOverPanel = null;

    [SerializeField]
    private Transform RocketPrefab = null;

    [SerializeField]
    private Vector3 CamStartPos = Vector3.zero;

    [SerializeField]
    private Target Target = null;

    [SerializeField]
    private Sprite RocketTipMask = null;

    [SerializeField]
    private Sprite RocketBodyMask = null;

    [SerializeField]
    private PowerBar PowerBar = null;

    [SerializeField]
    private Transform RocketStart = null;

    [SerializeField]
    private AnimationCurve LaunchCurve = null;

    [SerializeField]
    private AnimationCurve PowerCurve = null;

    [SerializeField]
    Transform PointsTxt = null;

    [SerializeField]
    private float AimSpeed = 0.5f;

    [SerializeField]
    private float PowerSpeed = 0.5f;

    [SerializeField]
    private int NumRounds = 5;

    [SerializeField]
    private TMPro.TextMeshProUGUI TxtScore = null;

    [SerializeField]
    private TMPro.TextMeshProUGUI TxtRound = null;

    private enum EState { Aim, Fly, Land, GameOver };
    private EState CurrentState = EState.Aim;

    // Global state
    private Transform Rocket = null;
    private int Round = 1;
    private int Score = 0;

    // Aim state
    private enum EAimSubState { Angle, Power };
    private EAimSubState AimState = EAimSubState.Angle;
    private float AimTimer = 0.0f;

    // Fly state
    bool HasSeparated = false;
    Transform RocketClone = null;

    // Land state
    float ResetTimer = 0.0f;

    void Start()
    {
        ResetRocket();
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

            Rocket.RotateAround(Rocket.Find("LaunchOrigin").position, new Vector3(0.0f, 0.0f, 1.0f), rot);
        }
        else if (AimState == EAimSubState.Power)
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
                Rocket.GetComponent<Animator>().SetTrigger("Flame");
                return;
            }
        }
    }

    void TickFlyState()
    {
        Rocket.GetComponent<Animator>().SetFloat("SpinSpeed", Rocket.GetComponent<Rigidbody2D>().velocity.magnitude / 20.0f);
        
        if (RocketClone != null)
        {
            RocketClone.GetComponent<Animator>().SetFloat("SpinSpeed", RocketClone.GetComponent<Rigidbody2D>().velocity.magnitude / 20.0f);
        }

        if (Rocket.transform.position.x > 0.0f && Rocket.transform.position.x > Cam.transform.position.x)
        {
            Cam.transform.position = new Vector3(Rocket.transform.position.x, Cam.transform.position.y, Cam.transform.position.z);
        }

        if (Rocket.transform.position.y > CamStartPos.y)
        {
            Cam.transform.position = new Vector3(Cam.transform.position.x, Rocket.transform.position.y, Cam.transform.position.z);
        }

        if (!HasSeparated && Rocket.GetComponent<Rigidbody2D>().velocity.y < 0.0f)
        {
            RocketClone = Instantiate(Rocket);
            RocketClone.GetComponent<Rigidbody2D>().velocity = Rocket.GetComponent<Rigidbody2D>().velocity * 0.8f;
            RocketClone.GetComponent<SpriteMask>().sprite = RocketBodyMask;
            RocketClone.GetComponent<Animator>().SetTrigger("Spin");
            RocketClone.GetComponent<Rocket>().TipCollider.enabled = false;
            Rocket.GetComponent<SpriteMask>().sprite = RocketTipMask;
            Rocket.GetComponent<Rocket>().BodyCollider.enabled = false;
            Rocket.GetComponent<Animator>().SetTrigger("FlameOut");
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

            Score += points;
            HasSeparated = false;
            CurrentState = EState.Land;

            TxtScore.text = "Score: " + Score;

            ++Round;

            if (Round > NumRounds)
            {
                CurrentState = EState.GameOver;
                GameOver();
            }
            else
            {
                TxtRound.text = "Round " + Round + "/" + NumRounds;
            }
        }
    }

    void TickLandState()
    {
        ResetTimer += Time.deltaTime;

        if (ResetTimer > 2.0f)
        {
            ResetTimer = 0.0f;
            CurrentState = EState.Aim;
            ResetRocket();
        }
    }

    void ResetRocket()
    {
        PointsTxt.gameObject.SetActive(false);
        
        if (Rocket != null)
        {
            Destroy(Rocket.gameObject);
        }
        
        Rocket = Instantiate(RocketPrefab);
        Rocket.transform.position = RocketStart.position;
        Rocket.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        
        Cam.transform.position = CamStartPos;
        Target.transform.position = new Vector3(Random.Range(0.0f, 9.5f), Target.transform.position.y, Target.transform.position.z);
    }

    public void PlayAgain()
    {
        Round = 1;
        Score = 0;

        TxtRound.text = "Round " + Round + "/" + NumRounds;
        TxtScore.text = "Score: " + Score;

        GameOverPanel.gameObject.SetActive(false);
        CurrentState = EState.Aim;
    }

    void GameOver()
    {
        ResetRocket();
        GameOverPanel.gameObject.SetActive(true);

        bool hasKey = PlayerPrefs.HasKey("Highscore");

        if (!hasKey || (hasKey && PlayerPrefs.GetInt("Highscore") < Score))
        {
            PlayerPrefs.SetInt("Highscore", Score);
            PlayerPrefs.Save();
            GameOverPanel.ShowHighscore(Score);
        }
        else
        {
            GameOverPanel.ShowNoHighscore(Score, PlayerPrefs.GetInt("Highscore"));
        }
    }
}
