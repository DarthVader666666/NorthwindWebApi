using System;

namespace NorthwindWebApi.Models
{
    public class BlogArticleReadOneModel
    {
        public int ArticleId { get; set; }

        public string? Title { get; set; }

        public DateTime? Posted { get; set; }

        public int? AuthorId { get; set; }

        public string? AuthorName { get; set; }

        public string? Text { get; set; }
    }
}
