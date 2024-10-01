using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bulky.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? StreetAddress { get; set; }
    //public string? Number { get; set; }
    //public string? Complement { get; set; }
    //public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public int? CompanyId { get; set; }
    [ForeignKey("CategoryId")]
    [ValidateNever]
    public Company Company { get; set; }
}