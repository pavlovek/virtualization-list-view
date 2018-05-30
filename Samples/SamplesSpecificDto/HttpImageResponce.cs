using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using SamplesBasicDto;

namespace SamplesSpecificDto
{
    public class HttpImageResponce : HttpResponce
    {
        [NotMapped]
        public Size ImageSize { get; set; }

        public int ImageSizeHeight
        {
            get { return ImageSize.Height; }
            set { ImageSize = new Size(ImageSizeWidth, value); }
        }

        public int ImageSizeWidth
        {
            get { return ImageSize.Width; }
            set { ImageSize = new Size(value, ImageSizeHeight); }
        }

        public int ColorDepth { get; set; }


        public HttpImageResponce()
        { }

        public HttpImageResponce(DateTime detectTime, MimeTypes mimeType, int size)
        {
            DetectTime = detectTime;
            MimeType = mimeType;
            Size = size;
        }

        public HttpImageResponce(DateTime detectTime, MimeTypes mimeType, int size, Size imageSize, int colorDepth)
            : this(detectTime, mimeType, size)
        {
            ImageSize = imageSize;
            ColorDepth = colorDepth;
        }


        public override object Clone()
        {
            return new HttpImageResponce(DetectTime,
                                         MimeType,
                                         Size,
                                         ImageSize,
                                         ColorDepth);
        }
    }
}
