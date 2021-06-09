<h1 align="center">VR Smooth Camera Plugin for Unity</h3>

<p align="center">
  Simple camera smoothing prefab to help facilitate the in-editor capture of VR content in Unity.
</p>

## Uhhh, Why?

The movement of a VR headset on a persons head is often jittery. 

![](01_NoSmoothing.gif)

<sup>(no smoothing)</sup>


If you smooth it out, it looks more cinematic.

![](02_SmoothDamp.gif)

<sup>(SmoothDamp position & rotation)</sup>


Smoothing helps to bridge the gap between what you see on flat screen vs what it feels like in the VR experience.

Using a separate capture camera also allows you to adjust camera settings without affecting the player's perspective. This is useful if your hands that you can see in-game are often cropped out of the 16:9 captured footage.

![](03_SmoothDamp_WiderFOV_LockedZ.gif)

<sup>(SmoothDamp position & rotation, wider FOV, locked camera roll)</sup>

  
In the end the goal is to have a simple in-editor tool that allows you to achieve clearer, more cinematic footage for flat screen sharing of VR content.

## How it Works
- The package includes a prefab with a camera and smoothing script that follows your preferred target. This allows you to go through motions normally while smoothing is applied to the separate smoothed camera only.
- Optionally, you may enable a window in your VR perspective that allows you to monitor the view of the smoothed camera. I find this helpful if you're trying to frame things in really specific ways.


![](04_Monitor.gif)


## Instructions
- Import the UnityPackage (created in 2019.4) 
- Drop the SmoothCameraFollow prefab into your scene.
- Attach your VR camera to the follow target field in the SmoothCamFollow script.
- Make sure the "Depth" or "Priority" field of the VRSmoothCam camera is set so it has higher priority than your VR camera.
- Use the SmoothCamSettings ScriptableObject to adjust settings.
- If you want to use the monitor you'll need to create or pick a layer that you can remove from the culling mask on the smoothed camera. Then open the SmoothCameraMonitor prefab and update the MonitorQuad's layer to the culled one.

## Things to Note
- This renders an extra camera simultaneously and it will affect performance.
- Not intended for Mixed Reality Capture.
- Not yet intended for runtime use.
- Currently only set up with SmoothDamp, but I'd like to experiment with additional smoothing methods.
- The "Lock Camera Roll" option bugs out when you look straight up or down.

## Credits
The idea for smoothing comes from Valve's <a href="https://support.steampowered.com/kb_article.php?ref=1367-QDNM-8600">full in-camera smoothing</a> they used for the <a href="https://www.youtube.com/watch?v=O2W0N3uKXmo">trailer of Half-Life: Alyx</a>.

If this has been helpful,
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/W7W64VWMO)

May your framerates be high and your cameras smooth!
