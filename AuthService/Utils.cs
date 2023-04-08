namespace AuthService
{
    public static class Utils
    {
        public static string SHA512(string input)
        {
            try
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                using var hash = System.Security.Cryptography.SHA512.Create();
                var hashedInputBytes = hash.ComputeHash(bytes);

                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
