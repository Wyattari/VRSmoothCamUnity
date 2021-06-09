<h1 align="center">VR Smooth Camera Plugin for Unity</h3>

<p align="center">
  Simple camera smoothing prefab to help facilitate the in-editor capture of VR content in Unity.
</p>

## Uhhh, Why?

The movement of a VR headset on a persons head is often quick and jittery. 


![](01_NoSmoothing.gif)

(no smoothing)

  
This works as intended in VR because it is close to the exact movements of the users head, but when footage is captured and viewed on a flat screen it can read as janky or cheap. This is due to a couple factors: 1. Smoother footage is easier to watch and comprehend detail. 2. We're building on a long history of cinema where expensive film cameras were very heavy and even handheld movement had a certain level of smoothness. Stabilization techniques are common among filmmakers looking to create cinematic shots that emulate the look of a heavy camera.


![](02_SmoothDamp.gif)

(SmoothDamp position & rotation)


Some might think that stabilizing your VR camera would not be a true representation of the gameplay, but I would argue that a straight capture to flat screen is not representative either, and the smoothing helps to bridge the gap between what you see on flat screen vs what it feels like in the VR experience.

Using a separate capture camera also allows you to adjust camera settings without affecting the player's perspective. For example, since game capture is only recording one eye, you can increase the smoothed camera's field of view to better match what the player sees with both eyes. Also useful if your hands that you can see in-game are often cropped out of the 16:9 captured footage.

![](03_SmoothDamp_WiderFOV_LockedZ.gif)

(SmoothDamp position & rotation, wider FOV, locked camera roll)

  
In the end the goal is to have a simple in-editor tool that allows you to achieve clearer, more cinematic footage for flat screen sharing of VR content.

## How it Works
- The package includes a prefab with a camera and smoothing script that follows your preferred target. This allows you to go through motions normally while smoothing is applied to the separate smoothed camera only.
- Optionally, you may enable a window in your VR perspective that allows you to monitor the view of the smoothed camera.


![](04_Monitor.gif)


## Instructions
- Import the UnityPackage (created in 2019.4) 
- Drop the SmoothCameraFollow prefab into your scene.
- Attach your VR camera to the follow target field in the SmoothCamFollow script.
- Make sure the "Depth" or "Priority" field of the VRSmoothCam camera is set so it has higher priority than your VR camera.
- If you want to use the monitor you'll need to create or pick a layer that you can remove from the culling mask on the smoothed camera. Then open the SmoothCameraMonitor prefab and update the MonitorQuad's layer to the culled one.

## Things to Note
- This renders an extra camera simultaneously and it will affect performance.
- Not intended for Mixed Reality Capture.
- Not yet intended for runtime use.
- Currently only set up with SmoothDamp, but I'd like to experiment with additional smoothing methods.

## Credits
The idea for smoothing comes from Valve's <a href="https://support.steampowered.com/kb_article.php?ref=1367-QDNM-8600">full in-camera smoothing</a> they used for the <a href="https://www.youtube.com/watch?v=O2W0N3uKXmo">trailer of Half-Life: Alyx</a>.
