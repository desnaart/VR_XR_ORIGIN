using UnityEngine;

public class SharedMaterialBlink : MonoBehaviour
{
    [Header("Material yang dipakai banyak object")]
    public Material targetMaterial;

    [Header("Warna")]
    public Color warnaNormal = Color.white;
    public Color warnaKedip = Color.magenta;

    [Header("Kedip")]
    public float kecepatanKedip = 2f;

    void Update()
    {
        if (targetMaterial == null) return;

        float t = Mathf.PingPong(Time.time * kecepatanKedip, 1f);

        targetMaterial.color = Color.Lerp(warnaNormal, warnaKedip, t);
    }
}