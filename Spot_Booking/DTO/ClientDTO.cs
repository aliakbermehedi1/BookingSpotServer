namespace Spot_Booking.DTO
{
    public class ClientDTO
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public DateTime BirthDate { get; set; }
        public int PhoneNo { get; set; }
        public bool MaritalStatus { get; set; }
        public IFormFile? PictureFile { get; set; }
        public List<int>? SpotId { get; set; }
    }
}
