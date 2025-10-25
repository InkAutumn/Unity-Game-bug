using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{
    public static Vector2 GetInputPosition()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }
#endif
        return Input.mousePosition;
    }

    public static bool GetInputDown()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            return true;
        }
#endif
        return Input.GetMouseButtonDown(0);
    }

    public static bool GetInput()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            TouchPhase phase = Input.GetTouch(0).phase;
            return phase == TouchPhase.Stationary || phase == TouchPhase.Moved;
        }
#endif
        return Input.GetMouseButton(0);
    }

    public static bool GetInputUp()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            return true;
        }
#endif
        return Input.GetMouseButtonUp(0);
    }

    public static bool IsPointerOverUI()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
#endif
        return EventSystem.current.IsPointerOverGameObject();
    }
}
