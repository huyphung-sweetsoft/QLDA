# JSON-LD Structured Data Builder

Tệp này mô tả cách sử dụng lớp `JsonLdStructuredDataBuilder` để tạo ra dữ liệu có cấu trúc dạng `application/ld+json` trên dự án ASP.NET Web Forms.

## Đặc điểm chính

- Fluent API với các lớp con tương ứng các schema phổ biến: `Organization`, `WebSite`, `WebPage`, `BreadcrumbList`, `Article`, `Product`, `FAQPage`, v.v.
- Tự động chuẩn hóa các kiểu dữ liệu phức tạp (ngày giờ, URL, entity lồng nhau) về định dạng JSON-LD hợp lệ.
- Hỗ trợ `@graph` khi cần khai báo nhiều entity trong cùng một payload.
- Dễ mở rộng: có thể dùng `GenericSchemaEntity` để bổ sung nhanh các schema chưa có sẵn.

## Cách sử dụng cơ bản trong Web Forms

### 1. Render trực tiếp thẻ `<script>`

```csharp
using Sweet.Seo.StructuredData;

protected void Page_Load(object sender, EventArgs e)
{
    var organization = new OrganizationSchema()
        .WithName("Sweet Studio")
        .WithUrl("https://sweetsoft.local")
        .WithLogo("https://sweetsoft.local/assets/logo.svg")
        .WithSameAs("https://www.facebook.com/sweetsoft")
        .WithAddress(new PostalAddressSchema()
            .WithStreetAddress("01 Nguyen Trai")
            .WithAddressLocality("Ho Chi Minh")
            .WithAddressCountry("VN"));

    var website = new WebSiteSchema()
        .WithName("Sweet Studio")
        .WithUrl("https://sweetsoft.local")
        .WithPotentialAction(new SearchActionSchema()
            .WithTarget("https://sweetsoft.local/search?q={search_term_string}")
            .WithQueryInput("required name=search_term_string"));

    SeoLiteral.Text = new JsonLdStructuredDataBuilder()
        .AddEntity(organization)
        .AddEntity(website)
        .BuildScriptTag();
}
```

`SeoLiteral` là một `Literal` control trong Web Forms dùng để in thẻ `<script type="application/ld+json">` ra HTML.

### 2. Article + Breadcrumb

```csharp
var breadcrumb = new BreadcrumbListSchema()
    .WithItems(
        new ListItemSchema().WithPosition(1).WithName("Trang chủ").WithItem("https://sweetsoft.local"),
        new ListItemSchema().WithPosition(2).WithName("Blog").WithItem("https://sweetsoft.local/blog"),
        new ListItemSchema().WithPosition(3).WithName("Chi tiết bài viết").WithItem(Request.Url.AbsoluteUri));

var article = new ArticleSchema()
    .WithHeadline(Post.Title)
    .WithDescription(Post.Description)
    .WithMainEntityOfPage(Request.Url.AbsoluteUri)
    .WithDatePublished(Post.PublishedAt)
    .WithDateModified(Post.UpdatedAt)
    .WithAuthor(new PersonSchema().WithName(Post.AuthorName))
    .WithPublisher(new OrganizationSchema().WithName("Sweet Studio"))
    .WithImage(new ImageObjectSchema().WithUrl(Post.FeaturedImage));

var jsonLd = JsonLdStructuredDataBuilder.BuildSingle(article);
```

Sau khi có chuỗi JSON-LD, bạn có thể:

- Gán vào `Literal` hoặc `PlaceHolder` để render.
- Lưu xuống file tĩnh hoặc cache nếu cần tái sử dụng.

## Mở rộng thêm schema mới

Nếu Google Search hỗ trợ schema mới nhưng chưa có class tương ứng, sử dụng `GenericSchemaEntity`:

```csharp
var jobPosting = new GenericSchemaEntity("JobPosting")
    .WithProperty("title", "Senior .NET Developer")
    .WithProperty("datePosted", DateTimeOffset.UtcNow)
    .WithProperty("employmentType", new[] { "FULL_TIME" })
    .WithProperty("hiringOrganization", new OrganizationSchema().WithName("Sweet"));

var json = JsonLdStructuredDataBuilder.BuildSingle(jobPosting);
```

Hoặc tạo class kế thừa `SchemaEntity` để có API fluent tương tự các schema có sẵn.

## Kiểm thử

Sau khi render, nên dùng [Rich Results Test](https://search.google.com/test/rich-results) hoặc [Schema Markup Validator](https://validator.schema.org/) để xác nhận payload hợp lệ với yêu cầu mới nhất của Google.