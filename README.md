<h3 align="center">VR Smooth Camera Plugin for Unity</h3>

<p align="center">
  Simple camera smoothing prefab to help facilitate the in-editor capture of VR content in Unity.
</p>

## Philosphy

The movement of a VR headset on a persons head is often quick and jittery. (gif of unsmoothed footage) This works as intended in VR because it is close to the exact movements of the users head, but when footage is captured and viewed on a flat screen it can read as janky or cheap. This is due to a couple factors: 1. Smoother footage is easier to watch and comprehend detail. 2. We're building on a long history of cinema where expensive film cameras were very heavy and even handheld movement had a certain level of smoothness. Stabilization techniques are common among filmmakers looking to create cinematic shots that emulate the look of a heavy camera.

Some might think that stabilizing your VR camera would not be a true representation of the gameplay, but I would argue that a straight capture to flat screen is not representative either, and the smoothing helps to bridge the gap between what you see on flat screen vs what it feels like in the VR experience.

In the end the goal is to achieve clearer, more cinematic footage for flat screen sharing of VR content.

## How it Works
- The package includes a camera with the VRSmoothCam script that follows your preferred target. This allows you to go through motions normally while smoothing is applied to the VRSmoothCam only.
- Optionally, you may enable a window in your VR perspective that allows you to monitor the view of the VRSmoothCam.
- As opposed to a continuous lerp this uses a dampened spring method for even smoother smoothness.

## Instructions
- Import the UnityPackage (created in 2019.4) 
- Drop the VRSmoothCam prefab into your scene.
- Attach your VR camera to the camera target in the VRSmoothCam script.
- Make sure the "Depth" field of the VRSmoothCam camera is lower than your VR perspective cameras.

## Things to Note
- This renders an extra camera simultaneously and it will affect performance.
- Consider using Unity Recorder to achieve exact framerates.
- This project is not yet intended for runtime use.

## Credits
The idea for smoothing comes from Valve's <a href="https://support.steampowered.com/kb_article.php?ref=1367-QDNM-8600">full in-camera smoothing</a> they used for the <a href="https://www.youtube.com/watch?v=O2W0N3uKXmo">trailer of Half-Life: Alyx</a>.

The original smoothing code comes from Digital Salmon's implementation of Ryan Juckett's Simple Harmonic Motion.
