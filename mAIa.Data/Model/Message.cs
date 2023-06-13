namespace mAIa.Data.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Messages", Schema = "Chat")]
    public class Message
    {
        public Message()
        {
            Timestamp = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid MessageID { get; set; }

        public DateTime Timestamp { get; set; }

        public string DiscordUsername { get; set; }

        [ForeignKey(nameof(User))]
        public ulong? DiscordUserID { get; set; }

        public ulong? ServerID { get; set; }

        public ulong ChannelID { get; set; }

        [Required]
        public string Content { get; set; }

        public string PlainText { get; set; }

        public int? PlainTextTokenCount { get; set; }

        public string Summary { get; set; }

        public int ContentTokenCount { get; set; }

        public int? SummaryTokenCount { get; set; }

        public string MessageType { get; set; }

        public virtual User User { get; set; }
    }
}