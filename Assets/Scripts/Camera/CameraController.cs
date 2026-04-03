using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CameraController : MonoBehaviour
{
    private static CameraController _global;

    [Header("Zoom")]
    [SerializeField, Min(0.01f)] private float minZoom = 2f;
    [SerializeField, Min(0.01f)] private float maxZoom = 15f;
    [SerializeField, Min(0f)] private float mouseWheelZoomSensitivity = 0.01f;
    [SerializeField, Min(0f)] private float pinchZoomSensitivity = 0.01f;

    [Header("Smoothing")]
    [SerializeField, Min(0f)] private float smoothness = 0f;

    [Header("Focus")]
    [SerializeField, Min(0.01f)] private float focusSmoothness = 0.3f;

    private Camera _camera;
    private Vector3 _targetPosition;
    private float _targetZoom;

    private bool _isDraggingMouse;
    private Vector2 _lastMousePosition;
    private bool _isFocusMoveActive;

    private void Awake()
    {
        _global = this;
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        _targetPosition = transform.position;
        _targetZoom = GetCurrentZoom();
        _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);
    }

    private void OnDestroy()
    {
        if (_global == this)
        {
            _global = null;
        }
    }

    public static bool TryGetGlobal(out CameraController controller)
    {
        if (_global == null)
        {
            _global = FindAnyObjectByType<CameraController>(FindObjectsInactive.Include);
        }

        controller = _global;
        return controller != null;
    }

    public void FocusOnWorldPosition(Vector3 worldPosition)
    {
        _targetPosition = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
        _isFocusMoveActive = true;
    }

    private void Update()
    {
        HandleMousePan();
        HandleZoom();
        ApplyCameraState();
    }

    private void HandleMousePan()
    {
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            _isDraggingMouse = true;
            _lastMousePosition = mouse.position.ReadValue();
        }

        if (_isDraggingMouse && mouse.leftButton.isPressed)
        {
            Vector2 currentPosition = mouse.position.ReadValue();
            Vector2 delta = currentPosition - _lastMousePosition;
            _lastMousePosition = currentPosition;

            PanByScreenDelta(delta);
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            _isDraggingMouse = false;
        }
    }

    private void PanByScreenDelta(Vector2 screenDelta)
    {
        if (screenDelta.sqrMagnitude <= Mathf.Epsilon || _camera == null)
        {
            return;
        }

        float worldUnitsPerPixel = (_targetZoom * 2f) / Mathf.Max(1f, Screen.height);
        Vector3 worldDelta = (-transform.right * screenDelta.x - transform.up * screenDelta.y) * worldUnitsPerPixel;
        _targetPosition += worldDelta;
    }

    private void HandleZoom()
    {
        var mouse = Mouse.current;
        if (mouse != null)
        {
            float wheelZoomDelta = mouse.scroll.ReadValue().y * mouseWheelZoomSensitivity;
            if (Mathf.Abs(wheelZoomDelta) > Mathf.Epsilon)
            {
                ApplyZoomDelta(wheelZoomDelta, mouse.position.ReadValue());
            }
        }

        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            TouchControl touch0 = touchscreen.touches[0];
            TouchControl touch1 = touchscreen.touches[1];

            if (touch0.press.isPressed && touch1.press.isPressed)
            {
                Vector2 t0Pos = touch0.position.ReadValue();
                Vector2 t1Pos = touch1.position.ReadValue();
                Vector2 t0PrevPos = t0Pos - touch0.delta.ReadValue();
                Vector2 t1PrevPos = t1Pos - touch1.delta.ReadValue();

                float previousDistance = Vector2.Distance(t0PrevPos, t1PrevPos);
                float currentDistance = Vector2.Distance(t0Pos, t1Pos);
                float pinchDelta = currentDistance - previousDistance;
                float pinchZoomDelta = pinchDelta * pinchZoomSensitivity;
                if (Mathf.Abs(pinchZoomDelta) > Mathf.Epsilon)
                {
                    Vector2 pinchCenter = (t0Pos + t1Pos) * 0.5f;
                    ApplyZoomDelta(pinchZoomDelta, pinchCenter);
                }
            }
        }
    }

    private void ApplyZoomDelta(float zoomDelta, Vector2 screenAnchor)
    {
        float previousZoom = _targetZoom;
        float newZoom = Mathf.Clamp(previousZoom - zoomDelta, minZoom, maxZoom);
        if (Mathf.Approximately(previousZoom, newZoom))
        {
            return;
        }

        Vector3 worldBefore = ScreenToWorldOnCameraPlane(screenAnchor, previousZoom, _targetPosition);
        Vector3 worldAfter = ScreenToWorldOnCameraPlane(screenAnchor, newZoom, _targetPosition);

        _targetPosition += worldBefore - worldAfter;
        _targetZoom = newZoom;
    }

    private Vector3 ScreenToWorldOnCameraPlane(Vector2 screenPosition, float zoom, Vector3 cameraPosition)
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 pixelOffset = screenPosition - screenCenter;
        float worldUnitsPerPixel = (zoom * 2f) / Mathf.Max(1f, Screen.height);

        return cameraPosition
            + transform.right * (pixelOffset.x * worldUnitsPerPixel)
            + transform.up * (pixelOffset.y * worldUnitsPerPixel);
    }

    private void ApplyCameraState()
    {
        if (_camera == null)
        {
            return;
        }

        float activeSmoothness = _isFocusMoveActive ? Mathf.Max(smoothness, focusSmoothness) : smoothness;
        if (activeSmoothness <= 0f)
        {
            transform.position = _targetPosition;
            SetCurrentZoom(_targetZoom);
            _isFocusMoveActive = false;
            return;
        }

        float t = 1f - Mathf.Exp(-Time.unscaledDeltaTime / Mathf.Max(0.0001f, activeSmoothness));
        transform.position = Vector3.Lerp(transform.position, _targetPosition, t);
        SetCurrentZoom(Mathf.Lerp(GetCurrentZoom(), _targetZoom, t));

        if (_isFocusMoveActive && Vector3.Distance(transform.position, _targetPosition) <= 0.05f)
        {
            _isFocusMoveActive = false;
        }
    }

    private float GetCurrentZoom()
    {
        return _camera != null && _camera.orthographic ? _camera.orthographicSize : 0f;
    }

    private void SetCurrentZoom(float value)
    {
        if (_camera != null && _camera.orthographic)
        {
            _camera.orthographicSize = value;
        }
    }

    private void OnValidate()
    {
        if (maxZoom < minZoom)
        {
            maxZoom = minZoom;
        }

        if (_camera == null)
        {
            _camera = GetComponent<Camera>();
        }

        if (_camera != null && _camera.orthographic)
        {
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, minZoom, maxZoom);
        }
    }
}
