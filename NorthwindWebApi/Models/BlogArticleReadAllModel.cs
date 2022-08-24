using System;

namespace NorthwindWebApi.Models
{
    public class BlogArticleReadAllModel
    {
        public int ArticleId { get; set; }

        public string? Title { get; set; }

        public DateTime? PostedDate { get; set; }

        public int? AuthorId { get; set; }

        public string? AuthorName { get; set; }
    }
}
