using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    Vector3 lastDragPos; //Last drag position
    float zoomSpeed = 0.005f;

    void Update()
    {
        registerCameraDrag();
        registerCameraZoom();
    }

    public void registerCameraDrag()
    {
        MapController map = GameObject.Find("MainObject").GetComponent<MapController>();
        //Get map parameters
        int mapWidth = map.MapWidth;
        int mapHeight = map.MapHeight;
        //Current world mouse position
        Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //True if drag was made on an android device
        bool mobileDrag = Application.platform == RuntimePlatform.Android && 
            Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved;

        //True if drag was made from unity editor
        bool editorDrag = Application.platform == RuntimePlatform.WindowsEditor && Input.GetMouseButton(0);

        if (editorDrag || mobileDrag)
        {
            Vector3 displacement = lastDragPos - mousepos;
            //Calculate final camera position
            Vector3 resultingPos = Camera.main.transform.position + displacement;

            float newxpos = resultingPos.x;
            float newypos = resultingPos.y;

            Vector3 newPosition = Camera.main.transform.position;

            newPosition.x = Mathf.Clamp(newxpos, 0 , mapWidth);
            newPosition.y = Mathf.Clamp(newypos, 0, mapHeight);

            Camera.main.transform.position = newPosition;
        }
        
        lastDragPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void registerCameraZoom()
    {
        if(Application.platform == RuntimePlatform.WindowsEditor && Input.mouseScrollDelta.y!=0)
        {
            float zoomDelta = Input.mouseScrollDelta.y * 0.5f;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoomDelta, 4,10);
            return;
        }

        if (Input.touchCount == 2)
        {
            Touch finger0Position = Input.GetTouch(0);
            Touch finger1Position = Input.GetTouch(1);

            Vector3 touch0LastPos = finger0Position.position - finger0Position.deltaPosition;
            Vector3 touch1LastPos = finger1Position.position - finger1Position.deltaPosition;

            float prevDist = (touch0LastPos - touch1LastPos).magnitude;
            float currentDist = (finger0Position.position - finger1Position.position).magnitude;

            float zoomDelta = (currentDist - prevDist)*zoomSpeed;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoomDelta, 4, 10);
        }
    }
}
       