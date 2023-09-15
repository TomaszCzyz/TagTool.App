namespace TagTool.App.Core.Models.Exceptions;

public class ImageLoadingException : Exception
{
    public ImageLoadingException()
    {
    }

    public ImageLoadingException(string message)
        : base(message)
    {
    }

    public ImageLoadingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
