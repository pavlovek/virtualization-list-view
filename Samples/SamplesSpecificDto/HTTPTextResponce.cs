using System;
using SamplesBasicDto;

namespace SamplesSpecificDto
{
    public class HttpTextResponce : HttpResponce
    {
        public string EncodingType { get; set; }


        public HttpTextResponce()
        { }

        public HttpTextResponce(DateTime detectTime, MimeTypes mimeType, int size)
        {
            DetectTime = detectTime;
            MimeType = mimeType;
            Size = size;
        }

        public HttpTextResponce(DateTime detectTime, MimeTypes mimeType, int size, string encodingType)
            : this(detectTime, mimeType, size)
        {
            EncodingType = encodingType;
        }


        public override object Clone()
        {
            return new HttpTextResponce(DetectTime,
                                        MimeType,
                                        Size,
                                        EncodingType);
        }
    }
}
