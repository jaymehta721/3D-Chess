using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraGradient : MonoBehaviour
{
    public Color topColor = Color.blue;
    public Color bottomColor = Color.red;

    private Material gradientMaterial;

    void Start()
    {
        // Load the shader and create a material
        Shader gradientShader = Shader.Find("Custom/GradientShader");
        gradientMaterial = new Material(gradientShader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Set the colors in the material
        gradientMaterial.SetColor("_TopColor", topColor);
        gradientMaterial.SetColor("_BottomColor", bottomColor);

        // Apply the material to the camera
        Graphics.Blit(src, dest, gradientMaterial);
    }
}
