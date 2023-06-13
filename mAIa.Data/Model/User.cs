namespace mAIa.Data.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Users", Schema = "Chat")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong DiscordUserID { get; set; }

        public string DiscordUsername { get; set; }

        public int InappropriateWarningCount { get; set; }

        public DateTime? InnappropriateWarningIgnoredFrom { get; set; }

        public virtual ICollection<Message> Messages { get; set; }

        public virtual ICollection<UserTrait> Traits { get; set; }
    }
}