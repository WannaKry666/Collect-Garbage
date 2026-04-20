using GarbageCollection.Common.DTOs.Complaint;

namespace GarbageCollection.Business.Interfaces
{
    public interface IComplaintService
    {
        Task<ComplaintResponseDto> CreateComplaintAsync(int citizenId, int reportId, CreateComplaintDto dto);
        Task SendMessageAsync(int citizenId, int reportId, int complaintId, SendComplaintMessageDto dto);
        Task<ComplaintResponseDto> GetComplaintAsync(int citizenId, int reportId, int complaintId);
        Task<ComplaintsListResult> GetComplaintsByReportAsync(int citizenId, int reportId, int page, int limit);
    }
}
