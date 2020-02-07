using UnityEngine.EventSystems;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.transform.position = new Vector3(0.0f,0.0f,-40.0f);
    }

    Vector3 lastDragPos; //Last drag position
    float zoomSpeed = 0.5f;

    void Update()
    {
        if (CameraController.isMouseOverUI() || MapController.commandMode)
            //Ignore any camera movement input if the user is touching the UI or 
            //in command mode
            return;

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

        bool mobileDrag = Application.platform == RuntimePlatform.Android &&  //Current platform is android?
            Input.touchCount == 1 &&//One finger is touching the screen?
            Input.GetTouch(0).phase == TouchPhase.Moved; // The finger on the screen was displaced?

        bool editorDrag = Application.platform == RuntimePlatform.WindowsEditor && //Current platform is windows?
            Input.GetMouseButton(0); //Left mouse button is clicked?

        if (editorDrag || mobileDrag) //If any drag was registered
        {
            Vector3 displacement = lastDragPos - mousepos; //Calculate drag vector

            displacement.z = 0.0f; //Avoid any z direction movement caused by drag

            Camera.main.transform.position += displacement;
        }
        
        lastDragPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void registerCameraZoom()
    {
        bool editorZoom = Application.platform == RuntimePlatform.WindowsEditor && //Current platform is windows?
            Input.mouseScrollDelta.y != 0; //Scrool wheel displacement is not zero

        bool mobileZoom = Input.touchCount == 2; //Two fingers touching the sreen?

        if (editorZoom)
        {
            float zoomDelta = Input.mouseScrollDelta.y * 0.5f; //Get the discpacement vector
            //Clamp zoom to a range from 4 to 10
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoomDelta, 4,100);
            return;
        }

        if (mobileZoom)
        {
            Touch finger0Position = Input.GetTouch(0);
            Touch finger1Position = Input.GetTouch(1);

            //Get finger positions prior to displacement
            Vector3 touch0LastPos = finger0Position.position - finger0Position.deltaPosition;
            Vector3 touch1LastPos = finger1Position.position - finger1Position.deltaPosition;

            //Find the distances between the fingers before and after displacement
            float prevDist = (touch0LastPos - touch1LastPos).magnitude;
            float currentDist = (finger0Position.position - finger1Position.position).magnitude;
            
            //Change zoom value proportional to the change in distance between fingers
            float zoomDelta = (currentDist - prevDist)*zoomSpeed;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoomDelta, 4, 10);
        }
    }

    public static bool isMouseOverUI()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        else
         return EventSystem.current.IsPointerOverGameObject();
    }
}
       