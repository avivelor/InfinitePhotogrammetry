using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsMeasure : MonoBehaviour
{
    public int m_NonPerformantFrameCount;

    // we have observed a drop in performance between simulation and runtime
    // on the device - in the editor, we've seen it fluctuate from 15-60 FPS
    // when the device runs just fine (also giving a little bit of elbow room
    // for when editor tanks the frame rate a bit more than what we've seen)
    const float k_FrameTimeMax = 1f / 15f;

    public void Update()
    {
        if (Time.deltaTime > k_FrameTimeMax)
            ++m_NonPerformantFrameCount;
    }
}
