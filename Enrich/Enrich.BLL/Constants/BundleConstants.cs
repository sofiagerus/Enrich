namespace Enrich.BLL.Constants
{
    public static class BundleConstants
    {
        public const string TitleRequired = "Collection title is required";
        public const string TitleMaxLengthMessage = "Collection title cannot exceed 200 characters";
        public const int TitleMaxLength = 200;

        public const string DescriptionMaxLengthMessage = "Collection description cannot exceed 1000 characters";
        public const int DescriptionMaxLength = 1000;

        public const string ImageUrlMaxLengthMessage = "Image cannot exceed 2 MB";
        public const int ImageUrlMaxLength = 2097152;
    }
}
