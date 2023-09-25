using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamController : MonoBehaviour
{
    public float zoomSpeed = 10f;
    public float closeCursorSpeed = 20f;
    public float zoomSensibility = 10f;
    public float camSpeed = 5f;
    public float camSensitivity = 3f;
    private float delayTime = 0.1f;
    private float delayCount = 0f;

    [SerializeField]
    private Camera _cam;
    [SerializeField]
    private BoardController _board;
    private float maximumZoom;
    private float minimumZoom;
    private float desiredZoom;
    private Vector3 desiredPos;
    private Vector3 defaultCameraPos;
    private Vector3 touchStart;

    private Vector2 limitBottomCorner;
    private Vector2 limitUpperCorner;

    // Start is called before the first frame update
    void Start()
    {
        SetBoardCamera();
    }

    public void SetBoardCamera()
    {
        maximumZoom = 3.5f;
        minimumZoom = ScalingZoomFunction(_board.size);
        if(minimumZoom < maximumZoom)
            maximumZoom = minimumZoom;

        limitBottomCorner = new Vector2(2f, 2f);
        float limit = _board.size - 1f;
        limitUpperCorner = new Vector2(limit, limit);

        desiredZoom = minimumZoom;
        _cam.orthographicSize = minimumZoom;
        desiredPos = _cam.transform.position;
        defaultCameraPos = desiredPos;

        _board.cursor.transform.position = new Vector3(desiredPos.x, desiredPos.y, _board.cursor.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && delayCount <= 0)
        {
            if(!_board.IsPointerOverUIObject())
            {
                touchStart = MousePoint();
            }
            else
            {
                touchStart = Vector3.zero;
            }
        }

        if (Input.touchCount == 2 && touchStart != Vector3.zero) // PINCH ZOOM
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            Zoom(difference * 0.001f);
            _board.downTile = null;

            delayCount = delayTime;
        }
        else if (Input.GetMouseButton(0) && touchStart != Vector3.zero && delayCount <= 0) // MOVE
        {
            Vector3 direction = touchStart - MousePoint();

            if(direction.magnitude > 0.1f)
            {
                if(direction.x > 0 && desiredPos.x >= limitUpperCorner.x)
                {
                    direction.x = 0;
                    touchStart = MousePoint();
                }
                if(direction.y > 0 && desiredPos.y >= limitUpperCorner.y)
                {
                    direction.y = 0;
                    touchStart = MousePoint();
                }
                if(direction.x < 0 && desiredPos.x <= limitBottomCorner.x)
                {
                    direction.x = 0;
                    touchStart = MousePoint();
                }
                if(direction.y < 0 && desiredPos.y <= limitBottomCorner.y)
                {
                    direction.y = 0;
                    touchStart = MousePoint();
                }

                desiredPos += direction * camSensitivity;

                _board.downTile = null;
            }

            touchStart = MousePoint();
        }
        if(!_board.IsPointerOverUIObject())
        {
            Zoom(Input.GetAxis("Mouse ScrollWheel"));
        }

        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, desiredZoom, Time.deltaTime * zoomSpeed);
        _cam.transform.position = Vector3.Lerp(_cam.transform.position, desiredPos, Time.deltaTime * camSpeed);

        if(delayCount > 0)
        {
            delayCount -= Time.deltaTime;
            touchStart = MousePoint();
        }
    }

    private Vector3 MousePoint()
    {
        return _cam.ScreenToWorldPoint(Input.mousePosition);
    }

    private void Zoom(float increment)
    {
        desiredZoom = Mathf.Clamp(desiredZoom - (increment * Time.deltaTime * zoomSensibility), maximumZoom, minimumZoom);
        
        if(increment > 0.001f)
        {
            Vector3 cursorPos = new Vector3(_board.cursor.transform.position.x, _board.cursor.transform.position.y, desiredPos.z);
            desiredPos = Vector3.Lerp(desiredPos, cursorPos, Time.deltaTime * closeCursorSpeed);
        }
        else if (increment < -0.001f)
        {
            desiredPos = Vector3.Lerp(desiredPos, defaultCameraPos, Time.deltaTime * closeCursorSpeed);
        }

        if(desiredZoom == minimumZoom)
            desiredPos = defaultCameraPos;
    }

    private float ScalingZoomFunction(int x)
    {
        return (x / 2f) + 1;
    }
}
