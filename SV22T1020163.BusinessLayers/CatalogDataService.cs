using SV22T1020163.DataLayers.Interfaces;
using SV22T1020163.DataLayers.SQLServer;
using SV22T1020163.Models.Catalog;
using SV22T1020163.Models.Common;

namespace SV22T1020163.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến danh mục hàng hóa của hệ thống, 
    /// bao gồm: mặt hàng (Product), thuộc tính của mặt hàng (ProductAttribute) và ảnh của mặt hàng (ProductPhoto).
    /// </summary>
    public static class CatalogDataService
    {
        private static readonly IProductRepository productDB;
        private static readonly IGenericRepository<Category> categoryDB;

        /// <summary>
        /// Constructor
        /// </summary>
        static CatalogDataService()
        {
            categoryDB = new CategoryRepository(Configuration.ConnectionString);
            productDB = new ProductRepository(Configuration.ConnectionString);
        }

        #region Category

        public static async Task<PagedResult<Category>> ListCategoriesAsync(PaginationSearchInput input)
        {
            return await categoryDB.ListAsync(input);
        }

        public static async Task<Category?> GetCategoryAsync(int CategoryID)
        {
            return await categoryDB.GetAsync(CategoryID);
        }

        /// <summary>
        /// Bổ sung một loại hàng mới
        /// </summary>
        public static async Task<int> AddCategoryAsync(Category data)
        {
            if (string.IsNullOrWhiteSpace(data.CategoryName))
                throw new ArgumentException("Tên loại hàng không được để trống");

            return await categoryDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin của một loại hàng
        /// </summary>
        public static async Task<bool> UpdateCategoryAsync(Category data)
        {
            if (string.IsNullOrWhiteSpace(data.CategoryName))
                return false;

            return await categoryDB.UpdateAsync(data);
        }

        public static async Task<bool> DeleteCategoryAsync(int CategoryID)
        {
            if (await categoryDB.IsUsedAsync(CategoryID))
                return false;

            return await categoryDB.DeleteAsync(CategoryID);
        }

        public static async Task<bool> IsUsedCategoryAsync(int CategoryID)
        {
            return await categoryDB.IsUsedAsync(CategoryID);
        }

        #endregion

        #region Product

        public static async Task<PagedResult<Product>> ListProductsAsync(ProductSearchInput input)
        {
            return await productDB.ListAsync(input);
        }

        public static async Task<List<Product>> ListFeaturedProductsByCategoryAsync(int maxCount = 8)
        {
            return await productDB.ListFeaturedByCategoryAsync(maxCount);
        }

        public static async Task<Product?> GetProductAsync(int productID)
        {
            return await productDB.GetAsync(productID);
        }

        /// <summary>
        /// Bổ sung một mặt hàng mới
        /// </summary>
        public static async Task<int> AddProductAsync(Product data)
        {
            if (string.IsNullOrWhiteSpace(data.ProductName))
                throw new ArgumentException("Tên mặt hàng không được để trống");

            if (data.CategoryID <= 0)
                throw new ArgumentException("Vui lòng chọn loại hàng");

            if (data.Price < 0) data.Price = 0;

            return await productDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin của một mặt hàng
        /// </summary>
        public static async Task<bool> UpdateProductAsync(Product data)
        {
            if (string.IsNullOrWhiteSpace(data.ProductName))
                return false;

            if (data.Price < 0) data.Price = 0;

            return await productDB.UpdateAsync(data);
        }

        public static async Task<bool> DeleteProductAsync(int productID)
        {
            if (await productDB.IsUsedAsync(productID))
                return false;

            return await productDB.DeleteAsync(productID);
        }

        public static async Task<bool> IsUsedProductAsync(int productID)
        {
            return await productDB.IsUsedAsync(productID);
        }

        #endregion

        #region ProductAttribute

        public static async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            return await productDB.ListAttributesAsync(productID);
        }

        public static async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            return await productDB.GetAttributeAsync(attributeID);
        }

        /// <summary>
        /// Bổ sung một thuộc tính mới cho mặt hàng.
        /// </summary>
        public static async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            if (string.IsNullOrWhiteSpace(data.AttributeName) || string.IsNullOrWhiteSpace(data.AttributeValue))
                throw new ArgumentException("Tên và giá trị thuộc tính không được để trống");

            if (data.DisplayOrder < 0) data.DisplayOrder = 0;

            return await productDB.AddAttributeAsync(data);
        }

        public static async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            if (string.IsNullOrWhiteSpace(data.AttributeName) || string.IsNullOrWhiteSpace(data.AttributeValue))
                return false;

            return await productDB.UpdateAttributeAsync(data);
        }

        public static async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            return await productDB.DeleteAttributeAsync(attributeID);
        }

        #endregion

        #region ProductPhoto

        public static async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            return await productDB.ListPhotosAsync(productID);
        }

        public static async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            return await productDB.GetPhotoAsync(photoID);
        }

        /// <summary>
        /// Bổ sung một ảnh mới cho mặt hàng.
        /// </summary>
        public static async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            if (string.IsNullOrWhiteSpace(data.Photo))
                throw new ArgumentException("Vui lòng chọn file ảnh");

            if (data.DisplayOrder < 0) data.DisplayOrder = 0;

            return await productDB.AddPhotoAsync(data);
        }

        public static async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            if (string.IsNullOrWhiteSpace(data.Photo))
                return false;

            return await productDB.UpdatePhotoAsync(data);
        }

        public static async Task<bool> DeletePhotoAsync(long photoID)
        {
            return await productDB.DeletePhotoAsync(photoID);
        }

        #endregion
    }
}