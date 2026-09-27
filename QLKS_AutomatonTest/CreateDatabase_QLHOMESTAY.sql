/*==============================================================*/
/* Database: QUẢN LÝ KHÁCH SẠN / HOMESTAY                      */
/* Script tạo cơ sở dữ liệu SQL Server                          */
/* Tạo ngày: 16/10/2025                                         */
/*==============================================================*/

USE master;
GO

-- Drop database nếu đã tồn tại
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'dataQLKS')
BEGIN
    ALTER DATABASE dataQLKS SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE dataQLKS;
END
GO

-- Tạo database mới
CREATE DATABASE dataQLKS;
GO

USE dataQLKS;
GO

/*==============================================================*/
/* Table: tblChucVu - Bảng chức vụ nhân viên                   */
/*==============================================================*/
CREATE TABLE tblChucVu (
    ma_chuc_vu INT IDENTITY(1,1) NOT NULL,
    chuc_vu NVARCHAR(50) NULL,
    CONSTRAINT PK_tblChucVu PRIMARY KEY (ma_chuc_vu)
);
GO

/*==============================================================*/
/* Table: tblDichVu - Bảng dịch vụ                             */
/*==============================================================*/
CREATE TABLE tblDichVu (
    ma_dv INT IDENTITY(1,1) NOT NULL,
    ten_dv NVARCHAR(100) NULL,
    gia FLOAT NULL,
    don_vi NVARCHAR(30) NULL,
    anh NVARCHAR(200) NULL,
    ton_kho INT NULL,
    CONSTRAINT PK_tblDichVu PRIMARY KEY (ma_dv)
);
GO

/*==============================================================*/
/* Table: tblKhachHang - Bảng khách hàng                       */
/*==============================================================*/
CREATE TABLE tblKhachHang (
    ma_kh NVARCHAR(50) NOT NULL,
    mat_khau NVARCHAR(50) NULL,
    ho_ten NVARCHAR(50) NULL,
    cccd NVARCHAR(30) NULL,
    sdt NVARCHAR(15) NULL,
    mail NVARCHAR(100) NULL,
    diem INT NULL,
    CONSTRAINT PK_tblKhachHang PRIMARY KEY (ma_kh)
);
GO

/*==============================================================*/
/* Table: tblLoaiPhong - Bảng loại phòng                       */
/*==============================================================*/
CREATE TABLE tblLoaiPhong (
    loai_phong INT IDENTITY(1,1) NOT NULL,
    mo_ta NVARCHAR(50) NULL,
    gia FLOAT NULL,
    ti_le_phu_thu INT NULL,
    anh NVARCHAR(300) NULL,
    CONSTRAINT PK_tblLoaiPhong PRIMARY KEY (loai_phong)
);
GO

/*==============================================================*/
/* Table: tblNhanVien - Bảng nhân viên                         */
/*==============================================================*/
CREATE TABLE tblNhanVien (
    ma_nv INT IDENTITY(1,1) NOT NULL,
    ho_ten NVARCHAR(50) NULL,
    ngay_sinh DATE NULL,
    dia_chi NVARCHAR(100) NULL,
    sdt NVARCHAR(15) NULL,
    tai_khoan NVARCHAR(50) NULL,
    mat_khau NVARCHAR(50) NULL,
    ma_chuc_vu INT NULL,
    CONSTRAINT PK_tblNhanVien PRIMARY KEY (ma_nv)
);
GO

/*==============================================================*/
/* Table: tblTang - Bảng tầng                                  */
/*==============================================================*/
CREATE TABLE tblTang (
    ma_tang INT IDENTITY(1,1) NOT NULL,
    ten_tang NVARCHAR(20) NULL,
    CONSTRAINT PK_tblTang PRIMARY KEY (ma_tang)
);
GO

/*==============================================================*/
/* Table: tblTinhTrangPhong - Bảng tình trạng phòng            */
/*==============================================================*/
CREATE TABLE tblTinhTrangPhong (
    ma_tinh_trang INT IDENTITY(1,1) NOT NULL,
    mo_ta NVARCHAR(50) NULL,
    CONSTRAINT PK_tblTinhTrangPhong PRIMARY KEY (ma_tinh_trang)
);
GO

/*==============================================================*/
/* Table: tblPhong - Bảng phòng                                */
/*==============================================================*/
CREATE TABLE tblPhong (
    ma_phong INT IDENTITY(1,1) NOT NULL,
    so_phong NVARCHAR(10) NULL,
    loai_phong INT NULL,
    ma_tang INT NULL,
    ma_tinh_trang INT NULL,
    CONSTRAINT PK_tblPhong PRIMARY KEY (ma_phong)
);
GO

