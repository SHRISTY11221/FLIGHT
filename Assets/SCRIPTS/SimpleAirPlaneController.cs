using UnityEngine;
using System;
using TMPro;

namespace HeneGames.Airplane
{
    [RequireComponent(typeof(Rigidbody))]
    public class SimpleAirPlaneController : MonoBehaviour
    {
        public Plane plane;
        public GameObject text;

        public enum AirplaneState
        {
            Ground,
            Flying,
            Landing,
            Crashed
        }

        public AirplaneState airplaneState = AirplaneState.Ground;

        private Rigidbody rb;

        private float forwardSpeed;
        private float inputH;
        private bool throttle;
        private bool landInput;

        private float currentPitch;
        private float flareTimer;

        private Playerinput inputActions;
        private Vector2 moveInput;

        [Header("Speed")]
        [SerializeField] private float groundAcceleration = 15f;
        [SerializeField] private float airAcceleration = 8f;
        [SerializeField] private float groundDeceleration = 12f;
        [SerializeField] private float takeoffSpeed = 20f;
        [SerializeField] private float maxSpeed = 35f;

        [Header("Lift & Gravity")]
        [SerializeField] private float liftForce = 25f;
        [SerializeField] private float landingGravity = 20f;

        [Header("Pitch Control")]
        [SerializeField] private float pitchDownSpeed = 25f;
        [SerializeField] private float pitchUpSpeed = 15f;
        [SerializeField] private float pitchRecoverySpeed = 6f;
        [SerializeField] private float maxPitchDown = 25f;
        [SerializeField] private float maxPitchUp = 15f;

        [Header("Rotation")]
        [SerializeField] private float yawSpeed = 50f;
        [SerializeField] private float rollSpeed = 120f;

        [Header("Crash Settings")]
        [SerializeField] private float crashVerticalSpeed = 8f;
        [SerializeField] private float maxLandingRoll = 25f;
        [SerializeField] private float flareTimeRequired = 0.4f;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckDistance = 1.6f;
        [SerializeField] private LayerMask groundMask;

        private void Awake()
        {
            inputActions = new Playerinput();

            text.SetActive(false);
            rb = GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            rb.constraints =
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;

            currentPitch = 0f;
        }

        private void OnEnable()
        {
            inputActions.Enable();

            inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

            inputActions.Player.Throttle.performed += _ => throttle = true;
            inputActions.Player.Throttle.canceled += _ => throttle = false;

            inputActions.Player.Land.performed += _ => landInput = true;
            inputActions.Player.Land.canceled += _ => landInput = false;
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void Update()
        {
            inputH = moveInput.x;
        }

        private void FixedUpdate()
        {
            switch (airplaneState)
            {
                case AirplaneState.Ground:
                    GroundUpdate();
                    break;

                case AirplaneState.Flying:
                    FlyingUpdate();
                    break;

                case AirplaneState.Landing:
                    LandingUpdate();
                    break;
            }
        }

        private void GroundUpdate()
        {
            rb.useGravity = true;
            plane.engenOn = throttle;

            if (throttle)
                forwardSpeed += groundAcceleration * Time.fixedDeltaTime;
            else
                forwardSpeed -= groundDeceleration * Time.fixedDeltaTime;

            forwardSpeed = Mathf.Clamp(forwardSpeed, 0f, maxSpeed);

            rb.velocity = transform.forward * forwardSpeed;

            rb.MoveRotation(
                Quaternion.Euler(
                    0f,
                    rb.rotation.eulerAngles.y + inputH * yawSpeed * Time.fixedDeltaTime,
                    0f
                )
            );

            if (throttle && forwardSpeed >= takeoffSpeed)
                airplaneState = AirplaneState.Flying;
        }

        private void FlyingUpdate()
        {
            plane.engenOn = throttle;

            forwardSpeed += airAcceleration * Time.fixedDeltaTime;
            forwardSpeed = Mathf.Clamp(forwardSpeed, takeoffSpeed, maxSpeed);

            rb.useGravity = false;

            if (landInput)
                currentPitch += pitchDownSpeed * Time.fixedDeltaTime;
            else if (throttle)
                currentPitch -= pitchUpSpeed * Time.fixedDeltaTime;
            else
                currentPitch = Mathf.Lerp(currentPitch, 0f, pitchRecoverySpeed * Time.fixedDeltaTime);

            currentPitch = Mathf.Clamp(currentPitch, -maxPitchUp, maxPitchDown);

            rb.velocity = transform.forward * forwardSpeed;

            rb.MoveRotation(
                Quaternion.Euler(
                    currentPitch,
                    rb.rotation.eulerAngles.y + inputH * yawSpeed * Time.fixedDeltaTime,
                    -inputH * rollSpeed * Time.fixedDeltaTime
                )
            );

            if (landInput)
            {
                flareTimer = 0f;
                airplaneState = AirplaneState.Landing;
            }
        }

        private void LandingUpdate()
        {
            plane.engenOn = true;
            rb.useGravity = true;

            forwardSpeed -= groundDeceleration * 0.5f * Time.fixedDeltaTime;
            forwardSpeed = Mathf.Clamp(forwardSpeed, 0f, maxSpeed);

            if (landInput)
            {
                currentPitch += pitchDownSpeed * Time.fixedDeltaTime;
                rb.AddForce(Vector3.down * landingGravity, ForceMode.Force);
            }
            else if (throttle)
            {
                flareTimer += Time.fixedDeltaTime;
                currentPitch -= pitchUpSpeed * Time.fixedDeltaTime;
            }
            else
            {
                currentPitch = Mathf.Lerp(currentPitch, 0f, pitchRecoverySpeed * Time.fixedDeltaTime);
            }

            currentPitch = Mathf.Clamp(currentPitch, -maxPitchUp, maxPitchDown);

            rb.velocity = new Vector3(
                transform.forward.x * forwardSpeed,
                rb.velocity.y,
                transform.forward.z * forwardSpeed
            );

            rb.MoveRotation(
                Quaternion.Euler(
                    currentPitch,
                    rb.rotation.eulerAngles.y + inputH * yawSpeed * Time.fixedDeltaTime,
                    0f
                )
            );

            if (IsGrounded())
                CheckCrashOrLand();

            if (!landInput && !IsGrounded())
                airplaneState = AirplaneState.Flying;
        }

       private void CheckCrashOrLand()
    {
    if (rb.velocity.y < -crashVerticalSpeed)
    {
        Crash();
        return;
    }

    rb.velocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;

    rb.rotation = Quaternion.Euler(0f, rb.rotation.eulerAngles.y, 0f);

    currentPitch = 0f;
    flareTimer = 0f;

    plane.engenOn = false;
    airplaneState = AirplaneState.Ground;
    }

        private void Crash()
        {
            airplaneState = AirplaneState.Crashed;
            plane.engenOn = false;

            rb.constraints = RigidbodyConstraints.None;
            rb.useGravity = true;
            text.SetActive(true);

            Debug.Log("💥 CRASH LANDING");
        }

        private bool IsGrounded()
        {
            return Physics.Raycast(transform.position,Vector3.down,groundCheckDistance,groundMask);
        }
    }
}