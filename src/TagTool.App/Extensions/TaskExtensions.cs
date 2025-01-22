namespace TagTool.App.Core.Extensions;

public static class TaskExtensions
{
    public static Task<bool> RunSafe(Func<Task> work)
    {
        var tcs = new TaskCompletionSource<bool>();
        Task.Run(async () =>
        {
            try
            {
                await work();

                tcs.SetResult(true);
            }
            catch (Exception)
            {
                tcs.SetResult(false);
            }
        });

        return tcs.Task;
    }
}