/*==============================================================*/
/* Table: tblTinhTrangPhieuDatPhong - Bảng tình trạng phiếu đặt*/
/*==============================================================*/
CREATE TABLE tblTinhTrangPhieuDatPhong (
    ma_tinh_trang INT IDENTITY(1,1) NOT NULL,
    tinh_trang NVARCHAR(50) NULL,
    CONSTRAINT PK_tblTinhTrangPhieuDatPhong PRIMARY KEY (ma_tinh_trang)
);
GO

/*==============================================================*/
/* Table: tblPhieuDatPhong - Bảng phiếu đặt phòng              */
/*==============================================================*/
CREATE TABLE tblPhieuDatPhong (
    ma_pdp INT IDENTITY(1,1) NOT NULL,
    ma_kh NVARCHAR(50) NULL,
    ngay_dat DATETIME NULL,
    ngay_vao DATETIME NULL,
    ngay_ra DATETIME NULL,
    ma_phong INT NULL,
    thong_tin_khach_thue NVARCHAR(400) NULL,
    ma_tinh_trang INT NULL,
    CONSTRAINT PK_tblPhieuDatPhong PRIMARY KEY (ma_pdp)
);
GO

/*==============================================================*/
/* Table: tblTinhTrangHoaDon - Bảng tình trạng hóa đơn         */
/*==============================================================*/
CREATE TABLE tblTinhTrangHoaDon (
    ma_tinh_trang INT IDENTITY(1,1) NOT NULL,
    mo_ta NVARCHAR(50) NULL,
    CONSTRAINT PK_tblTinhTrangHoaDon PRIMARY KEY (ma_tinh_trang)
);
GO

/*==============================================================*/
/* Table: tblHoaDon - Bảng hóa đơn                             */
/*==============================================================*/
CREATE TABLE tblHoaDon (
    ma_hd INT IDENTITY(1,1) NOT NULL,
    ma_nv INT NULL,
    ma_pdp INT NULL,
    ngay_tra_phong DATETIME NULL,
    ma_tinh_trang INT NULL,
    tien_phong FLOAT NULL,
    tien_dich_vu FLOAT NULL,
    phu_thu FLOAT NULL,
    tong_tien FLOAT NULL,
    CONSTRAINT PK_tblHoaDon PRIMARY KEY (ma_hd)
);
GO

/*==============================================================*/
/* Table: tblDichVuDaDat - Bảng dịch vụ đã đặt                 */
/*==============================================================*/
CREATE TABLE tblDichVuDaDat (
    id INT IDENTITY(1,1) NOT NULL,
    ma_hd INT NULL,
    ma_dv INT NULL,
    so_luong INT NULL,
    CONSTRAINT PK_tblDichVuDaDat PRIMARY KEY (id)
);
GO

/*==============================================================*/
/* Table: tblTinNhan - Bảng tin nhắn/đánh giá khách hàng       */
/*==============================================================*/
CREATE TABLE tblTinNhan (
    id INT IDENTITY(1,1) NOT NULL,
    ngay_gui DATETIME NULL,
    ma_kh NVARCHAR(50) NULL,
    ho_ten NVARCHAR(100) NULL,
    mail NVARCHAR(100) NULL,
    noi_dung NVARCHAR(500) NULL,
    danh_gia INT NULL,
    CONSTRAINT PK_tblTinNhan PRIMARY KEY (id)
);
GO

/*==============================================================*/
/* Tạo các ràng buộc khóa ngoại (Foreign Keys)                 */
/*==============================================================*/

-- FK: tblNhanVien -> tblChucVu
ALTER TABLE tblNhanVien
    ADD CONSTRAINT fk_ma_cv
    FOREIGN KEY (ma_chuc_vu) 
    REFERENCES tblChucVu(ma_chuc_vu);
GO

-- FK: tblPhong -> tblLoaiPhong
ALTER TABLE tblPhong
    ADD CONSTRAINT fk_ma_lp
    FOREIGN KEY (loai_phong) 
    REFERENCES tblLoaiPhong(loai_phong);
GO

-- FK: tblPhong -> tblTang
ALTER TABLE tblPhong
    ADD CONSTRAINT fk_ma_tang
    FOREIGN KEY (ma_tang) 
    REFERENCES tblTang(ma_tang);
