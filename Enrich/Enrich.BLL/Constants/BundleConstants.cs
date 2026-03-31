namespace Enrich.BLL.Constants
{
    public static class BundleConstants
    {
        public const string TitleRequired = "Назва бандлу є обов'язковою";
        public const string TitleMaxLengthMessage = "Назва бандлу не може перевищувати 200 символів";
        public const int TitleMaxLength = 200;

        public const string DescriptionMaxLengthMessage = "Опис бандлу не може перевищувати 1000 символів";
        public const int DescriptionMaxLength = 1000;

        public const string ImageUrlMaxLengthMessage = "Image cannot exceed 2 MB";
        public const int ImageUrlMaxLength = 2097152;
    }
}
