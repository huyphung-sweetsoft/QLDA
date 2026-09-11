using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class ThanhVienDuAnRepository : BaseRepository<TblThanhVienDuAn> {
        public ThanhVienDuAnRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public TblThanhVienDuAn Insert(TblThanhVienDuAn item, string description = null)
        {
            Guid id = Guid.Parse(item.GetColumnValue("IdThanhVienDuAn").ToString());
            Guid idDuAn = Guid.Parse(item.GetColumnValue("IdDuAn").ToString());
            item.Save();
            Task.Run(async () => {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, id, item.NguoiTao, idDuAn, GetNhanVienDisplayName(item.IdNhanVien.Value), description).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblDuAn");
                }
            });
            return item;
        }

        public TblThanhVienDuAn Update(TblThanhVienDuAn thanhVienDuAn, string description = null)
        {
            Guid id = Guid.Parse(thanhVienDuAn.GetColumnValue("IdThanhVienDuAn").ToString());
            Guid idDuAn = Guid.Parse(thanhVienDuAn.GetColumnValue("IdDuAn").ToString());
            TblThanhVienDuAn itemOld = GetById(id);
            thanhVienDuAn.Save();
            string updatedBy = string.Empty;
            try
            {
                updatedBy = thanhVienDuAn.GetColumnValue("NguoiCapNhat")?.ToString();
            }
            catch { }
            Task.Run(async () => {
                try
                {
                    await _auditManager.LogChangesAsync(itemOld, thanhVienDuAn, _tableName, id, updatedBy, idDuAn, GetNhanVienDisplayName(thanhVienDuAn.IdNhanVien.Value), description).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for TblDuAn");
                }
            });
            return thanhVienDuAn;
        }

        public override TblThanhVienDuAn GetById(Guid id)
        {
            return new Select().From(TblThanhVienDuAn.Schema).Where(TblThanhVienDuAn.IdThanhVienDuAnColumn).IsEqualTo(id).And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false).ExecuteSingle<TblThanhVienDuAn>();
        }

        public List<TblThanhVienDuAn> GetByIdDuAn(Guid idDuAn)
        {
            Select select = new Select();
            select.From(TblThanhVienDuAn.Schema);
            select.Where(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn);
            return select.ExecuteTypedList<TblThanhVienDuAn>();
        }
        //Hàm này sẽ bổ sung thêm 1 tham số và 1 điều kiện And
        //Lý do: Giả sử khi đã chọn nhân viên A làm PM, thì khi vào danh sách nhân viên để chọn đám nhân viên thêm vào dự án, //sẽ tồn tại trường hợp thk nhân viên A cũng nằm trong danh sách đó, và nếu chọn nó để thêm vào thì cái vai trò của nó sẽ bị đè lên
        //Từ nhân viên A - Pm -> Nhân viên A - nhân viên
        //Nên bổ sung thêm 1 điều kiện check vai trò, lúc này vì vai trò khác nhau nên cái hàm Get này sẽ bị rỗng, và vì bị rỗng nên nó trả về null
        //Và trả về null nên nó sẽ đẩy sang thằng thêm mới thay vì cập nhật
        //T đã cài 2 lớp để tránh việc PM sẽ rơi vào cái danh sách nhân viên được chọn rồi, nên thực sự cái này ko cần đổi
        //Trong trường hợp dở người mà bug hay gì đó thk PM vẫn bị dính vào danh sách nhân viên chọn vào dự án thì thay vì nó đè vai trò của thk PM về thành nhân viên
        //Gây lỗi dự án thì nó sẽ tạo thêm 1 dòng (thêm mới ấy) thk PM này với Vai trò là nhân viên
        //Nói chung là backup, ko ảnh hưởng gì cả
        public TblThanhVienDuAn GetNhanVienIsActiveInDuAn(Guid idNhanVien, Guid idDuAn, Guid idVaiTro)
        {
            return new Select().From(TblThanhVienDuAn.Schema).Where(TblThanhVienDuAn.IdNhanVienColumn).IsEqualTo(idNhanVien).And(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn).And(TblThanhVienDuAn.IdVaiTroDuAnColumn).IsEqualTo(idVaiTro)//dòng thêm vào.And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false).ExecuteSingle<TblThanhVienDuAn>();
        }
        //THêm mới 3 hàm sau
        //1. Hàm này dùng đến lấy danh sách dựa vào id dự án và vai trò và có ngoại lệ (except 1 đứa), //mục đích là lấy danh sách với vai trò là PM để tiến hành cho chức năng đỏi PM từ nhân viên A sang nv B
        //Mỗi dự án chỉ có 1 PM nên hàm này nếu viết dạng lấy 1 cũng được nhưng viết list cho chắc để tránh trường hợp db lỗi 
        //làm tồn tại 2 PM đang hoạt động trên cùng 1 dự án. qua Manager nói rõ hơn
        //Đổi về lấy 1 nếu muốn 
        public List<TblThanhVienDuAn> GetByIdDuAnAndVaiTroExcept(Guid idDuAn, Guid idVaiTro, Guid idNhanVienGiuLai)
        {
            return new Select().From(TblThanhVienDuAn.Schema).Where(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn).And(TblThanhVienDuAn.IdVaiTroDuAnColumn).IsEqualTo(idVaiTro).And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false).And(TblThanhVienDuAn.IdNhanVienColumn).IsNotEqualTo(idNhanVienGiuLai).ExecuteTypedList<TblThanhVienDuAn>();
        }
        //2. Hàm này lấy idNhanVien để dùng cho chức năng edit, lấy list id đó để đánh tích vô mấy cái checkbox 
        public List<Guid> GetIdNhanVienByDuAnAndVaiTro(Guid idDuAn, Guid idVaiTro)
        {
            List<TblThanhVienDuAn> list = new Select().From(TblThanhVienDuAn.Schema).Where(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn).And(TblThanhVienDuAn.IdVaiTroDuAnColumn).IsEqualTo(idVaiTro).And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false).ExecuteTypedList<TblThanhVienDuAn>();

            return list.Where(x => x.IdNhanVien.HasValue).Select(x => x.IdNhanVien.Value).ToList();
        }

        public string GetNhanVienDisplayName( Guid idNhanVien)
        {
            string sql = $@"
        DECLARE @idNhanVien UNIQUEIDENTIFIER = '{idNhanVien}';

        SELECT TOP 1
            COALESCE
            ( NULLIF( LTRIM(RTRIM(DisplayName)), N''
                ), UserName
            )
        FROM dbo.aspnet_Users
        WHERE UserId = @idNhanVien;";

            return new InlineQuery().ExecuteScalar<string>(sql);
        }
    }
}
