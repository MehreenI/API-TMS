namespace API_TMS.Dtos
{
    public class AnnouncementCreateDto
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
    }

    public class AnnouncementUpdateDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
    }

    public class AnnouncementResponseDto
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
