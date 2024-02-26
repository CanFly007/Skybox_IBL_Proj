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
        // ����һ��2D�������飬���ڴ洢ÿ������ն�ͼ
        irradianceMap = new RenderTexture(
            resolution, resolution, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        irradianceMap.enableRandomWrite = true;
        irradianceMap.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray; // ��Ϊ2D��������
        irradianceMap.volumeDepth = 6; // 6����
        irradianceMap.Create();

        // ����Compute Shader�е��ں�ID
        int kernelID = irradianceMapShader.FindKernel("CSMain");

        // ȷ��skyboxCubemap�Ѿ�����ֵ
        if (skyboxCubemap == null)
        {
            Debug.LogError("Skybox Cubemap is not assigned.");
            return;
        }

        // ����Compute Shader�Ĳ���
        irradianceMapShader.SetTexture(kernelID, "SkyboxTexture", skyboxCubemap);
        irradianceMapShader.SetTexture(kernelID, "Result", irradianceMap);
        irradianceMapShader.SetInt("_Resolution", resolution);

        // �����߳����С������Ҫ���ɵ��߳�����
        int threadGroupsX = Mathf.CeilToInt(resolution / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(resolution / 8.0f);

        // ����Compute Shader
        irradianceMapShader.Dispatch(kernelID, threadGroupsX, threadGroupsY, 1); // ȷ��Zά�ȵ��߳�����Ϊ1

        // �ڴ�֮��irradianceMap�������ɵ��ն�ͼ
    }

    void OnDestroy()
    {
        if (irradianceMap) irradianceMap.Release();
    }
}