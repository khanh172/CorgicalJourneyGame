using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(Animator))]
public class GridPlayerController : MonoBehaviour
{
    public Transform startPosition;
    public float moveDistance = 1f;
    public float moveSpeed = 5f;
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;
    public LayerMask goalLayer;
    public float rotationDuration = 0.5f;

    public Animator animator;
    public Transform mouthPoint;

    private Vector3 targetPosition;
    private Vector3 previousPosition;
    private bool isMoving = false;
    private bool isPickingUp = false;
    private GameObject carriedStick = null;
    private Transform carriedStickNearPoint = null;
    private Vector3 carriedStickOffset = Vector3.zero;

    private int currentYRotation = 0;
    private bool isRotating = false;
    private int lastRotationDirection = 0;
    private bool hasReversedRotation = false;

    private const float fixedY = -1.167f;
    private int originalYRotationBeforeRotate = 0;

    private Tween moveTween;
    private Tween rotateTween;

    private bool hasWon = false;
    private Quaternion stickInitialRotation; 

    private GameObject pendingStickToPick = null;

    void Start()
    {
        animator = GetComponent<Animator>();
        transform.position = RoundVector(startPosition != null ? startPosition.position : transform.position);
        targetPosition = transform.position;

        currentYRotation = Mathf.RoundToInt(transform.eulerAngles.y / 90f) * 90 % 360;
        transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
    }

