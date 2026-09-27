using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLKS.Areas.Admin.Models
{
    public class KhachHang
    {
        public String hoten { get; set; }
        public String socccd { get; set; }
        public String tuoi { get; set; }
        public String sodt { get; set; }
        public KhachHang()
        {
        }
        public KhachHang(String hoten, String tuoi)
        {
            this.hoten = hoten;
            this.tuoi = tuoi;
        }
        public KhachHang(String hoten,String socccd, String tuoi,String sodt)
        {
            this.hoten = hoten;
            this.socccd = socccd;
            this.tuoi = tuoi;
            this.sodt = sodt;
        }
    }
    
}