namespace TagTool.App.Models.Exceptions;

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
