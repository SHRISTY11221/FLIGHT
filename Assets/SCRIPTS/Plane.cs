using UnityEngine;



public class Plane : MonoBehaviour
{
    public GameObject prop;
    public GameObject propBlured;

    [HideInInspector]
    public bool engenOn;

    public float movementThreshold = 0.001f;

    Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        float movedDistance = Vector3.Distance(transform.position, lastPosition);

        engenOn = movedDistance > movementThreshold;

        if (engenOn)
        {
            prop.SetActive(false);
            propBlured.SetActive(true);
        }
        else
        {
            prop.SetActive(true);
            propBlured.SetActive(false);
        }

        lastPosition = transform.position;
    }
}
