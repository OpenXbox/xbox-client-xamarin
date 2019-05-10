using System;
using CoreMedia;
using CoreVideo;
using VideoToolbox;

namespace xnano.iOS.Gamestream
{
    public class VideoDecoder
    {
        readonly VTDecompressionSession _decoderSession;
        readonly VTDecompressionSession.VTDecompressionOutputCallback _callback;
        public VideoDecoder()
        {
            _decoderSession = VTDecompressionSession.Create(
                HandleVTDecompressionOutputCallback,
                CMVideoFormatDescription.FromH264ParameterSets(null, 1, out CMFormatDescriptionError error),
                new VTVideoDecoderSpecification(),
                new CVPixelBufferAttributes());
        }

        void HandleVTDecompressionOutputCallback(IntPtr sourceFrame, VTStatus status, VTDecodeInfoFlags flags, CoreVideo.CVImageBuffer buffer, CoreMedia.CMTime presentationTimeStamp, CoreMedia.CMTime presentationDuration)
        {
        }

    }
}
