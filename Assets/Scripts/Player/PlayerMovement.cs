using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 2;
    [HideInInspector] public bool isDashing = false;

    private Transform _transform;
    private PlayerController _playerController;
    private PlayerCamera _playerCamera;
    private Animator _animator;
    private Vector3 _dashLandPosition;
    private const string POS_Z_PARAMETER = "PosZ";
    private const string POS_X_PARAMETER = "PosX";
    private const string IS_DASHING_PARAMETER = "IsDashing";

    private void Awake()
    {
        _transform = transform;
        _playerController = GetComponent<PlayerController>();
        _playerCamera = GetComponent<PlayerCamera>();
        _animator = _playerController.animator;
    }

    private void Update()
    {
        if (_playerController.hasAuthority)
        {
            Movement();
            Dash();
        }
    }

    private void Movement()
    {
        float verticalAxis = isDashing ? 0 : Input.GetAxis("Vertical");
        float horizontalAxis = isDashing ? 0 : Input.GetAxis("Horizontal");
        Vector3 verticalDirection = Vector3.forward * verticalAxis;
        Vector3 horizontalDirection = Vector3.right * horizontalAxis;

        if (verticalAxis != 0)
            SetPlayerDirection();

        _transform.Translate(_movementSpeed * Time.deltaTime * (verticalDirection + horizontalDirection));
        _animator.SetFloat(POS_Z_PARAMETER, verticalAxis);
        _animator.SetFloat(POS_X_PARAMETER, horizontalAxis);
    }

    private void Dash()
    {
        if (!isDashing && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            SetPlayerDirection();
            isDashing = true;
            _dashLandPosition = _transform.position + _transform.forward * CustomNetworkRoomManager.DashDistance;
            _animator.SetBool(IS_DASHING_PARAMETER, isDashing);
        }

        if (isDashing)
        {
            _transform.position = Vector3.MoveTowards(_transform.position, _dashLandPosition, Time.deltaTime * CustomNetworkRoomManager.DashSpeed);

            if (Vector3.Distance(_transform.localPosition, _dashLandPosition) == 0)
            {
                isDashing = false;
                _animator.SetBool(IS_DASHING_PARAMETER, isDashing);
            }
        }
    }

    private void SetPlayerDirection()
    {
        _transform.eulerAngles = Vector3.up * _playerCamera.yAngle;
    }

    public void TeleportTo(Vector3 position)
    {
        if (_playerController.hasAuthority)
        {
            _transform.position = position;
        }
    }
}
