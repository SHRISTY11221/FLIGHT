using UnityEngine;

public class BalloonDropper : MonoBehaviour
{
    public GameObject balloonPrefab;
    public Transform dropPoint;
    public float dropCooldown = 1f;

    float lastDropTime;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && Time.time > lastDropTime)
        {
            DropBalloon();
            lastDropTime = Time.time + dropCooldown;
        }
    }

    void DropBalloon()
    {
        Instantiate(balloonPrefab, dropPoint.position, Quaternion.identity);
    }
}