using System;
using System.Collections.Generic;
using System.Threading.Tasks;

//---------------------- PROGRAMMER LOG ----------------------//
//Created by: Truong, 09 Apr 2025
namespace SweetSoft.QLDA.Core.ExcelManager
{
    public class ReportJobRepository
    {
        private readonly string _connectionString;

        public ReportJobRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<ReportJobModel> GetJobAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ReportJobModel>> GetPendingJobsAsync()
        {
            throw new NotImplementedException();
        }


        public async Task InsertJobAsync(ReportJobModel job)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateProgressAsync(Guid jobId, int progress, int total)
        {
            throw new NotImplementedException();
        }

        public async Task MarkStartedAsync(Guid jobId)
        {
            throw new NotImplementedException();
        }

        public async Task MarkSuccessAsync(Guid jobId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set Status = "Failed", Error, FinishedAt
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task MarkFailedAsync(Guid jobId, string error)
        {
            throw new NotImplementedException();
        }

        public async Task MarkCanceledAsync(Guid jobId)
        {
            throw new NotImplementedException();
        }
    }

}
