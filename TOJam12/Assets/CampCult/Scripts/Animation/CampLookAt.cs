using UnityEngine;
using System.Collections;

public class CampLookAt : MonoBehaviour {

    public Transform lookAt;
    public float smoothing = .9f;
    public bool yOnly = false;
	
    void OnEnable()
    {
        if (lookAt == null)
            lookAt = Camera.main.transform;
    }

	// Update is called once per frame
	void Update () {
        Quaternion q = transform.rotation;
        transform.LookAt(lookAt);
        if (yOnly)
            transform.localEulerAngles = Vector3.Scale(transform.localEulerAngles,Vector3.up);
        transform.rotation = Quaternion.Lerp(q, transform.rotation, smoothing);
	}
}
