using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    Vector3 lastPos;
    // Update is called once per frame
    void Update()
    {
        registerCameraDrag();
    }

    public void registerCameraDrag()
    {
        //Current world mouse position
        Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //True if drag was made on an android device
        bool mobileDrag = Application.platform == RuntimePlatform.Android && 
            Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved;

        //True if drag was made from unity editor
        bool editorDrag = Application.platform == RuntimePlatform.WindowsEditor && Input.GetMouseButton(0);

        if (editorDrag || mobileDrag)
            Camera.main.transform.Translate(lastPos - mousepos);

        lastPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
       