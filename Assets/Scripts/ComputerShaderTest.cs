using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerShaderTest : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture renderTexture;

    private void Start()
    {
        renderTexture = new RenderTexture(256, 256, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        
        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(256, 256, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }
        
        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetFloat("Resolution", renderTexture.width);
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
        Graphics.Blit(renderTexture, dest);
    }
}
