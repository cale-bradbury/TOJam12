using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnable : MonoBehaviour {

    public LayerMask collisionLayer;
    public bool faceRay = true;
    public Vector3 adtionalRotation;

    internal Vector3[] allDirections = new Vector3[] { Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, Vector3.down };
    internal Vector3[] wallDirections = new Vector3[] { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };

    public virtual void Spawn(FloorMaster floor)
    {
        Vector3 pos = floor.FindEmpty();
        transform.parent = floor.transform;
        RaycastHit hit;
        Vector3 direction = allDirections[Mathf.FloorToInt(Random.value * allDirections.Length)];
        Physics.Raycast(pos, direction, out hit, 1000, collisionLayer);
        transform.position = hit.point;
        if (faceRay)
            transform.LookAt(transform.localToWorldMatrix.MultiplyPoint(direction));
        transform.localEulerAngles += adtionalRotation;
    }

}
