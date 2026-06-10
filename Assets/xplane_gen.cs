using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class xplane_gen : MonoBehaviour
{
    public Texture2D texture;
    public float scale = 0.2f;

    void Start()
    {
        CreateCrossPlane();
    }

    void CreateCrossPlane()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.mainTexture = texture;

        // Quad pertama
        GameObject quad1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad1.transform.SetParent(transform);
        quad1.transform.localPosition = Vector3.zero;
        quad1.transform.localRotation = Quaternion.identity;
        quad1.transform.localScale = Vector3.one * scale;
        quad1.GetComponent<MeshRenderer>().material = mat;

        // Quad kedua
        GameObject quad2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad2.transform.SetParent(transform);
        quad2.transform.localPosition = Vector3.zero;
        quad2.transform.localRotation = Quaternion.Euler(0,90,0);
        quad2.transform.localScale = Vector3.one * scale;
        quad2.GetComponent<MeshRenderer>().material = mat;

        // hapus collider supaya lebih ringan
        Destroy(quad1.GetComponent<Collider>());
        Destroy(quad2.GetComponent<Collider>());
    }
}