    void Update()
    {
        if (hasWon || isPickingUp || isRotating || isMoving) return;

        if (Input.GetKeyDown(KeyCode.Q)) { StartRotation(90, -1); return; }
        if (Input.GetKeyDown(KeyCode.E)) { StartRotation(-90, 1); return; }

        Vector3 inputDir = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.W)) inputDir = Vector3.forward;
        if (Input.GetKeyDown(KeyCode.S)) inputDir = Vector3.back;
        if (Input.GetKeyDown(KeyCode.A)) inputDir = Vector3.left;
        if (Input.GetKeyDown(KeyCode.D)) inputDir = Vector3.right;

        if (inputDir != Vector3.zero)
        {
            Vector3 nextPos = RoundVector(transform.position + inputDir * moveDistance);
            if (IsGroundBelow(nextPos) && !IsBlocked(nextPos))
            {
                previousPosition = transform.position;
                targetPosition = nextPos;
                isMoving = true;
                animator.SetInteger("AnimationID", 2);
                GameManager.Instance?.PlaySound(GameManager.Instance.soundMove);

                if (moveTween != null && moveTween.IsActive()) moveTween.Kill();
                moveTween = transform.DOMove(targetPosition, moveDistance / moveSpeed).SetEase(Ease.Linear).OnUpdate(() =>
                {
                    if (carriedStick != null && StickHitBlocked())
                    {
                        CancelMovement();
                    }
                }).OnComplete(() =>
                {
                    isMoving = false;
                    animator.SetInteger("AnimationID", 0);
                    transform.position = targetPosition;

                    if (carriedStick != null && IsOnGoal())
                    {
                        WinGame();
                    }
                });
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (carriedStick == null)
            {
                GameObject stick = DetectStickNearby();
                if (stick != null) StartCoroutine(PickUpStick(stick));
            }
            else DropStick();
        }

        if (!isMoving && !isRotating)
            transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);
    }

    void StartRotation(int degree, int direction)
    {
        lastRotationDirection = direction;
        originalYRotationBeforeRotate = currentYRotation;
        currentYRotation = (currentYRotation + degree + 360) % 360;

        isRotating = true;
        hasReversedRotation = false;

        if (rotateTween != null && rotateTween.IsActive()) rotateTween.Kill();
        
        rotateTween = transform.DORotate(Vector3.up * currentYRotation, rotationDuration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                if (!hasReversedRotation && carriedStick != null && StickHitBlocked())
                {
                    ReverseRotation();
                }
            })
            .OnComplete(() =>
            {
                isRotating = false;
                animator.SetInteger("AnimationID", 0);
                if (pendingStickToPick != null)
                {
                    StartCoroutine(PickUpStick(pendingStickToPick));
                    pendingStickToPick = null;
                }
            });

        animator.SetInteger("AnimationID", 3);
    }


    void ReverseRotation()
    {
        if (rotateTween != null && rotateTween.IsActive()) rotateTween.Kill();
        hasReversedRotation = true;
        isRotating = true;

        currentYRotation = originalYRotationBeforeRotate;

        rotateTween = transform.DORotate(Vector3.up * currentYRotation, rotationDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isRotating = false;
                animator.SetInteger("AnimationID", 0);
            });
    }

    void CancelMovement()
    {
        if (moveTween != null && moveTween.IsActive()) moveTween.Kill();
        isMoving = false;
        transform.position = previousPosition;
        animator.SetInteger("AnimationID", 0);
        GameManager.Instance?.PlaySound(GameManager.Instance.soundStickBlocked);

        
    }

    bool StickHitBlocked()
    {
        if (carriedStick == null) return false;

        Transform pointA = carriedStick.transform.Find("PointA");
        Transform pointB = carriedStick.transform.Find("PointB");

        if (pointA == null || pointB == null)
        {
            
            return false;
        }

        Collider[] collidersA = Physics.OverlapSphere(pointA.position, 0.2f);
        Collider[] collidersB = Physics.OverlapSphere(pointB.position, 0.2f);

        foreach (var col in collidersA)
            if (col.CompareTag("Blocked")) return true;

        foreach (var col in collidersB)
            if (col.CompareTag("Blocked")) return true;

        return false;
    }


    bool IsGroundBelow(Vector3 position)
    {
        Ray ray = new Ray(position + Vector3.up * 2f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 5f, groundLayer))
            return hit.collider.CompareTag("Ground");
        return false;
    }

    bool IsBlocked(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, 0.3f, obstacleLayer);
        foreach (Collider col in hits)
        {
            if (col.CompareTag("Blocked"))
                return true;
        }
        return false;
    }

    bool IsOnGoal()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.3f, goalLayer);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Goal")) return true;
        }
        return false;
    }

    void WinGame()
    {
        if (hasWon) return;
        hasWon = true;

        GameManager.Instance?.SpawnWinEffect(transform.position + Vector3.up * 1f); // ✅ Hiệu ứng
        GameManager.Instance?.PlaySoundForSeconds(GameManager.Instance.soundWin, 3f);
        
        animator.SetInteger("AnimationID", 6);

        
        StartCoroutine(DelayWin());
    }

    IEnumerator DelayWin()
    {
        
        yield return new WaitForSeconds(4f);
        GameManager.Instance?.WinGame();
    }


    Vector3 RoundVector(Vector3 v)
    {
        return new Vector3(Mathf.Round(v.x), fixedY, Mathf.Round(v.z));
    }

    GameObject DetectStickNearby()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Stick")) return col.gameObject;
        }
        return null;
    }

    IEnumerator PickUpStick(GameObject stick)
    {
        isPickingUp = true;

        Transform pointA = stick.transform.Find("PointA");
        Transform pointB = stick.transform.Find("PointB");

        if (pointA == null || pointB == null)
        {
            
            isPickingUp = false;
            yield break;
        }

        float stickY = NormalizeAngle(stick.transform.eulerAngles.y);
        float playerY = NormalizeAngle(transform.eulerAngles.y);

        if ((Mathf.Abs(stickY) == 90 && Mathf.Abs(playerY) != 180) ||
            (Mathf.Abs(stickY) == 180 && Mathf.Abs(playerY) != 90) ||
            (Mathf.Abs(stickY) == 0 && Mathf.Abs(playerY) != 90))
        {
            

            float targetY = 0f;
            if (Mathf.Abs(stickY) == 90)
                targetY = 180f * Mathf.Sign(stickY);
            else
                targetY = 90f;

            float delta = NormalizeAngle(targetY - playerY);
            int direction = delta > 0 ? 1 : -1;

            pendingStickToPick = stick;
            StartRotation((int)Mathf.Abs(delta), direction);
            isPickingUp = false;
            yield break;
        }

        animator.SetInteger("AnimationID", 5);
        yield return new WaitForSeconds(0.2f);

        float distA = Vector3.Distance(mouthPoint.position, pointA.position);
        float distB = Vector3.Distance(mouthPoint.position, pointB.position);
        carriedStickNearPoint = distA < distB ? pointA : pointB;
        carriedStickOffset = carriedStickNearPoint.position - stick.transform.position;

        stickInitialRotation = stick.transform.rotation; // ✅ lưu lại hướng ban đầu

        stick.transform.SetParent(null);
        stick.transform.position = mouthPoint.position - carriedStickOffset;
        stick.transform.SetParent(mouthPoint);

        carriedStick = stick;
        animator.SetInteger("AnimationID", 0);
        isPickingUp = false;
    }


    void DropStick()
    {
        if (carriedStick == null || carriedStickNearPoint == null) return;

        Vector3 forwardDir = YRotationToDirection(currentYRotation);
        Vector3 dropPos = RoundVector(transform.position + forwardDir * 0.5f);

        if (!IsGroundBelow(dropPos))
        {
            Debug.Log("❌ Không có nền dưới vị trí thả!");
            return;
        }

        carriedStick.transform.SetParent(null);

       
        carriedStick.transform.rotation = Quaternion.Euler(0, currentYRotation+90, 0);


        
        Transform pointA = carriedStick.transform.Find("PointA");
        Transform pointB = carriedStick.transform.Find("PointB");

        if (pointA == null || pointB == null)
        {
           
            return;
        }

        Vector3 nearPointWorld = (carriedStickNearPoint == pointA) ? pointA.position : pointB.position;
        Vector3 delta = carriedStick.transform.position - nearPointWorld;

        carriedStick.transform.position = dropPos + delta;

        carriedStick = null;
        carriedStickNearPoint = null;
    }

    Vector3 YRotationToDirection(int yRotation)
    {
        switch (yRotation % 360)
        {
            case 0: return Vector3.forward;
            case 90: return Vector3.right;
            case 180: return Vector3.back;
            case 270: return Vector3.left;
            default: return Vector3.forward;
        }
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return Mathf.Round(angle);
    }

    
}
