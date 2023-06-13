namespace mAIa.Data.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("UserTraits", Schema = "Chat")]
    public class UserTrait
    {
        public UserTrait()
        {
            DateAdded = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserTraitID { get; set; }

        [ForeignKey(nameof(User))]
        public ulong DiscordUserID { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public string RelevanceToUser { get; set; }

        public DateTime DateAdded { get; set; }

        public virtual User User { get; set; }
    }
}