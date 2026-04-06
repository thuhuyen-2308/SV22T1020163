namespace SV22T1020163.Admin
{
    /// <summary>
    /// Thư mục lưu ảnh chung (sản phẩm, nhân viên) — thư mục anh_chung cùng cấp solution, appsettings Image:Root.
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

        public static string EmployeesPath(IWebHostEnvironment env, IConfiguration config) =>
            Path.Combine(ResolveRoot(env, config), "employees");
    }
}
