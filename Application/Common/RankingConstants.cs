namespace Application.Common
{
    public static class RankingConstants
    {
        // Bộ trọng số mặc định (Cân bằng)
        public const double DefRatingWeight = 0.5;
        public const double DefPriceWeight = 0.3;
        public const double DefDistanceWeight = 0.2;

        // Bộ trọng số khi người dùng ưu tiên sắp xếp theo Giá (SortBy = "price")
        public const double PricePriorityRatingWeight = 0.35;
        public const double PricePriorityPriceWeight = 0.55;
        public const double PricePriorityDistanceWeight = 0.10;

        // Bộ trọng số khi người dùng ưu tiên sắp xếp theo Khoảng cách (SortBy = "nearest")
        public const double DistPriorityRatingWeight = 0.20;
        public const double DistPriorityPriceWeight = 0.10;
        public const double DistPriorityDistanceWeight = 0.70;

        // Các giá trị biên fallback phòng trường hợp chia cho 0
        public const double FallbackMinPrice = 50000.0;
        public const double FallbackMaxPrice = 1000000.0;
        public const double DefaultMaxRadiusKm = 20.0;
    }
}
