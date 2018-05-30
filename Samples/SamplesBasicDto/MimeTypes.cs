using System.ComponentModel;

namespace SamplesBasicDto
{
    public enum MimeTypes
    {
        [Description("text/html")]
        TextHtml = 0,

        [Description("text/css")]
        TextCss = 1,

        [Description("image/png")]
        ImagePng = 2,

        [Description("text/jpeg")]
        ImageJpeg = 3,

        [Description("stream/video")]
        Video = 4
    }
}
