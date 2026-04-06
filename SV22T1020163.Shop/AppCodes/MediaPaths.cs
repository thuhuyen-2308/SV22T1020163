namespace SV22T1020163.Shop
{
    /// <summary>
    /// Thư mục lưu ảnh upload chung với Admin (thư mục anh_chung) — appsettings Image:Root.
    /// </summary>
    public static class MediaPaths
    {
        public static string ResolveRoot(IWebHostEnvironment env, IConfiguration config)
        {
            var configured = config["Image:Root"];
            if (string.IsNullOrWhiteSpace(configured))
                configured = Path.Combine("..", "anh_chung");
            var full = Path.GetFullPath(Path.Combine(env.ContentRootPath, configured));
            Directory.CreateDirectory(Path.Combine(full, "products"));
            Directory.CreateDirectory(Path.Combine(full, "employees"));
            return full;
        }

        public static string ProductsPath(IWebHostEnvironment env, IConfiguration config) =>
            Path.Combine(ResolveRoot(env, config), "products");
    }
}
