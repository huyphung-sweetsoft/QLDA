namespace SweetSoft.QLDA.Core.ExcelManager
{
    //public class ReportJobProcessor : BackgroundService
    //{
    //    private readonly IServiceProvider _provider;

    //    public ReportJobProcessor(IServiceProvider provider)
    //    {
    //        _provider = provider;
    //    }

    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        while (!stoppingToken.IsCancellationRequested)
    //        {
    //            using var scope = _provider.CreateScope();
    //            var repo = scope.ServiceProvider.GetRequiredService<ReportJobRepository>();
    //            var reportService = scope.ServiceProvider.GetRequiredService<ReportService>();

    //            var jobs = await repo.GetPendingJobsAsync();

    //            foreach (var job in jobs)
    //            {
    //                try
    //                {
    //                    await repo.MarkStartedAsync(job.Id);

    //                    var options = JsonConvert.DeserializeObject<ExcelExportOptions>(job.OptionsJson);
    //                    await reportService.GenerateReportToFileAsync(job.Id, job.SqlQuery, options, job.ReportFileName,
    //                        (done, total) => repo.UpdateProgressAsync(job.Id, done, total));

    //                    await reportService.SendEmailWithDownloadLinkAsync(job.EmailTo, job.ReportFileName);
    //                    await repo.MarkSuccessAsync(job.Id);
    //                }
    //                catch (Exception ex)
    //                {
    //                    await repo.MarkFailedAsync(job.Id, ex.ToString());
    //                }
    //            }

    //            await Task.Delay(5000, stoppingToken); 
    //        }
    //    }
    //}

}
