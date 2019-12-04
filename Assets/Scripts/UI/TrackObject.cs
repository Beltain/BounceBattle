using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackObject : MonoBehaviour
{
    public Transform worldObjectToTrack;

    private void Awake()
    {
        if (worldObjectToTrack != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(worldObjectToTrack.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (worldObjectToTrack != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(worldObjectToTrack.position);
        }
    }


}
