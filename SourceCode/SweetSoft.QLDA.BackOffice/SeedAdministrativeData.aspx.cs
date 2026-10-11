using SweetSoft.QLDA.BackOffice.Data.Administrative;
using System;

namespace SweetSoft.QLDA.BackOffice
{
    public partial class SeedAdministrativeData : System.Web.UI.Page
    {
        protected void btnSeed_Click(object sender, EventArgs e)
        {
            try
            {
                AdministrativeDataSeeder.Seed();

                lblResult.Text =
                    "Seed dữ liệu thành công!";
            }
            catch (Exception ex)
            {
                lblResult.Text =
                    "Seed thất bại: " + ex.Message;
            }
        }
    }
}