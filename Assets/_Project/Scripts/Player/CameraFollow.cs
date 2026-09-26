using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float FollowSpeed = 2f;
    public Transform Target;

    [Header("Límites de la Cámara")]
    public bool useLimits = true;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -2f; // Ajusta este valor en el Inspector para evitar que la cámara baje
    public float maxY = 10f;

    [Header("Efecto Shake")]
    public float shakeDuration = 0f;
    public float shakeAmount = 0.1f;
    public float decreaseFactor = 1.0f;

    Vector3 targetPosition;

    private void Start()
    {
        FindTarget();
        if (Target != null)
        {
            float startX = Target.position.x;
            float startY = Target.position.y;

            if (useLimits)
            {
                startX = Mathf.Clamp(startX, minX, maxX);
                startY = Mathf.Clamp(startY, minY, maxY);
            }

            transform.position = new Vector3(startX, startY, -10f);
        }
    }

    private void FindTarget()
    {
        if (Target != null) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) Target = player.transform;
    }

    private void LateUpdate()
    {
        if (Target == null)
        {
            FindTarget();
            if (Target == null) return;
        }

        Vector3 newPosition = Target.position;

        // Delimitar la posición a la que quiere ir la cámara
        if (useLimits)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        }

        newPosition.z = -10f;

        // Mover la cámara de forma suave hacia la posición delimitada
        targetPosition = Vector3.Lerp(transform.position, newPosition, Mathf.Clamp01(FollowSpeed * Time.deltaTime));

        // Aplicar el temblor si está activo
        if (shakeDuration > 0)
        {
            Vector2 offset = Random.insideUnitCircle * shakeAmount;
            targetPosition += new Vector3(offset.x, offset.y, 0f);
            shakeDuration -= Time.deltaTime * decreaseFactor;
        }

        transform.position = targetPosition;
    }

    public void ShakeCamera()
    {
        shakeDuration = 0.2f;
    }
}