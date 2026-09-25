using SweetSoft.QLDA.Core.EnumHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.FileManager
{
    public enum FileUploadTypes
    {
        [ERender("Tệp đính kèm email")]
        AttachmentsEmail,
        [ERender("Ảnh đại diện người dùng")]
        UserAvatar,
        [ERender("Ảnh của chướng ngại vật")]
        OvercomeObstacle,
        [ERender("Mẫu tài liệu")]
        DocumentTemplate,
        [ERender("Phiên bản tài liệu")]
        DocumentVersion,
        [ERender("Tệp kết quả ký tài liệu")]
        DocumentSigningResult,
        [ERender("Tệp đính kèm chi phí")]
        CostAttachment,
        [ERender("Tệp đính kèm lịch họp")]
        MeetingAttachment,
        [ERender("Tệp hợp đồng thực hiện")]
        ProjectContract,
        [ERender("Tệp PDF hợp đồng thực hiện")]
        ProjectContractPdf
    }
}
