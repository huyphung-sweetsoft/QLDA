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
        OvercomeObstacle 
    }
}