GO

-- FK: tblPhong -> tblTinhTrangPhong
ALTER TABLE tblPhong
    ADD CONSTRAINT fk_ma_tt_2
    FOREIGN KEY (ma_tinh_trang) 
    REFERENCES tblTinhTrangPhong(ma_tinh_trang);
GO

-- FK: tblPhieuDatPhong -> tblKhachHang
ALTER TABLE tblPhieuDatPhong
    ADD CONSTRAINT fk_tgd_ma_kh2
    FOREIGN KEY (ma_kh) 
    REFERENCES tblKhachHang(ma_kh);
GO

-- FK: tblPhieuDatPhong -> tblPhong
ALTER TABLE tblPhieuDatPhong
    ADD CONSTRAINT fk_tgd_ma_phong_2
    FOREIGN KEY (ma_phong) 
    REFERENCES tblPhong(ma_phong);
GO

-- FK: tblPhieuDatPhong -> tblTinhTrangPhieuDatPhong
ALTER TABLE tblPhieuDatPhong
    ADD CONSTRAINT fk_tgd_tt_2
    FOREIGN KEY (ma_tinh_trang) 
    REFERENCES tblTinhTrangPhieuDatPhong(ma_tinh_trang);
GO

-- FK: tblHoaDon -> tblNhanVien
ALTER TABLE tblHoaDon
    ADD CONSTRAINT fk_ma_nv
    FOREIGN KEY (ma_nv) 
    REFERENCES tblNhanVien(ma_nv);
GO

-- FK: tblHoaDon -> tblPhieuDatPhong
ALTER TABLE tblHoaDon
    ADD CONSTRAINT fk_ma_pdp
    FOREIGN KEY (ma_pdp) 
    REFERENCES tblPhieuDatPhong(ma_pdp);
GO

-- FK: tblHoaDon -> tblTinhTrangHoaDon
ALTER TABLE tblHoaDon
    ADD CONSTRAINT fk_ma_tthd
    FOREIGN KEY (ma_tinh_trang) 
    REFERENCES tblTinhTrangHoaDon(ma_tinh_trang);
GO

-- FK: tblDichVuDaDat -> tblHoaDon
ALTER TABLE tblDichVuDaDat
    ADD CONSTRAINT fk_ma_hd
    FOREIGN KEY (ma_hd) 
    REFERENCES tblHoaDon(ma_hd);
GO

-- FK: tblDichVuDaDat -> tblDichVu
ALTER TABLE tblDichVuDaDat
    ADD CONSTRAINT fk_ma_dv
    FOREIGN KEY (ma_dv) 
    REFERENCES tblDichVu(ma_dv);
GO

-- FK: tblTinNhan -> tblKhachHang
ALTER TABLE tblTinNhan
    ADD CONSTRAINT fk_tin_nhan
    FOREIGN KEY (ma_kh) 
    REFERENCES tblKhachHang(ma_kh);
GO

/*==============================================================*/
/* Insert dữ liệu mẫu cơ bản                                   */
/*==============================================================*/

-- Dữ liệu mẫu cho tblChucVu
INSERT INTO tblChucVu (chuc_vu) VALUES 
    (N'Quản lý'),
    (N'Lễ tân'),
    (N'Nhân viên phục vụ'),
    (N'Kế toán'),
    (N'Bảo vệ');
GO

-- Dữ liệu mẫu cho tblTinhTrangPhong
INSERT INTO tblTinhTrangPhong (mo_ta) VALUES 
    (N'Trống'),
    (N'Đã đặt'),
    (N'Đang sử dụng'),
    (N'Đang dọn dẹp'),
    (N'Bảo trì');
GO

-- Dữ liệu mẫu cho tblTinhTrangPhieuDatPhong
INSERT INTO tblTinhTrangPhieuDatPhong (tinh_trang) VALUES 
    (N'Chờ xác nhận'),
    (N'Đã xác nhận'),
    (N'Đã nhận phòng'),
    (N'Đã hủy'),
    (N'Hoàn thành');
GO

-- Dữ liệu mẫu cho tblTinhTrangHoaDon
INSERT INTO tblTinhTrangHoaDon (mo_ta) VALUES 
    (N'Chưa thanh toán'),
    (N'Đã thanh toán'),
    (N'Thanh toán một phần'),
    (N'Đã hủy');
GO

