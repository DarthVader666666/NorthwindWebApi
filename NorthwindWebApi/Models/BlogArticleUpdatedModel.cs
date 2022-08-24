using System;

namespace NorthwindWebApi.Models
{
    public class BlogArticleUpdatedModel
    {
        public string? Title { get; set; }

        public string? Text { get; set; }

        public int? AuthorId { get; set; }
    }
}
