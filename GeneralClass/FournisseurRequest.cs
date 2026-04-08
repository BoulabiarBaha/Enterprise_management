namespace MyApp.GeneralClass
{
    public class FournisseurRequest
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string Tel { get; set; } = "";
        public string Address { get; set; } = "";
        public Guid CreatedBy { get; set; }
    }
}
