using UnityEngine;

public class Move
{
    private GameObject _ring;

    public GameObject Ring
    {
        get { return _ring; }
        set
        {
            if (value != null) _ring = value;
            else Debug.LogWarning("Tried to assign null to the Ring!");
        }
    }

    private Vector3 _futurePosition;

    public Vector3 FuturePosition
    {
        get { return _futurePosition; }
        set
        {
            if (value != null) _futurePosition = value;
            else Debug.LogWarning("Tried to assign null to the Future Position!");
        }
    }

    public Move(GameObject ring, Vector3 futurePosition)
    {
        Ring = ring;
        FuturePosition = futurePosition;
    }
}