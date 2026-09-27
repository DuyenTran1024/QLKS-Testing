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
    public class Testcases_DangKy
    {
        private IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        // --- HÀM ĐỌC DỮ LIỆU TỪ FILE CSV ---
        public static IEnumerable<TestCaseData> ReadCsvData()
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Testcases_DangKy.csv");

            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                parser.ReadFields(); // Đọc bỏ qua dòng Tiêu đề đầu tiên

                while (!parser.EndOfData)
                {
                    string[] fields = null; // Khởi tạo mảng chứa dữ liệu

                    try
                    {
                        // 1. CHỈ đưa lệnh đọc có nguy cơ gây lỗi vào try...catch
                        fields = parser.ReadFields();
                    }
                    catch (MalformedLineException ex)
                    {
                        // Nếu lỗi, in ra log và lệnh continue sẽ nhảy qua vòng lặp tiếp theo (bỏ qua dòng lỗi)
                        TestContext.Progress.WriteLine($"Đã bỏ qua một dòng do lỗi: {ex.Message}");
                        continue;
                    }

                    // 2. Việc kiểm tra và yield return được đưa ra NGOÀI khối try...catch
                    if (fields != null && fields.Length >= 7 && fields[0].StartsWith("TC_Register"))
                    {
                        string testCaseId = fields[0];
                        string moTa = fields[2];
                        string testData = fields[5];
                        string expectedResult = fields[6];

                        if (!moTa.Contains("Test tích hợp"))
                        {
                            yield return new TestCaseData(testCaseId, testData, expectedResult).SetName(testCaseId);
                        }
                    }
                }
            }
        }

        // --- HÀM TÁCH DỮ LIỆU TỪ CHUỖI ---
        // Hàm này giúp tách "Tên tài khoản: abc" thành "abc"
        private string ExtractValue(string rawData, string key)
        {
            if (string.IsNullOrEmpty(rawData)) return "";

            var lines = rawData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith(key))
                {
                    return line.Substring(key.Length).Trim(); // Lấy phần giá trị đằng sau
                }
            }
            return "";
        }

        // --- HÀM TEST CHÍNH ---
        // Tham số [TestCaseSource] sẽ tự động gọi hàm ReadCsvData và truyền dữ liệu vào đây
        [Test, TestCaseSource(nameof(ReadCsvData))]
        public void TestLuongDangKy(string testCaseId, string testData, string expectedResult)
        {
            driver.Navigate().GoToUrl("http://localhost:49921/Account/Register");

            // 1. Tách dữ liệu từ ô "Dữ liệu kiểm thử"
            string tk = ExtractValue(testData, "Tên tài khoản:");
            string mk = ExtractValue(testData, "Mật khẩu:");
            string ht = ExtractValue(testData, "Họ tên:");
            string cccd = ExtractValue(testData, "Số CCCD:");
            string sdt = ExtractValue(testData, "Số ĐT:");
            string email = ExtractValue(testData, "Email:");

            // (Mẹo nhỏ: Với TC_Register_001 cần ID duy nhất để không bị trùng sau nhiều lần test)
            if (testCaseId == "TC_Register_001" && !string.IsNullOrEmpty(tk))
            {
                tk += DateTime.Now.ToString("HHmmss");
            }

            // 2. Điền Form
            driver.FindElement(By.Id("ma_kh")).SendKeys(tk);
            driver.FindElement(By.Id("mat_khau")).SendKeys(mk);
            driver.FindElement(By.Id("ho_ten")).SendKeys(ht);
            driver.FindElement(By.Id("cccd")).SendKeys(cccd);
            driver.FindElement(By.Id("sdt")).SendKeys(sdt);
            driver.FindElement(By.Id("mail")).SendKeys(email);

            // 3. Submit
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Tạo tài khoản']")).Click();
            Thread.Sleep(2000); // Đợi server xử lý

            // 4. Kiểm chứng kết quả (Assert)
            if (expectedResult.Contains("thông báo") || expectedResult.Contains("không hợp lệ") || expectedResult.Contains("Vui lòng nhập"))
            {
                // Nếu Kịch bản là LỖI -> Hệ thống phải chặn lại ở trang Đăng ký (URL vẫn chứa Register)
                Assert.IsTrue(driver.Url.Contains("Register"), $"Lỗi tại {testCaseId}: Hệ thống đã cho qua dù dữ liệu sai!");

                // Nâng cao: Có thể kiểm tra thêm text thông báo lỗi trên UI
                // Assert.IsTrue(driver.PageSource.Contains("Vui lòng nhập đầy đủ thông tin"), "Không hiện thông báo lỗi!");
            }
            else
            {
                // Nếu Kịch bản là THÀNH CÔNG -> Hệ thống phải chuyển trang
                Assert.IsFalse(driver.Url.Contains("Register"), $"Lỗi tại {testCaseId}: Không thể đăng ký tài khoản hợp lệ!");
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