using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using SamplesBasicDto;

namespace SamplesSpecificDto
{
    public class HttpVideoResponce : HttpResponce
    {
        [NotMapped]
        public Size Resolution { get; set; }

        public int ResolutionHeight
        {
            get { return Resolution.Height; }
            set { Resolution = new Size(ResolutionWidth, value); }
        }

        public int ResolutionWidth
        {
            get { return Resolution.Width; }
            set { Resolution = new Size(value, ResolutionHeight); }
        }

        public TimeSpan Duration { get; set; }

        public string CodecType { get; set; }


        public HttpVideoResponce()
        { }

        public HttpVideoResponce(DateTime detectTime, MimeTypes mimeType, int size)
        {
            DetectTime = detectTime;
            MimeType = mimeType;
            Size = size;
        }

        public HttpVideoResponce(DateTime detectTime, MimeTypes mimeType, int size, Size resolution, TimeSpan duration, string codecType)
            : this(detectTime, mimeType, size)
        {
            Resolution = resolution;
            Duration = duration;
            CodecType = codecType;
        }


        public override object Clone()
        {
            return new HttpVideoResponce(DetectTime,
                                         MimeType,
                                         Size,
                                         Resolution,
                                         Duration,
                                         CodecType);
        }
    }
}