-- Dữ liệu mẫu cho tblTang
INSERT INTO tblTang (ten_tang) VALUES 
    (N'Tầng 1'),
    (N'Tầng 2'),
    (N'Tầng 3'),
    (N'Tầng 4'),
    (N'Tầng 5');
GO

-- Dữ liệu mẫu cho tblLoaiPhong
INSERT INTO tblLoaiPhong (mo_ta, gia, ti_le_phu_thu, anh) VALUES 
    (N'Phòng Standard', 500000, 10, N'/images/standard.jpg'),
    (N'Phòng Superior', 700000, 15, N'/images/superior.jpg'),
    (N'Phòng Deluxe', 1000000, 20, N'/images/deluxe.jpg'),
    (N'Phòng Suite', 1500000, 25, N'/images/suite.jpg'),
    (N'Phòng VIP', 2000000, 30, N'/images/vip.jpg');
GO

-- Dữ liệu mẫu cho tblDichVu
INSERT INTO tblDichVu (ten_dv, gia, don_vi, anh, ton_kho) VALUES 
    (N'Nước suối', 10000, N'Chai', N'/images/nuoc.jpg', 100),
    (N'Coca Cola', 15000, N'Lon', N'/images/coca.jpg', 80),
    (N'Bia Heineken', 25000, N'Lon', N'/images/bia.jpg', 50),
    (N'Mì tôm', 8000, N'Gói', N'/images/mi.jpg', 150),
    (N'Dịch vụ giặt ủi', 50000, N'Kg', N'/images/giat.jpg', 999),
    (N'Thuê xe máy', 150000, N'Ngày', N'/images/xemay.jpg', 10),
    (N'Massage', 300000, N'Giờ', N'/images/massage.jpg', 5),
    (N'Đưa đón sân bay', 500000, N'Lượt', N'/images/airport.jpg', 3);
GO

-- Dữ liệu mẫu cho tblNhanVien (Tài khoản quản lý mặc định)
INSERT INTO tblNhanVien (ho_ten, ngay_sinh, dia_chi, sdt, tai_khoan, mat_khau, ma_chuc_vu) VALUES 
    (N'Nguyễn Văn Admin', '1990-01-01', N'Hà Nội', N'0912345678', N'admin', N'admin123', 1),
    (N'Trần Thị Lễ Tân', '1995-05-15', N'Hồ Chí Minh', N'0987654321', N'letan', N'letan123', 2);
GO

-- Dữ liệu mẫu cho tblPhong
INSERT INTO tblPhong (so_phong, loai_phong, ma_tang, ma_tinh_trang) VALUES 
    (N'101', 1, 1, 1),
    (N'102', 1, 1, 1),
    (N'103', 2, 1, 1),
    (N'201', 2, 2, 1),
    (N'202', 3, 2, 1),
    (N'203', 3, 2, 1),
    (N'301', 4, 3, 1),
    (N'302', 4, 3, 1),
    (N'401', 5, 4, 1),
    (N'402', 5, 4, 1);
GO

-- Dữ liệu mẫu cho tblKhachHang (Tài khoản khách hàng mẫu)
INSERT INTO tblKhachHang (ma_kh, mat_khau, ho_ten, cccd, sdt, mail, diem) VALUES 
    (N'KH001', N'123456', N'Lê Văn A', N'001234567890', N'0901234567', N'levana@email.com', 100),
    (N'KH002', N'123456', N'Phạm Thị B', N'002345678901', N'0902345678', N'phamthib@email.com', 50);
GO

/*==============================================================*/
/* Hoàn thành tạo database                                     */
/*==============================================================*/
PRINT N'=================================================';
PRINT N'Database QLHOMESTAY đã được tạo thành công!';
PRINT N'=================================================';
PRINT N'Thông tin database:';
PRINT N'- Tên database: dataQLKS';
PRINT N'- Số bảng: 14 bảng';
PRINT N'- Đã tạo các ràng buộc khóa ngoại';
PRINT N'- Đã thêm dữ liệu mẫu cơ bản';
PRINT N'';
PRINT N'Tài khoản quản trị mặc định:';
PRINT N'- Username: admin';
PRINT N'- Password: admin123';
PRINT N'';
PRINT N'Tài khoản lễ tân:';
PRINT N'- Username: letan';
PRINT N'- Password: letan123';
PRINT N'';
PRINT N'Tài khoản khách hàng mẫu:';
PRINT N'- Mã KH: KH001, Password: 123456';
PRINT N'- Mã KH: KH002, Password: 123456';
PRINT N'=================================================';
GO
