using System.ComponentModel.DataAnnotations;

namespace Bulky.Models;

public class Company
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? StreetAddress { get; set; }
    //public string? Number { get; set; }
    //public string? Complement { get; set; }
    //public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? PhoneNumber { get; set; }
}
