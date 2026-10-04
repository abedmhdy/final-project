using UnityEngine;

// A small number that floats up from an enemy and fades out when it is hit.
// Created entirely from code (no prefab), using Unity's built-in font.
public class DamagePopup : MonoBehaviour
{
    private const float Lifetime = 0.6f;
    private const float RiseSpeed = 1.2f;

    private static Font font;

    private TextMesh textMesh;
    private Color baseColor;
    private float age;

    public static void Spawn(Vector3 position, string text, Color color)
    {
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject popupObject = new GameObject("DamagePopup");
        popupObject.transform.position = position;

        MeshRenderer meshRenderer = popupObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = font.material;
        meshRenderer.sortingOrder = 10;

        TextMesh textMesh = popupObject.AddComponent<TextMesh>();
        textMesh.font = font;
        textMesh.text = text;
        textMesh.fontSize = 48;
        textMesh.characterSize = 0.07f;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;

        DamagePopup popup = popupObject.AddComponent<DamagePopup>();
        popup.textMesh = textMesh;
        popup.baseColor = color;
    }

    private void Update()
    {
        age += Time.deltaTime;
        transform.position += Vector3.up * RiseSpeed * Time.deltaTime;

        Color color = baseColor;
        color.a = 1f - Mathf.Clamp01(age / Lifetime);
        textMesh.color = color;

        if (age >= Lifetime) Destroy(gameObject);
    }
}
