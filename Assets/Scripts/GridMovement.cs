using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GridMovement : MonoBehaviour
{
    [Header("Grid")]
    public float moveDuration = 0.2f;

    [Header("Jump Arc")]
    public float jumpHeight = 0.4f;
    public float meshYOffset = -0.05f;
    public Transform visualMesh;

    [Header("Death Shake")]
    public float deathShakeDuration  = 1.1f;
    public float deathShakeIntensity = 0.52f;

    [Header("Victory Animation")]
    public float victoryBounceSpeed  = 8f;
    public float victoryBounceHeight = 0.35f;
    public float victorySpinSpeed    = 280f;

    [Header("Audio")]
    public AudioClip moveClip;
    public AudioClip deathClip;
    public AudioClip winClip;

    [HideInInspector] public bool is2D;
    [HideInInspector] public bool inputLocked;

    private Vector3 startPos, targetPos;
    public Vector3 TargetPosition => targetPos;
    private bool isMoving;
    private float t;
    private bool isJumping;
    private Vector3Int? pendingMoveTarget;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        GameEvents.OnPlayerDeath += OnDeath;
        GameEvents.OnLevelWin   += OnWin;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerDeath -= OnDeath;
        GameEvents.OnLevelWin   -= OnWin;
    }

    void Start()
    {
        targetPos = transform.position;
    }

    void Update()
    {
        if (isMoving)
        {
            t += Time.deltaTime / moveDuration;
            float smooth = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            transform.position = Vector3.Lerp(startPos, targetPos, smooth);

            if (visualMesh != null)
            {
                float arc = isJumping ? Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * jumpHeight : 0f;
                visualMesh.localPosition = new Vector3(0f, meshYOffset + arc, 0f);
            }

            if (t >= 1f)
            {
                transform.position = targetPos;
                if (visualMesh != null) visualMesh.localPosition = new Vector3(0f, meshYOffset, 0f);
                isMoving  = false;
                isJumping = false;
                OnMoveComplete();
            }
            return;
        }

        if (inputLocked) return;

        Vector3 dir = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.W))      dir = Vector3.forward;
        else if (Input.GetKeyDown(KeyCode.S)) dir = Vector3.back;
        else if (Input.GetKeyDown(KeyCode.A)) dir = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.D)) dir = Vector3.right;

        if (dir != Vector3.zero) TryMove(dir);
    }

    void OnDeath(DeathCause cause)
    {
        Play(deathClip);
        StartCoroutine(DeathShake());
    }

    void OnWin()
    {
        Play(winClip);
        StartCoroutine(VictoryAnimation());
    }

    IEnumerator DeathShake()
    {
        if (visualMesh == null) yield break;

        Vector3 origin  = visualMesh.localPosition;
        float   elapsed = 0f;

        while (elapsed < deathShakeDuration)
        {
            elapsed += Time.deltaTime;
            float strength = deathShakeIntensity * (1f - elapsed / deathShakeDuration);
            visualMesh.localPosition = origin + (Vector3)Random.insideUnitCircle * strength;
            yield return null;
        }

        visualMesh.localPosition = origin;
    }

    IEnumerator VictoryAnimation()
    {
        if (visualMesh == null) yield break;

        Vector3 origin  = visualMesh.localPosition;
        float   elapsed = 0f;

        while (true)
        {
            elapsed += Time.deltaTime;
            float bounce = Mathf.Abs(Mathf.Sin(elapsed * victoryBounceSpeed)) * victoryBounceHeight;
            visualMesh.localPosition = new Vector3(origin.x, origin.y + bounce, origin.z);
            visualMesh.Rotate(0f, victorySpinSpeed * Time.deltaTime, 0f);
            yield return null;
        }
    }

    void TryMove(Vector3 dir)
    {
        if (is2D) { TryMove2D(dir); return; }

        Vector3Int current = Vector3Int.RoundToInt(transform.position);
        Vector3Int next    = current + Vector3Int.RoundToInt(dir);
        Vector3Int below   = next + Vector3Int.down;

        bool nextOccupied = GridManager.Instance.HasBlock(next);
        bool floorExists  = GridManager.Instance.HasBlock(below);

        if (!nextOccupied && floorExists)
        {
            BeginMove(next, false);
            return;
        }

        if (nextOccupied)
        {
            Vector3Int stepUp      = next + Vector3Int.up;
            Vector3Int abovePlayer = current + Vector3Int.up;

            if (!GridManager.Instance.HasBlock(stepUp) && !GridManager.Instance.HasBlock(abovePlayer))
                BeginMove(stepUp, true);
            return;
        }

        if (!nextOccupied && !floorExists)
        {
            Vector3Int dropCell  = next + Vector3Int.down;
            Vector3Int dropFloor = dropCell + Vector3Int.down;
            if (!GridManager.Instance.HasBlock(dropCell) && GridManager.Instance.HasBlock(dropFloor))
            {
                pendingMoveTarget = dropCell;
                BeginMove(next, false);
            }
        }
    }

    void TryMove2D(Vector3 dir)
    {
        Vector3Int current = Vector3Int.RoundToInt(transform.position);
        var flatDir        = new Vector2Int(Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.z));
        var targetCol      = new Vector2Int(current.x, current.z) + flatDir;
        var flatGrid       = PerspectiveController.Instance.flatGrid;

        if (!flatGrid.TryGetValue(targetCol, out BlockType type)) return;
        if (type == BlockType.Brick) return;

        BeginMove(new Vector3Int(targetCol.x, 1, targetCol.y), false);
    }

    void BeginMove(Vector3Int targetCell, bool jumping)
    {
        startPos  = transform.position;
        targetPos = new Vector3(targetCell.x, targetCell.y, targetCell.z);
        isJumping = jumping;
        t         = 0f;
        isMoving  = true;
        Play(moveClip);
    }

    void Play(AudioClip clip)
    {
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    void OnMoveComplete()
    {
        if (pendingMoveTarget.HasValue)
        {
            var target = pendingMoveTarget.Value;
            pendingMoveTarget = null;
            BeginMove(target, false);
            return;
        }

        if (is2D)
        {
            Vector3Int cell = Vector3Int.RoundToInt(transform.position);
            var col = new Vector2Int(cell.x, cell.z);
            if (PerspectiveController.Instance.flatGrid.TryGetValue(col, out BlockType type))
            {
                if (type == BlockType.Red) { GameEvents.TriggerDeath(DeathCause.RedZone); return; }
                if (type == BlockType.Win) { GameEvents.TriggerWin();                     return; }
            }
            return;
        }

        Vector3Int c     = Vector3Int.RoundToInt(transform.position);
        Vector3Int below = c + Vector3Int.down;

        if (GridManager.Instance.TryGetBlock(below, out var data))
        {
            if (data.type == BlockType.Red) { GameEvents.TriggerDeath(DeathCause.RedZone); return; }
            if (data.type == BlockType.Win) { GameEvents.TriggerWin();                     return; }
        }
    }

    public void SetTarget(Vector3 pos)
    {
        targetPos = pos;
        isMoving  = false;
    }
}
