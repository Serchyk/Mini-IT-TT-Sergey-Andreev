using UnityEngine;

/// <summary>
/// Simple input abstraction for PC mouse and mobile touch.
/// Use InputAdapter.GetPointerWorldPosition() in gameplay code.
/// </summary>
namespace MiniIT.CORE
{
    public static class InputAdapter
    {
        /// <summary>Return true if there is a primary pointer down this frame.</summary>
        public static bool IsPointerDown()
        {
            if (Input.touchCount > 0)
            {
                return Input.GetTouch(0).phase == TouchPhase.Began;
            }

            return Input.GetMouseButtonDown(0);
        }

        /// <summary>Return pointer world position using main camera.</summary>
        public static Vector3 GetPointerWorldPosition(Camera cam)
        {
            Vector3 screen;
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                screen = t.position;
            }
            else
            {
                screen = Input.mousePosition;
            }

            return cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, cam.nearClipPlane));
        }
    }
}