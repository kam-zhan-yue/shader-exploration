using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using UnityEditor.SceneManagement;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Fractal : MonoBehaviour
{
    static readonly int MatricesID = Shader.PropertyToID("_Matrices");

    [SerializeField, Range(1,8)] private int depth;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;

    private static Vector3[] directions = {
        Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
    };

    private static Quaternion[] rotations = {
        Quaternion.identity,
        Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler(0f, 0f, 90f),
        Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f)
    };

    private FractalPart[][] _parts;
    private Matrix4x4[][] _matrices;
    private const int NODES = 5;

    //To render the parts, we need to send their matrices to the GPU
    private ComputeBuffer[] _matricesBuffers;

    private void OnEnable()
    {
        _parts = new FractalPart[depth][];
        _matrices = new Matrix4x4[depth][];
        _matricesBuffers = new ComputeBuffer[depth];
        //A 4x4 matrix has sixteen float values, so the stride of the buffers is 16 * 4
        int stride = 16 * 4;
        for (int i = 0, length = 1; i < _parts.Length; i++, length *= NODES) 
        {
            _parts[i] = new FractalPart[length];
            _matrices[i] = new Matrix4x4[length];
            _matricesBuffers[i] = new ComputeBuffer(length, stride);
        }

        _parts[0][0] = CreatePart(0);
        for (int li = 1; li < _parts.Length; li++) {
            FractalPart[] levelParts = _parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi+=5) {
                for (int ci = 0; ci < NODES; ci++) {
                    levelParts[fpi + ci] = CreatePart(ci);
                }
            }
        }
    }

    private FractalPart CreatePart(int childIndex)
    {
        FractalPart fractalPart = new()
        {
            direction = directions[childIndex],
            rotation = rotations[childIndex]
        };
        return fractalPart;
    }

    private void Update()
    {
        float spinAngleDelta = 22.5f * Time.deltaTime;

        FractalPart rootPart = _parts[0][0];
        rootPart.spinAngle += spinAngleDelta;
        rootPart.worldRotation = rootPart.rotation;
        _parts[0][0] = rootPart;
        
        //Create a simple transformation matrix from the root part with scale 1
        _matrices[0][0] = Matrix4x4.TRS(rootPart.worldPosition, rootPart.worldRotation, Vector3.one);

        float scale = 1f;
        //Start at one as the root part doesn't move
        for (int i = 1; i < _parts.Length; ++i)
        {
            scale *= 0.5f;
            FractalPart[] parentParts = _parts[i - 1];
            FractalPart[] levelParts = _parts[i];
            Matrix4x4[] levelMatrices = _matrices[i];
            for (int j = 0; j < levelParts.Length; ++j)
            {
                //Get the corresponding parent for every 5 children
                FractalPart parent = parentParts[j / 5];
                FractalPart part = levelParts[j];
                part.spinAngle += spinAngleDelta;
                //Propagate the rotation of the parent. Rotations can be stacked via a multiplication of quarternions
                part.worldRotation = parent.worldRotation * (part.rotation * Quaternion.Euler(0f, part.spinAngle, 0f));
                //The parent's rotation should also affect the direction of its offset
                part.worldPosition =
                    parent.worldPosition +
                    parent.worldRotation * (1.5f * scale * part.direction);
                levelParts[j] = part;
                levelMatrices[j] = Matrix4x4.TRS(part.worldPosition, part.worldRotation, scale * Vector3.one);
            }
        }
        
        //To upload the matrices to the GPU, invoke SetData on all buffers
        var bounds = new Bounds(Vector3.zero, 3f * Vector3.one);
        for (int i = 0; i < _matricesBuffers.Length; i++) {
            ComputeBuffer buffer = _matricesBuffers[i];
            buffer.SetData(_matrices[i]);
            material.SetBuffer(MatricesID, buffer);
            Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, buffer.count);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < _matricesBuffers.Length; ++i)
        {
            _matricesBuffers[i].Release();
        }

        _parts = null;
        _matrices = null;
        _matricesBuffers = null;
    }

    private void OnValidate()
    {
        if (_parts != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }
}