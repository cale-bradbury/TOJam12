using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour {

    public Gradient color;
    public float smoothing = .1f;
    Light light;

	// Use this for initialization
	void OnEnable () {
        light = GetComponent<Light>();
	}
	
	// Update is called once per frame
	void Update () {
        light.color = Color.Lerp(light.color, color.Evaluate(Mathf.Pow(Random.value, 1.5f)), smoothing);
	}
}
