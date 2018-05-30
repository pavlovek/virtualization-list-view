using System;

namespace SamplesBasicDto
{
    public abstract class HttpResponce : ICloneable
    {
        public int Id { get; set; }

        public DateTime DetectTime { get; set; }

        public MimeTypes MimeType { get; set; }

        public int Size { get; set; }


        public override bool Equals(object obj)
        {
            var otherHttp = obj as HttpResponce;
            if (otherHttp == null)
                return false;

            if (otherHttp.Id != Id
                || otherHttp.DetectTime != DetectTime
                || otherHttp.MimeType != MimeType)
                return false;
            return true;
        }

        public abstract object Clone();
    }
}
