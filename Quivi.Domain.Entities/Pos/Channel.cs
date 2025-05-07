using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Pos
{
    public class Channel : IDeletableEntity
    {
        public int Id { get; set; }
        public string? PoSIdentifier { get; set; }
        public string Identifier
        {
            get => _identifier;
            set
            {
                _identifier = value;
                IdentifierSortable = value.PadLeft(50, '0');
            }
        }
        
        /// <summary>
        /// Computed column for sorting purposes by Identifier in case of Identifier is an integer.
        /// </summary>
        public string IdentifierSortable { get; internal set; } = string.Empty;

        private string _identifier = string.Empty;

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int ChannelProfileId { get; set; }
        public ChannelProfile? ChannelProfile { get; set; }

        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public ICollection<Session>? Sessions { get; set; }
        public ICollection<SpatialChannel>? SpatialChannels { get; set; }
        public ICollection<Order>? Orders { get; set; }
        #endregion
    }
}
