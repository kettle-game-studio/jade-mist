using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public class GravitySettings
    {
        public Vector3 vector;
        public float count;
        public Vector3 Value { get; private set; }
        public Func<Vector3, Vector3> DefaultVector { get; private set; }
        public float InterpolationPeriod { get; }
        public AnimationCurve GravityCurve { get; }

        Func<Vector3, Vector3> lastDefaultVector;
        float lastUpdateTime;
        public Vector3 CurrentDefaultGravity(Vector3 point)
        {
            float k = GravityCurve.Evaluate((Time.time - lastUpdateTime) / InterpolationPeriod);
            Vector3 a = lastDefaultVector(point);
            Vector3 b = DefaultVector(point);
            Vector3 result = a * (1 - k) + b * k;
            return result;
        }

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

        public void UpdateDefaultGravity(Func<Vector3, Vector3> newGravityField, bool force = false)
        {
            if (DefaultVector == newGravityField)
                return;
            Vector3 valueCopy = Value;
            lastDefaultVector = force ? newGravityField : (Vector3) => valueCopy;
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

    [Serializable]
    public class MoveSettings
    {
        public float moveSpeed = 6;
        public float flyVelocityRotation = -5;
        public float groundVelocityRotation = 1;
    }

    public InputActionAsset actions;

    public Transform respawnPoint;
    public float deathDistance = 200;
    public Transform playerCamera;
    public Vector3 baseGravity = Vector3.down;
    public GravitySettings gravity;


    public float mouseSpeed = 1;
    public float lookVerticalLimitFrom = -60;
    public float lookVerticalLimitTo = 60;
    public float updateGravityPeriod = 1;

    public float jumpHeight = 2;
    public float jumpAngle = 45;
    public MoveSettings walkSettings;
    public MoveSettings runSettings;

    public Image image;

    public AnimationCurve gravityCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Range(0, 1)]
    public float flyInertia = 0.8f;
    [Range(0, 1)]
    public float groundInertia = 0.2f;
    [Range(0, 1)]
    public float gravityVectorInterpolationK = 0.9f;


    MoveSettings moveSettings;
    Rigidbody rigidBody;
    InputAction lookAction;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;
    InputAction interactAction;
    float verticalLookAngle;
    bool canJump = false;

    Vector3 DownVector => transform.rotation * Vector3.down;
    Vector3 ForwardVector => transform.rotation * Vector3.forward;
    Vector3 RightVector => transform.rotation * Vector3.right;
    float JumpVelocity => Mathf.Sqrt(2 * baseGravity.magnitude * jumpHeight);

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rigidBody = GetComponent<Rigidbody>();
        gravity = new GravitySettings((Vector3) => baseGravity, updateGravityPeriod, gravityCurve);
        verticalLookAngle = 0;
        moveSettings = walkSettings;

        InputActionMap playerMap = actions.FindActionMap("Player");
        playerMap.Enable();
        lookAction = playerMap.FindAction("Look");
        moveAction = playerMap.FindAction("Move");
        jumpAction = playerMap.FindAction("Jump");
        sprintAction = playerMap.FindAction("Sprint");
        interactAction = playerMap.FindAction("Interact");
    }

    void Update()
    {

        var collide = Physics.Raycast(new Ray(playerCamera.position, playerCamera.transform.forward), out var raycastHitInfo, 3f);
        
        if (!collide || raycastHitInfo.collider == null || !raycastHitInfo.collider.gameObject.TryGetComponent<Interactinator>(out var interactinator))
        {
            image.color = Color.white;
        }
        else
        {
            image.color = Color.red;

            if (interactAction.WasPressedThisDynamicUpdate())
            {
                interactinator.Interact(this, raycastHitInfo);
            }
        }


        var from = respawnPoint == null ? Vector3.zero : respawnPoint.transform.position;
        if (Vector3.Distance(from, transform.position) > deathDistance)
            Die();

        Vector2 lookValue = lookAction.ReadValue<Vector2>();
        lookValue = lookValue * mouseSpeed;
        verticalLookAngle = Mathf.Clamp(verticalLookAngle + lookValue.y, lookVerticalLimitFrom, lookVerticalLimitTo);
        playerCamera.transform.localRotation = Quaternion.AngleAxis(verticalLookAngle, Vector3.left);
        transform.rotation = Quaternion.AngleAxis(lookValue.x, -DownVector) * transform.rotation;
    }

    void FixedUpdate()
    {
        moveSettings = sprintAction.IsPressed() ? runSettings : walkSettings;
        gravity.Reset(transform.position);
        transform.rotation = ToGravityRotationWithVelocity() * transform.rotation;
        Vector2 moveValue = moveAction.ReadValue<Vector2>() * moveSettings.moveSpeed;
        float downSpeed = Vector3.Dot(rigidBody.linearVelocity, DownVector);
        float forwardSpeed = Vector3.Dot(rigidBody.linearVelocity, ForwardVector);
        float rightSpeed = Vector3.Dot(rigidBody.linearVelocity, RightVector);

        float inertia = canJump ? groundInertia : flyInertia;
        if (canJump)

            if (jumpAction.IsPressed() && canJump)
                downSpeed = -JumpVelocity;

        rigidBody.linearVelocity =
            DownVector * downSpeed +
            ForwardVector * Mathf.Lerp(moveValue.y, forwardSpeed, inertia) +
            RightVector * Mathf.Lerp(moveValue.x, rightSpeed, inertia);

        if (!canJump)
            rigidBody.linearVelocity += gravity.Value * Time.deltaTime;

        canJump = false;
    }


    Vector3 lastTarget = Vector3.down;
    Quaternion ToGravityRotationWithVelocity()
    {
        Vector3 normalizedGravity = gravity.Value.normalized;
        Vector3 velocityRotationAxis = Vector3.Cross(rigidBody.linearVelocity, normalizedGravity);
        float velocityRotation = canJump ? moveSettings.groundVelocityRotation : moveSettings.flyVelocityRotation;
        Vector3 targetVector = Quaternion.AngleAxis(velocityRotationAxis.magnitude * velocityRotation / moveSettings.moveSpeed, velocityRotationAxis) * normalizedGravity;
        targetVector = Vector3.Lerp(lastTarget, targetVector, gravityVectorInterpolationK).normalized;
        lastTarget = targetVector;
        Vector3 axis = Vector3.Cross(DownVector, targetVector);
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

    public void SetRespawnPoint(Transform newRespawnpoint)
    {
        respawnPoint = newRespawnpoint;
    }

    public void Die()
    {
        if (respawnPoint != null)
            transform.SetPositionAndRotation(respawnPoint.transform.position, respawnPoint.transform.rotation);
        else
            transform.position = Vector3.zero;


        rigidBody.linearVelocity = Vector3.zero;
        lastTarget = baseGravity.normalized;
        gravity.UpdateDefaultGravity(_ => baseGravity, true);
    }
}
