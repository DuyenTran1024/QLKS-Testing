using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace QLKS.Tests
{
    public class DangKyTichHopTests
    {
        private IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        public static IEnumerable<TestCaseData> ReadIntegrationCsvData()
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Testcases_DangKy.csv");
            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                parser.ReadFields();

                while (!parser.EndOfData)
                {
                    string[] fields = null;
                    try { fields = parser.ReadFields(); }
                    catch (MalformedLineException) { continue; }

                    if (fields != null && fields.Length >= 7 && fields[0].StartsWith("TC_Register"))
                    {
                        string testCaseId = fields[0];
                        string moTa = fields[2];
                        string testData = fields[5];
                        string expectedResult = fields[6];

                        if (moTa.Contains("Test tích hợp"))
                        {
                            yield return new TestCaseData(testCaseId, testData, expectedResult).SetName(testCaseId);
                        }
                    }
                }
            }
        }

        private string ExtractValue(string rawData, string key)
        {
            if (string.IsNullOrEmpty(rawData)) return "";
            var lines = rawData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith(key)) return line.Substring(key.Length).Trim();
            }
            return "";
        }

        [Test, TestCaseSource(nameof(ReadIntegrationCsvData))]
        public void TestLuonTichHop(string testCaseId, string testData, string expectedResult)
        {
            string tk = ExtractValue(testData, "Tên tài khoản:");
            string mk = ExtractValue(testData, "Mật khẩu:");
            string ht = ExtractValue(testData, "Họ tên:");
            string cccd = ExtractValue(testData, "Số CCCD:");
            string sdt = ExtractValue(testData, "Số ĐT:");
            string email = ExtractValue(testData, "Email:");
            string adminUser = ExtractValue(testData, "Tên tài khoản Admin:");
            string adminPass = ExtractValue(testData, "Mật khẩu Admin:");

            

            // 1. ĐĂNG KÝ
            driver.Navigate().GoToUrl("http://localhost:49921/Account/Register");
            driver.FindElement(By.Id("ma_kh")).SendKeys(tk);
            driver.FindElement(By.Id("mat_khau")).SendKeys(mk);
            driver.FindElement(By.Id("ho_ten")).SendKeys(ht);
            driver.FindElement(By.Id("cccd")).SendKeys(cccd);
            driver.FindElement(By.Id("sdt")).SendKeys(sdt);
            driver.FindElement(By.Id("mail")).SendKeys(email);
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Tạo tài khoản']")).Click();
            Thread.Sleep(1500);

            bool isRegisteredSuccess = !driver.Url.Contains("Register");

            // RÀO CHẮN: Kiểm tra ngay lập tức xem đăng ký có thành công thật không
            if (expectedResult.Contains("Tài khoản được tạo thành công") && !isRegisteredSuccess)
            {
                Assert.Fail($"LỖI TỪ FORM ĐĂNG KÝ: Web từ chối tạo tài khoản {tk}. Khả năng cao do vi phạm Validate (trùng lặp, sai format)!");
            }

            // 2. ĐĂNG XUẤT 
            if (isRegisteredSuccess)
            {
                // Không cần rê chuột rườm rà nữa, dùng JavaScript chọc thẳng vào nút Đăng Xuất luôn!
                IWebElement btnLogout = driver.FindElement(By.XPath("//a[contains(@href, 'Logout')]"));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btnLogout);

                Thread.Sleep(1000);
                // Bấm OK ở hộp thoại Confirm
                driver.SwitchTo().Alert().Accept();
                Thread.Sleep(1000);
            }

            // 3. ĐĂNG NHẬP ADMIN
            driver.Navigate().GoToUrl("http://localhost:49921/Admin/Index/Login");
            driver.FindElement(By.Id("tai_khoan")).SendKeys(adminUser);
            driver.FindElement(By.Id("mat_khau")).SendKeys(adminPass);
            driver.FindElement(By.XPath("//input[@value='Đăng Nhập']")).Click();
            Thread.Sleep(1500);

            // 4. MỞ MENU QUẢN LÝ -> KHÁCH HÀNG (Dùng luôn JS Click cho chắc cú)
            IWebElement menuQuanLy = driver.FindElement(By.XPath("//span[text()='Quản Lý']/.."));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", menuQuanLy);
            Thread.Sleep(1000);

            IWebElement menuKhachHang = driver.FindElement(By.XPath("//a[contains(@href, 'KhachHang')]"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", menuKhachHang);
            Thread.Sleep(1500);

            // 5. TÌM TRONG BẢNG BẰNG XPATH ĐỘNG
            // Khởi tạo XPath tìm các dòng (tr) trong bảng
            string xpathQuery = $"//table[@id='example-table']//td[contains(., '{tk}')]";

            var cells = driver.FindElements(By.XPath(xpathQuery));

            try
            {
                if (expectedResult.Contains("không cho tạo") || expectedResult.Contains("không hợp lệ") || expectedResult.Contains("vui lòng nhập"))
                {
                    Assert.IsTrue(cells.Count == 0, $"LỖI BUG: Tài khoản '{tk}' nhập sai mà vẫn xuất hiện trong Admin!");
                }
                else
                {
                    Assert.IsTrue(cells.Count > 0, $"LỖI: Tài khoản '{tk}' hợp lệ nhưng không hiển thị trong bảng Admin!");
                }
            }
            finally
            {
                // Dù hàm Assert bên trên có quăng lỗi (Fail) hay chạy êm ru (Pass), 
                // thì block finally này LUÔN LUÔN được thực thi.

                // Gọi hàm dọn dẹp, truyền email vừa dùng để test vào:
                CleanUpDatabase(email);
            }
        }

        private void CleanUpDatabase(string email)
        {
            // Nếu email rỗng (các case test không nhập email) thì không cần xóa
            if (string.IsNullOrEmpty(email)) return;

            // TODO: Thay thế chuỗi này bằng Connection String kết nối đến Database QLKS của bạn
            string connectionString = "Data Source=TEN_SERVER_CUA_BAN;Initial Catalog=TEN_DATABASE_QLKS;Integrated Security=True";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // TODO: Đổi tên bảng (vd: KhachHang) và tên cột (vd: Email) cho đúng với thiết kế Database của bạn
                    string query = "DELETE FROM KhachHang WHERE Email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"[Clean Up] Đã xóa thành công tài khoản rác có email: {email} khỏi Database.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Chỉ in ra log để theo dõi, không làm sập Test Case nếu lỗi dọn dẹp
                Console.WriteLine($"[Lỗi Clean Up] Không thể xóa dữ liệu: {ex.Message}");
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