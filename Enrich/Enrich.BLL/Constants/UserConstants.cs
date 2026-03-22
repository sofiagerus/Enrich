namespace Enrich.BLL.Constants
{
    public static class UserConstants
    {
        public const string EmailRequired = "Email is required";
        public const string InvalidEmailFormat = "Invalid email format";

        public const string PasswordRequired = "Password is required";
        public const string PasswordMinLengthMessage = "Password must be at least 8 characters long";
        public const string PasswordRequiresUppercase = "Password must contain at least one uppercase letter";
        public const int PasswordMinLength = 8;
        public const string PasswordUppercaseRegex = "^(?=.*[A-Z]).+$";

        public const string UsernameRequired = "Username is required";
        public const string UsernameMinLengthMessage = "Username must be at least 3 characters long";
        public const string UsernameMaxLengthMessage = "Username cannot exceed 16 characters";
        public const string UsernameInvalidFormat = "Only Latin letters, numbers, dots, and underscores are allowed";
        public const int UsernameMinLength = 3;
        public const int UsernameMaxLength = 16;
        public const string UsernameAllowedCharactersRegex = "^[a-zA-Z0-9._]*$";

        public const string BioMaxLengthMessage = "Bio cannot exceed 100 characters";
        public const int BioMaxLength = 100;
    }
}
