using System;
using UnityEngine;

public class Graph : MonoBehaviour 
{
	public Transform pointPrefab;

	[Range(10, 200)]
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

	private Transform[] _points = Array.Empty<Transform>();
	private float _duration;
	private bool _transitioning;
	private FunctionLibrary.FunctionName _transitionFunction;
	private void Awake()
	{
		float step = 2f / resolution;
		Vector3 position = Vector3.zero;
		Vector3 scale = Vector3.one * step;
		Debug.Log($"Scale: +{scale} Step: {step}");

		//Initialise points on a square grid
		_points = new Transform[resolution * resolution];
		for (int i = 0; i < _points.Length; i++)
		{
			Transform point = _points[i] = Instantiate(pointPrefab, transform, false);
			point.localPosition = position;
			point.localScale = scale;
		}
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

		//Update the function transition if needed
		if (_transitioning) {
			UpdateFunctionTransition();
		}
		//Otherwise, just update the function
		else {
			UpdateFunction();
		}
	}
	
	private void PickNextFunction () {
		function = transitionMode == TransitionMode.Cycle ?
			FunctionLibrary.GetNextFunctionName(function) :
			FunctionLibrary.GetRandomFunctionNameOtherThan(function);
	}
	
	private void UpdateFunctionTransition () {
		FunctionLibrary.Function
			from = FunctionLibrary.GetFunction(_transitionFunction),
			to = FunctionLibrary.GetFunction(function);
		float progress = _duration / _transitionDuration;
		float time = Time.time;
		float step = 2f / resolution;
		float v = 0.5f * step - 1f;
		for (int i = 0, x = 0, z = 0; i < _points.Length; i++, x++) {
			if (x == resolution) {
				x = 0;
				z += 1;
				v = (z + 0.5f) * step - 1f;
			}
			float u = (x + 0.5f) * step - 1f;
			_points[i].localPosition = FunctionLibrary.Morph(
				u, v, time, from, to, progress
			);
		}
	}
	
	private void UpdateFunction()
	{
		FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
		float time = Time.time;
		float step = 2f / resolution;
		float v = 0.5f * step - 1f;
		for (int i = 0, x = 0, z = 0; i < _points.Length; i++, x++) {
			if (x == resolution) {
				x = 0;
				z += 1;
				v = (z + 0.5f) * step - 1f;
			}
			float u = (x + 0.5f) * step - 1f;
			_points[i].localPosition = f(u, v, time);
		}
	}
}