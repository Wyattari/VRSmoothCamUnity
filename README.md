<h1 align="center">VR Smooth Camera Plugin for Unity</h3>

<p align="center">
  Simple camera smoothing tool to help facilitate the in-editor capture of VR content in Unity.
</p>

Capturing movement in a VR headset often results in jittery footage because of the natural movement of a head—which is fine for the person wearing the headset, but not great for the person viewing it on a flat screen.

![](01_NoSmoothing.gif)

<sup>(no smoothing)</sup>


Smoothing that movement is a little easier on the eyes.

![](02_SmoothDamp.gif)

<sup>(SmoothDamp position & rotation)</sup>


The VR Smooth Camera is a simple in-editor tool that allows you to achieve clearer, more cinematic footage for flat screen sharing of VR content. This also allows you to adjust camera settings without affecting the player's perspective, so in-game hands won’t be cropped out of 16:9 captured footage.

![](03_SmoothDamp_WiderFOV_LockedZ.gif)

<sup>(SmoothDamp position & rotation, wider FOV, locked camera roll)</sup>

  
The VR Smooth Camera helps bridge the gap between what you see on flat screen vs what you experience in VR.

## How it Works
- The package includes a prefab with a camera and smoothing script that follows your preferred target. This allows you to go through motions normally while smoothing is applied to only the separate smoothed camera.
- You can also enable a window in your VR perspective that allows you to monitor the view of the smoothed camera and make sure you get your framing just right.


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
- Currently only uses SmoothDamp, but it's set up to experiment with additional smoothing methods.
- The "Lock Camera Roll" option currently bugs out when you look straight up or down.

## Credits
The idea for smoothing comes from Valve's <a href="https://support.steampowered.com/kb_article.php?ref=1367-QDNM-8600">full in-camera smoothing</a> they used for the <a href="https://www.youtube.com/watch?v=O2W0N3uKXmo">trailer of Half-Life: Alyx</a>.

If this has been helpful,
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/W7W64VWMO)

May your framerates be high and your cameras smooth!
