using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class DebugHelperScript : MonoBehaviour
{
    [Button]
    public void FetchNormalData()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, 2);
        Debug.Log(hit.collider.transform.up);
    }
    
    [Button]
    public void CalculateSlopeForce()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2))
        {
            Vector3 slopeNormal = hit.normal;
            float slopeAngle = Vector3.Angle(slopeNormal, Vector3.up);

            // Angle from perpendicular
            float angleFromPerpendicular = 90f - slopeAngle;

            Debug.Log($"Angle from perpendicular: {90f - angleFromPerpendicular}");

            if (slopeAngle <= 40f)
            {
                // Calculate force needed to counteract gravity on the slope
                float slopeForce = 5f * Physics.gravity.magnitude * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
                Vector3 forceDirection = Vector3.Cross(Vector3.Cross(slopeNormal, Vector3.down), slopeNormal).normalized;

                // Apply the force to keep the ball stationary on the slope
                Debug.Log(forceDirection * slopeForce);
            }
        }
    }
}
