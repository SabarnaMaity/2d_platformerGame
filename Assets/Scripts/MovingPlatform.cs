using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Points")]
    [Tooltip("Optional waypoints. If empty, platform moves between its start position and start position + moveOffset.")]
    public Transform[] waypoints;
    public Vector2 moveOffset = new Vector2(4f, 0f);

    [Header("Settings")]
    public float speed = 2.5f;
    public float waitTime = 0.5f;

    private List<Vector3> targetPoints = new List<Vector3>();
    private int currentPointIndex = 0;
    private bool movingForward = true;
    private float waitTimer = 0f;
    private Vector3 lastPosition;
    private Rigidbody2D rb;

    private readonly HashSet<Transform> riders = new HashSet<Transform>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }
    }

    private void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            foreach (Transform wp in waypoints)
            {
                if (wp != null)
                {
                    targetPoints.Add(wp.position);
                }
            }
        }

        if (targetPoints.Count < 2)
        {
            targetPoints.Clear();
            targetPoints.Add(transform.position);
            targetPoints.Add(transform.position + (Vector3)moveOffset);
        }

        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (targetPoints.Count < 2) return;

        Vector3 currentTarget = targetPoints[currentPointIndex];

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            lastPosition = transform.position;
            return;
        }

        Vector3 nextPos = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.fixedDeltaTime);
        Vector3 delta = nextPos - transform.position;

        if (rb != null)
        {
            rb.MovePosition(nextPos);
        }
        else
        {
            transform.position = nextPos;
        }

        // Move any riders (e.g. Player) by the platform's delta position
        if (riders.Count > 0 && delta != Vector3.zero)
        {
            foreach (Transform rider in riders)
            {
                if (rider != null)
                {
                    rider.position += delta;
                }
            }
        }

        lastPosition = nextPos;

        if (Vector3.Distance(transform.position, currentTarget) < 0.02f)
        {
            waitTimer = waitTime;
            if (movingForward)
            {
                currentPointIndex++;
                if (currentPointIndex >= targetPoints.Count)
                {
                    currentPointIndex = targetPoints.Count - 2;
                    movingForward = false;
                }
            }
            else
            {
                currentPointIndex--;
                if (currentPointIndex < 0)
                {
                    currentPointIndex = 1;
                    movingForward = true;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // Verify player is on top of platform (contact normal pointing mostly upwards)
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    riders.Add(collision.collider.transform);
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            riders.Remove(collision.collider.transform);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (waypoints != null && waypoints.Length > 0)
        {
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
        else
        {
            Vector3 start = Application.isPlaying ? (targetPoints.Count > 0 ? targetPoints[0] : transform.position) : transform.position;
            Vector3 end = start + (Vector3)moveOffset;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(start, 0.2f);
            Gizmos.DrawWireSphere(end, 0.2f);
        }
    }
}

