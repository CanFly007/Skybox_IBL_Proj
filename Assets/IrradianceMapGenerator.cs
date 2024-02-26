using UnityEngine;

public class IrradianceMapGenerator : MonoBehaviour
{
    public ComputeShader irradianceMapShader;
    public Cubemap skyboxCubemap;
    public int resolution = 128;

    public RenderTexture irradianceMap;

    void Start()
    {
        GenerateIrradianceMap();
    }

    void GenerateIrradianceMap()
    {
        // 创建一个2D纹理数组，用于存储每个面的照度图
        irradianceMap = new RenderTexture(
            resolution, resolution, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        irradianceMap.enableRandomWrite = true;
        irradianceMap.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray; // 改为2D纹理数组
        irradianceMap.volumeDepth = 6; // 6个面
        irradianceMap.Create();

        // 查找Compute Shader中的内核ID
        int kernelID = irradianceMapShader.FindKernel("CSMain");

        // 确保skyboxCubemap已经被赋值
        if (skyboxCubemap == null)
        {
            Debug.LogError("Skybox Cubemap is not assigned.");
            return;
        }

        // 设置Compute Shader的参数
        irradianceMapShader.SetTexture(kernelID, "SkyboxTexture", skyboxCubemap);
        irradianceMapShader.SetTexture(kernelID, "Result", irradianceMap);
        irradianceMapShader.SetInt("_Resolution", resolution);

        // 根据线程组大小计算需要分派的线程组数
        int threadGroupsX = Mathf.CeilToInt(resolution / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(resolution / 8.0f);

        // 调度Compute Shader
        irradianceMapShader.Dispatch(kernelID, threadGroupsX, threadGroupsY, 1); // 确保Z维度的线程组数为1

        // 在此之后，irradianceMap包含生成的照度图
    }

    void OnDestroy()
    {
        if (irradianceMap) irradianceMap.Release();
    }
}