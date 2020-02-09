using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDotProduct : MonoBehaviour
{
    public float dotProduct;
    public Transform firstObject;
    public Transform secondObject;
    // Update is called once per frame
    void Update()
    {
        dotProduct = Vector3.Dot(firstObject.position, secondObject.position);
    }
}
