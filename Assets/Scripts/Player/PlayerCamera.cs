using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform _cameraAnchor;
    [SerializeField] private Transform _cameraContainer;
    [SerializeField] private float _cameraPositionDistance;
    [SerializeField] private float _cameraSensitivity;

    [HideInInspector] public float yAngle;

    private Transform _camera;
    private PlayerController _playerController;
    private float _xRotation;
    private float _yRotation;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        InitCamera();
    }

    private void Update()
    {
        UpdateCameraTransform();
    }

    private void InitCamera()
    {
        if (_playerController.isLocalPlayer)
        {
            _camera = Camera.main.transform;
            float x = _cameraAnchor.position.x;
            float y = _cameraAnchor.position.y;
            float z = _cameraAnchor.position.z - _cameraPositionDistance;
            _cameraContainer.position = new Vector3(x, y, z);
        }
    }

    private void UpdateCameraTransform()
    {
        if (_playerController.isLocalPlayer)
        {
            _xRotation += -Input.GetAxis("Mouse Y") * _cameraSensitivity;
            _yRotation += Input.GetAxis("Mouse X") * _cameraSensitivity;

            _cameraAnchor.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
            yAngle = _cameraAnchor.eulerAngles.y;
            _camera.position = _cameraContainer.position;
            _camera.forward = _cameraAnchor.forward;
        }
    }
}
