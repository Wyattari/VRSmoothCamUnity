using System;
using UnityEngine;
using UnityEditor.Recorder.FrameCapturer;

namespace UnityEditor.Recorder
{
    class GIFRecorder : GenericRecorder<GIFRecorderSettings>
    {
        fcAPI.fcGifContext m_ctx;
        fcAPI.fcStream m_stream;

        protected internal override bool BeginRecording(RecordingSession session)
        {
            if (!base.BeginRecording(session)) { return false; }

            Settings.fileNameGenerator.CreateDirectory(session);

            return true;
        }

        protected internal override void EndRecording(RecordingSession session)
        {
            // Case REC-98 crash gif animation when start/stop recording in playmode
            // If you start recording while in playmode pause the RecordFrame
            // will never be called and m_ctx and m_stream will de-reference null
            // pointers that will crash unity.
            if (m_ctx)
            {
                m_ctx.Release();
            }

            if (m_stream)
            {
                m_stream.Release();
            }

            base.EndRecording(session);
        }

        protected internal override void RecordFrame(RecordingSession session)
        {
            if (m_Inputs.Count != 1)
                throw new Exception("Unsupported number of sources");

            var input = (BaseRenderTextureInput)m_Inputs[0];
            var frame = input.OutputRenderTexture;

            if (!m_ctx)
            {
                var gifSettings = Settings.gifEncoderSettings;
                gifSettings.width = frame.width;
                gifSettings.height = frame.height;
                m_ctx = fcAPI.fcGifCreateContext(ref gifSettings);
                var path = Settings.fileNameGenerator.BuildAbsolutePath(session);
                m_stream = fcAPI.fcCreateFileStream(path);
                fcAPI.fcGifAddOutputStream(m_ctx, m_stream);
            }

            fcAPI.fcLock(frame, TextureFormat.RGB24, (data, fmt) =>
            {
                fcAPI.fcGifAddFramePixels(m_ctx, data, fmt, session.recorderTime);
            });
        }
    }
}
