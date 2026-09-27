using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Threading;

namespace QLKS.Tests
{
    public class Testcases_DangNhap
    {
        private IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        public static IEnumerable<TestCaseData> ReadLoginCsvData()
        {
            // Trỏ đến file CSV Đăng nhập đã được đổi tên
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Testcases_DangNhap.csv");
            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                parser.ReadFields(); // Bỏ qua dòng header

                while (!parser.EndOfData)
                {
                    string[] fields = null;
                    try { fields = parser.ReadFields(); }
                    catch (MalformedLineException) { continue; }

                    // Thay tiền tố TC_Login cho phù hợp với ID trong file CSV của bạn (ví dụ: TC_Login_001)
                    if (fields != null && fields.Length >= 7 && fields[0].StartsWith("TC_"))
                    {
                        string testCaseId = fields[0];
                        string testData = fields[5]; // Cột Dữ liệu kiểm thử
                        string expectedResult = fields[6]; // Cột Kết quả mong đợi
                        string moTa = fields[2]; // Cột Mô tả

                        yield return new TestCaseData(testCaseId, testData, expectedResult, moTa).SetName(testCaseId);
                    }
                }
            }
        }

        private string ExtractValue(string rawData, string keyword)
        {
            if (string.IsNullOrEmpty(rawData)) return "";

            // Tách dữ liệu thành từng dòng
            var lines = rawData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // Chuyển tất cả về chữ thường để so sánh, bỏ qua lỗi gõ hoa/thường
                if (line.ToLower().Contains(keyword.ToLower()))
                {
                    // Chặt dòng chữ ra làm 2 phần tại vị trí dấu hai chấm ":"
                    var parts = line.Split(':');
                    if (parts.Length >= 2)
                    {
                        // Lấy phần thứ 2 (giá trị) và cắt sạch khoảng trắng thừa 2 đầu
                        return parts[1].Trim();
                    }
                }
            }
            return "";
        }

        [Test, TestCaseSource(nameof(ReadLoginCsvData))]
        public void TestLuongDangNhap(string testCaseId, string testData, string expectedResult, string moTa)
        {
            // 1. Mở trang Đăng nhập
            // Tự động phân luồng URL: Nếu Mô tả chứa chữ "Admin" thì vào trang Admin, ngược lại vào trang Khách
            if (moTa.Contains("Admin")) // Lấy biến moTa từ hàm ReadLoginCsvData truyền xuống nhé
            {
                driver.Navigate().GoToUrl("http://localhost:49921/Admin/Index/Login");
            }
            else
            {
                driver.Navigate().GoToUrl("http://localhost:49921/Account/Login");
            }

            // 2. Lấy dữ liệu (Chỉ truyền từ khóa cốt lõi)
            string tk = ExtractValue(testData, "username");
            string mk = ExtractValue(testData, "password");

            // Vẫn giữ lại 2 dòng in log để kiểm chứng nhé:
            TestContext.Progress.WriteLine($"[DEBUG] Tài khoản trích xuất được: '{tk}'");
            TestContext.Progress.WriteLine($"[DEBUG] Mật khẩu trích xuất được: '{mk}'");

            // Xác định xem đây là luồng test Admin hay Khách hàng dựa vào mô tả
            bool isAdminLogin = moTa.Contains("Admin");

            // 3. ĐIỀN FORM VỚI ID ĐỘNG
            // Chọn ID ô Username tương ứng với từng trang
            string idUsername = isAdminLogin ? "tai_khoan" : "ma_kh";
            string idPassword = "mat_khau"; // Thường ô mật khẩu ở 2 trang đều là mat_khau, nếu của bạn khác thì sửa nhé

            IWebElement txtUser = driver.FindElement(By.Id(idUsername));
            txtUser.Clear();
            if (!string.IsNullOrEmpty(tk))
            {
                txtUser.SendKeys(tk);
            }

            IWebElement txtPass = driver.FindElement(By.Id(idPassword));
            txtPass.Clear();
            if (!string.IsNullOrEmpty(mk))
            {
                txtPass.SendKeys(mk);
            }

            // 4. BẤM NÚT ĐĂNG NHẬP
            // Dùng XPath tìm kiếm theo value để "bắt" được nút ở cả 2 trang
            driver.FindElement(By.XPath("//input[@value='Đăng Nhập']")).Click();
            Thread.Sleep(2000);

            // 5. Kiểm chứng (Assert)
            // Dựa vào code AccountController.cs của bạn, đăng nhập thành công sẽ chuyển sang BookRoom
            if (expectedResult.ToLower().Contains("thành công") || expectedResult.ToLower().Contains("chuyển"))
            {
                Assert.IsFalse(driver.Url.Contains("Login"), $"Lỗi tại {testCaseId}: Tài khoản đúng nhưng không thể đăng nhập!");
                Assert.IsTrue(driver.Url.Contains("BookRoom") || driver.Url.Contains("Home") || driver.Url.Contains("Dashboard") || driver.Url.Contains("Admin"),
                $"Lỗi tại {testCaseId}: Đăng nhập xong nhưng không vào đúng trang đích!");
            }
            else
            {
                // Luồng báo lỗi (Sai pass, sai user, bỏ trống)
                Assert.IsTrue(driver.Url.Contains("Login"), $"BUG BẢO MẬT tại {testCaseId}: Dữ liệu sai nhưng hệ thống vẫn cho đăng nhập!");

                // (Nâng cao) Bắt text lỗi "Login data is incorrect!" hiển thị trên màn hình
                // Assert.IsTrue(driver.PageSource.Contains("incorrect") || driver.PageSource.Contains("Vui lòng"), $"Lỗi tại {testCaseId}: Không hiển thị thông báo lỗi!");
            }
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}