using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TouchPhase = UnityEngine.TouchPhase;

namespace Hutilities.AR
{
	public class PlacementController : MonoBehaviour
	{
		private const int InputDelay = 100; //ms

		[SerializeField] private Camera _arCamera;
		[SerializeField] private ARRaycastManager _arRaycastManager;
		[SerializeField] private ARPlaneManager _arPlaneManager;
		[SerializeField] private GameObject _objectToCreate;

		private bool _isArPlaneDetected, _isInputAvailable;
		private Transform _createdObject;
		private Action<GameObject> _placementCallback;
		private static List<ARRaycastHit> _hits;

		private void Awake()
		{
			_arPlaneManager.planesChanged += PlaneChanged;

			_hits = new List<ARRaycastHit>();

			_isInputAvailable = true;
		}

		private void OnDestroy()
		{
			Destroy(_createdObject.gameObject);
		}

		public void BindPlacementCallback(Action<GameObject> action) => _placementCallback = action;
		public void Clear() => _isInputAvailable = false;
		private void Update() => InputHandler();
		private void ConfirmPlacement() => _isInputAvailable = false;

		private void PlaneChanged(ARPlanesChangedEventArgs obj)
		{
			if (_isArPlaneDetected)
				return;

			if (_arPlaneManager.trackables.count < 1)
				return;

			_isArPlaneDetected = true;

			_arPlaneManager.planesChanged -= PlaneChanged;

			SetDelayedInput();
		}

		private async void SetDelayedInput()
		{
			await Task.Delay(InputDelay);

			_isInputAvailable = true;
		}

		private void InputHandler()
		{
			var touchCount = Input.touchCount;

			if (!_isArPlaneDetected || !_isInputAvailable)
				return;

			//if (EventSystem.current.IsPointerOverGameObject())
			//	return;

			if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse0))
			{
				var pointerData = new PointerEventData(EventSystem.current);

				if (pointerData.selectedObject != null && pointerData.selectedObject.GetComponent<Selectable>() != null)
					return;
			}

			if (touchCount == 1)
			{
				var touch = Input.GetTouch(0);
				var touchPosition = touch.position;

				if (_arRaycastManager.Raycast(touchPosition, _hits, TrackableType.PlaneWithinPolygon))
				{
					var pose = _hits[0].pose;

					if (_createdObject == null)
						InstantiateObject(pose);
					else
					{
						switch (touch.phase)
						{
							case TouchPhase.Moved:
								_createdObject.transform.position = pose.position;
								SetRotation();
								break;
							case TouchPhase.Ended:
								ConfirmPlacement();
								break;
						}
					}
				}
				else
				{
					if (_createdObject != null)
						ConfirmPlacement();
				}
			}
		}

		public void InstantiateObject(Pose pose)
		{
			_createdObject =
				Instantiate(_objectToCreate, pose.position, Quaternion.identity).GetComponent<Transform>();

			_placementCallback?.Invoke(_createdObject.gameObject);

			SetRotation();
		}

		private void SetRotation()
		{
			var createdTransform = _createdObject.transform;
			var rotation = Quaternion.LookRotation(createdTransform.position - _arCamera.transform.position).eulerAngles;
			rotation.x = createdTransform.rotation.eulerAngles.x;
			rotation.z = createdTransform.rotation.eulerAngles.z;

			createdTransform.rotation = Quaternion.Euler(rotation);
		}
	}
}
