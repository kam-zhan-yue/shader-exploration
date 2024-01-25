using System;
using UnityEngine;

public class GPUGraph : MonoBehaviour
{
	private const int MAX_RESOLUTION = 1000;
	//Compute Shader Variables
	public ComputeShader computeShader;

	private static readonly int PositionsId = Shader.PropertyToID("_Positions"),
		ResolutionId = Shader.PropertyToID("_Resolution"),
		StepId = Shader.PropertyToID("_Step"),
		TimeId = Shader.PropertyToID("_Time"),
		TransitionProgressId = Shader.PropertyToID("_TransitionProgress");
	private ComputeBuffer _positionsBuffer;

	public Material material;
	public Mesh mesh;
	
	[Range(10, MAX_RESOLUTION)]
	public int resolution = 10;
	
	[SerializeField]
	public FunctionLibrary.FunctionName function;
	private enum TransitionMode { Cycle, Random }
	[SerializeField]
	TransitionMode transitionMode;
	
	[Min(0f)] 
	private float _functionDuration = 1f;

	[Min(0f)]
	private float _transitionDuration = 1f;
	
	private float _duration;
	private bool _transitioning;
	private FunctionLibrary.FunctionName _transitionFunction;


	//Use OnEnable for hot reloads
	private void OnEnable() 
	{
		//A compute buffer contains arbitrary untyped data. Specify the exact size of each element in bytes
		//in the second argument. These are 3D position vectors, which consist of three float numbers.
		//Hence, an element size is three times four. Thus, 40,000 positions ~ 0.48MB of GPU memory.
		//Use max resolution for variable resolutions
		_positionsBuffer = new ComputeBuffer(MAX_RESOLUTION * MAX_RESOLUTION, 3*4);
	}

	private void OnDisable()
	{
		_positionsBuffer.Release();
		_positionsBuffer = null;
	}
	
	private void Update()
	{
		_duration += Time.deltaTime;
		
		//If the function is transitioning, use duration until transition is over
		if (_transitioning)
		{
			if (_duration >= _transitionDuration) {
				_duration -= _transitionDuration;
				_transitioning = false;
			}
		}
		//If not, check one time if it is time to transition
		else if (_duration >= _functionDuration) {
			_transitioning = true;
			_transitionFunction = function;
			_duration -= _functionDuration;
			PickNextFunction();
		}
		UpdateFunctionOnGPU();
	}
	
	private void UpdateFunctionOnGPU() 
	{
		float step = 2f / resolution;
		computeShader.SetInt(ResolutionId, resolution);
		computeShader.SetFloat(StepId, step);
		computeShader.SetFloat(TimeId, Time.time);
		if (_transitioning) {
			computeShader.SetFloat(
				TransitionProgressId,
				Mathf.SmoothStep(0f, 1f, _duration / _transitionDuration)
			);
		}
		int kernelIndex =
			(int)function +
			(int)(_transitioning ? _transitionFunction : function) *
			FunctionLibrary.FunctionCount;
		computeShader.SetBuffer(kernelIndex, PositionsId, _positionsBuffer);
		int groups = Mathf.CeilToInt(resolution / 8f);
		computeShader.Dispatch(kernelIndex, groups, groups, 1);
		
		//Set parameters on the material, too
		material.SetBuffer(PositionsId, _positionsBuffer);
		material.SetFloat(StepId, step);
		//The graph sits at the origin and remains inside a cube with size 2.
		//The points have a size, which can poke out of the bounds in all directions
		Bounds bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));

		Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
	}
	
	private void PickNextFunction () {
		function = transitionMode == TransitionMode.Cycle ?
			FunctionLibrary.GetNextFunctionName(function) :
			FunctionLibrary.GetRandomFunctionNameOtherThan(function);
	}
}