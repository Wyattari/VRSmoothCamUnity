using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.InputSystem;

public class RecordAnimation : MonoBehaviour
{
    public AnimationClip clip;
    bool recording = false;
    GameObjectRecorder recorder;


    void Start()
    {
        recorder = new GameObjectRecorder(gameObject);
        recorder.BindComponentsOfType<Transform>(gameObject, false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.isPressed)
        {
            if (!recording)
            {
                recording = true;
            }
            else
            {   
                recording = false;
                //stop recording & save thing
                if (clip != null)
                {
                    recorder.SaveToClip(clip);
                }
            }
        }
    }

    private void LateUpdate()
    {
        if(clip == null)
            return;

        recorder.TakeSnapshot(Time.deltaTime);
    }
}
