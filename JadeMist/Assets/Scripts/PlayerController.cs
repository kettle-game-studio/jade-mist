using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public class GravitySettings
    {
        public Vector3 vector;
        public float count;
        public Vector3 Value { get; private set; }
        public Func<Vector3, Vector3> DefaultVector{get; private set;}
        public float InterpolationPeriod{get;}
        public AnimationCurve GravityCurve{get;}

        Func<Vector3, Vector3> lastDefaultVector;
        float lastUpdateTime;
        public Vector3 CurrentDefaultGravity(Vector3 point) => Vector3.Lerp(
            lastDefaultVector(point), DefaultVector(point),
            GravityCurve.Evaluate((Time.time - lastUpdateTime) / InterpolationPeriod)
        );

        public GravitySettings(Func<Vector3, Vector3> defaultVectorField, float interpolationPeriod, AnimationCurve gravityCurve)
        {
            GravityCurve = gravityCurve;
            InterpolationPeriod = interpolationPeriod;
            lastDefaultVector = defaultVectorField;
            DefaultVector = defaultVectorField;
            Value = Vector3.zero;
            lastUpdateTime = Time.time;
            count = 0;
            vector = Vector3.zero;
        }

        public void UpdateDefaultGravity(Func<Vector3, Vector3> newGravityField)
        {
            lastDefaultVector = (Vector3) => Value;
            DefaultVector = newGravityField;
            lastUpdateTime = Time.time;
        }

        public void Reset(Vector3 point)
        {
            Value = count > 1 ? vector / count : (vector + CurrentDefaultGravity(point) * (1 - count));
            vector = Vector3.zero;
            count = 0;
        }
    }

    public Transform playerCamera;
    public Vector3 baseGravity = Vector3.down;
    public GravitySettings gravity;

    public float mouseSpeed = 1;
    public float lookVerticalLimitFrom = -60;
    public float lookVerticalLimitTo = 60;
    public float moveSpeed = 50;
    public float jumpHeight = 2;
    public float jumpAngle = 45;
    public float updateGravityPeriod = 1;
    public AnimationCurve gravityCurve = AnimationCurve.Linear(0, 0, 1, 1);

    Rigidbody rigidBody;
    InputAction lookAction;
    InputAction moveAction;
    InputAction jumpAction;
    float verticalLookAngle;
    bool canJump = false;

    Vector3 DownVector => transform.rotation * Vector3.down;
    Vector3 ForwardVector => transform.rotation * Vector3.forward;
    Vector3 RightVector => transform.rotation * Vector3.right;
    float JumpVelocity => Mathf.Sqrt(2 * baseGravity.magnitude * jumpHeight);

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        lookAction = InputSystem.actions.FindAction("Look");
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        gravity = new GravitySettings((Vector3) => baseGravity, updateGravityPeriod, gravityCurve);
    }

    void Update()
    {
        Vector2 lookValue = lookAction.ReadValue<Vector2>();
        lookValue = lookValue * mouseSpeed;
        verticalLookAngle = Mathf.Clamp(verticalLookAngle + lookValue.y, lookVerticalLimitFrom, lookVerticalLimitTo);
        playerCamera.transform.localRotation = Quaternion.AngleAxis(verticalLookAngle, Vector3.left);
        transform.rotation = Quaternion.AngleAxis(lookValue.x, -DownVector) * transform.rotation;
    }

    void FixedUpdate()
    {
        gravity.Reset(transform.position);
        transform.rotation = ToGravityRotation() * transform.rotation;
        Vector2 moveValue = moveAction.ReadValue<Vector2>() * moveSpeed;
        float downSpeed = Vector3.Dot(rigidBody.linearVelocity, DownVector);

        if (jumpAction.IsPressed() && canJump){
            downSpeed = -JumpVelocity;
            Debug.Log(JumpVelocity);
        }

        rigidBody.linearVelocity =
            DownVector * downSpeed +
            gravity.Value * Time.deltaTime +
            ForwardVector * moveValue.y +
            RightVector * moveValue.x;

        canJump = false;
    }

    Quaternion ToGravityRotation()
    {
        Vector3 axis = Vector3.Cross(DownVector, gravity.Value.normalized);
        float angle = Mathf.Asin(axis.magnitude);
        return Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (var contact in collision.contacts)
            if (Vector3.Dot(contact.normal, -gravity.Value.normalized) > Mathf.Cos(jumpAngle * Mathf.Deg2Rad))
                canJump = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, playerCamera.position);
        Gizmos.DrawWireSphere(transform.position - DownVector * 0.3f, 0.3f);
        Gizmos.DrawWireSphere(playerCamera.position, 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + RightVector);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + DownVector);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + ForwardVector);
    }
}
