using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BlazorHtmlEditor.Demo.Server.Models;

/// <summary>
/// Sample data model for demonstrating the editor.
/// This class represents a typical customer entity with various property types
/// including strings, dates, booleans, decimals, and collections.
/// The Display attributes provide user-friendly names and descriptions for the UI.
/// </summary>
public class SampleModel
{
    /// <summary>
    /// Gets or sets the customer's first name.
    /// </summary>
    [Display(Name = "First Name", Description = "Customer's first name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's last name.
    /// </summary>
    [Display(Name = "Last Name", Description = "Customer's last name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address.
    /// Used for contact and account identification.
    /// </summary>
    [Display(Name = "Email", Description = "Customer's email address")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's phone number.
    /// </summary>
    [Display(Name = "Phone", Description = "Customer's phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the street address of the customer.
    /// </summary>
    [Display(Name = "Address", Description = "Street address")]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city name.
    /// </summary>
    [Display(Name = "City", Description = "City name")]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the postal code (ZIP code).
    /// </summary>
    [Display(Name = "Postal Code", Description = "ZIP or postal code")]
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the country name.
    /// </summary>
    [Display(Name = "Country", Description = "Country name")]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's date of birth.
    /// </summary>
    [Display(Name = "Birth Date", Description = "Date of birth")]
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the customer account is active.
    /// </summary>
    [Display(Name = "Is Active", Description = "Whether the customer is active")]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the current account balance.
    /// </summary>
    [Display(Name = "Account Balance", Description = "Current account balance")]
    public decimal AccountBalance { get; set; }

    /// <summary>
    /// Gets or sets the list of customer orders.
    /// This demonstrates how collections are handled in the template editor.
    /// </summary>
    [Display(Name = "Orders", Description = "List of customer orders")]
    public List<string> Orders { get; set; } = new();

    /// <summary>
    /// Gets or sets additional notes about the customer.
    /// This field can contain any free-form text.
    /// </summary>
    [Display(Name = "Notes", Description = "Additional notes about the customer")]
    public string Notes { get; set; } = string.Empty;
}
