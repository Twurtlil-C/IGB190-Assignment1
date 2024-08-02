using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Utilities
{
    public static List<T> GetAllWithinRange<T>(Vector3 position, float maxDistance) where T : MonoBehaviour
    {
        List<T> withinRange = new List<T>();
        T[] objs = GameObject.FindObjectsOfType<T>();
        foreach (T obj in objs)
        {
            float distance = Vector3.Distance(obj.transform.position, position);
            if (distance < maxDistance)
            {
                withinRange.Add(obj);
            }
        }
        return withinRange;
    }

    public static T GetClosest<T>(Vector3 position, float maxDistance) where T : MonoBehaviour
    {
        T closest = null;
        float closestDistance = maxDistance;
        T[] objs = GameObject.FindObjectsOfType<T>();
        foreach (T obj in objs)
        {
            float distance = Vector3.Distance(obj.transform.position, position);
            if (distance < closestDistance)
            {
                closest = obj;
                closestDistance = distance;
            }
        }
        return closest;
    }

    public static Vector3 GetMouseWorldPosition ()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }
        return Vector3.positiveInfinity;
    }
}
