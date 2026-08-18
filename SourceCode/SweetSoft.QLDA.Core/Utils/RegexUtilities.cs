//-----------------------PROGRAMER LOGS---------------------------
//'**Change 01: Truong - 08 Oct 2024: - Manage encryption settings for data at rest and in transit.
//                                    -Configure security protocols to protect data integrity and privacy.
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SweetSoft.QLDA.Core.Utils
{
    public class RegexUtilities
    {
        static bool invalid;
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            return true;
            //try
            //{
            //    return Regex.IsMatch(url, @"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)");
            //}
            //catch
            //{
            //    return false;
            //}
        }
        public static bool IsValidVideo(string path)
        {
            try
            {
                return Regex.IsMatch(path, @"(?:((?:https|http):\/\/)|(?:\/)).+(?:.mp3|mp4|avi)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        public static bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            try
            {
                // Valid unicode
                invalid = Regex.IsMatch(strIn, @"[ă|â|đ|ê|ô|ơ|ư|à|ả|ã|ạ|á|ằ|ẳ|ẵ|ặ|ắ|ầ|ẩ|ẫ|ậ|ấ|è|ẻ|ẽ|ẹ|é|ề|ể|ễ|ệ|ế|ì|ỉ|ĩ|ị|í|ò|ỏ|õ|ọ|ó|ồ|ổ|ỗ|ộ|ố|ờ|ở|ỡ|ợ|ớ|ù|ủ|ũ|ụ|ú|ừ|ử|ữ|ự|ứ|ỳ|ỷ|ỹ|ỵ|ý|Ă|Â|Đ|Ê|Ô|Ơ|Ư|À|Ả|Ã|Ạ|Á|Ằ|Ẳ|Ẵ|Ặ|Ắ|Ầ|Ẩ|Ẫ|Ậ|Ấ|È|Ẻ|Ẽ|Ẹ|É|Ề|Ể|Ễ|Ệ|Ế|Ì|Ỉ|Ĩ|Ị|Í|Ò|Ỏ|Õ|Ọ|Ó|Ồ|Ổ|Ỗ|Ộ|Ố|Ờ|Ở|Ỡ|Ợ|Ớ|Ù|Ủ|Ũ|Ụ|Ú|Ừ|Ử|Ữ|Ự|Ứ|Ỳ|Ỷ|Ỹ|Ỵ|Ý]");
            }
            catch
            {
                return false;
            }

            if (invalid)
                return false;
            // Return true if strIn is in valid email format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length > 15)
                return false;
            try
            {
                return Regex.IsMatch(phone, @"^(\+|0)?[1-9]\d{0,14}$");
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        public static bool IsValidPassword(string password, ref string message)
        {
            try
            {
                //if (string.IsNullOrEmpty(password))
                //    return false;
                //return Regex.IsMatch(password, @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$");

                if (password.Length < 8)
                {
                    message = "Password must be at least 8 characters long.";
                    return false;
                }

                // Check for at least one uppercase letter
                //if (!Regex.IsMatch(password, @"[A-Z]"))
                //{
                //    message = "Password must contain at least one uppercase letter.";
                //    return false;
                //}

                //// Check for at least one lowercase letter
                //if (!Regex.IsMatch(password, @"[a-z]"))
                //{
                //    message = "Password must contain at least one lowercase letter.";
                //    return false;
                //}

                // Check for at least one digit
                if (!Regex.IsMatch(password, @"[0-9]") && !Regex.IsMatch(password, @"[\W_]"))
                {
                    message = "Contain a number or special character";
                    return false;
                }

                // Check for at least one special character
                //if (!Regex.IsMatch(password, @"[\W_]")) // \W matches any non-word character (special chars)
                //{
                //    message = "Password must contain at least one special character.";
                //    return false;
                //}

                // If all conditions are met
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidDomainName(string strIn)
        {
            return Regex.IsMatch(strIn, @"^([a-zA-Z0-9]+(\.[a-zA-Z0-9]+)+.*)$");
        }
        public static bool IsValidCryptoAddress(string address)
        {
            try
            {
                if (string.IsNullOrEmpty(address))
                    return false;
                return Regex.IsMatch(address, @"^0x[a-fA-F0-9]{40}$");
            }
            catch
            {
                return false;
            }
        }
        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
        public static string RegexPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return "";
            return phone.Replace(" ", "").Replace("+", "").Replace("84", "0").Replace(".", "").Replace("(", "").Replace(")", "");
        }
        public static bool IsValidCardNumber(string cardNumber)
        {
            try
            {
                cardNumber = new string(cardNumber.Where(char.IsDigit).ToArray());
                if (cardNumber.Length < 9 && cardNumber.Length > 16)
                    return false;
                return true;
                int sum = 0;
                bool alternate = false;
                for (int i = cardNumber.Length - 1; i >= 0; i--)
                {
                    int n = int.Parse(cardNumber[i].ToString());

                    if (alternate)
                    {
                        n *= 2;
                        if (n > 9)
                        {
                            n -= 9;
                        }
                    }

                    sum += n;
                    alternate = !alternate;
                }
                return (sum % 10 == 0);
            }
            catch
            {
                return false;
            }
        }
        public static bool IsValidNumberAndHyphen(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return Regex.IsMatch(input, @"^[0-9\-]+$");
        }
        public static bool IsCreditCardInfoValid(string cardNo, string expiryDate, string cvv)
        {
            var cardCheck = new Regex(@"^(1298|1267|4512|4567|8901|8933)([\-\s]?[0-9]{4}){3}$");
            var monthCheck = new Regex(@"^(0[1-9]|1[0-2])$");
            var yearCheck = new Regex(@"^20[0-9]{2}$");
            var cvvCheck = new Regex(@"^\d{3}$");

            if (!cardCheck.IsMatch(cardNo)) // <1>check card number is valid
                return false;
            if (!cvvCheck.IsMatch(cvv)) // <2>check cvv is valid as "999"
                return false;

            var dateParts = expiryDate.Split('/'); //expiry date in from MM/yyyy            
            if (!monthCheck.IsMatch(dateParts[0]) || !yearCheck.IsMatch(dateParts[1])) // <3 - 6>
                return false; // ^ check date format is valid as "MM/yyyy"

            var year = int.Parse(dateParts[1]);
            var month = int.Parse(dateParts[0]);
            var lastDateOfExpiryMonth = DateTime.DaysInMonth(year, month); //get actual expiry date
            var cardExpiry = new DateTime(year, month, lastDateOfExpiryMonth, 23, 59, 59);

            //check expiry greater than today & within next 6 years <7, 8>>
            return (cardExpiry > DateTime.UtcNow && cardExpiry < DateTime.UtcNow.AddYears(6));
        }
        public static string FixMediaUrls(string html, string domain)
        {
            string pattern = @"(<(?:img|video|iframe)[^>]+?\bsrc=[""'])([^""']+)([""'])";

            return Regex.Replace(html, pattern, match =>
            {
                string prefix = match.Groups[1].Value;
                string url = match.Groups[2].Value;
                string suffix = match.Groups[3].Value;

                if (!Regex.IsMatch(url, @"^(https?:)?//"))
                {
                    url = $"{domain}/{url.TrimStart('/')}";
                }

                return $"{prefix}{url}{suffix}";
            }, RegexOptions.IgnoreCase);
        }
        public static string UsingHiddenHostPathUrls(string html, string domain)
        {
            string pattern = @"(<(?:img|video|iframe)[^>]+?\bsrc=[""'])([^""']+)([""'])";

            return Regex.Replace(html, pattern, match =>
            {
                string prefix = match.Groups[1].Value;
                string url = match.Groups[2].Value;
                string suffix = match.Groups[3].Value;

                if (!Regex.IsMatch(url, @"^(https?:)?//"))
                {
                    if (url.Contains("uploads"))
                        url = $"{domain}?Path={url.TrimStart('/')}";
                    else
                        url = $"{domain}?Path=uploads/{url.TrimStart('/')}";
                }

                return $"{prefix}{url}{suffix}";
            }, RegexOptions.IgnoreCase);
        }
        public static string GetInitials(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Nếu là email thì lấy phần trước @
            if (text.Contains("@"))
            {
                text = text.Split('@')[0];
            }

            var parts = text.Trim().Split(new[] { ' ', '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            }

            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

    }
}
