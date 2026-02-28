using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class CreateRecyclingEnterpriseDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [Required]
    [MaxLength(300)]
    public string Address { get; set; }

    [Required]
    public Guid RepresentativeId { get; set; }
}

public class UpdateRecyclingEnterpriseDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [Required]
    [MaxLength(300)]
    public string Address { get; set; }
}

public class UpdateEnterpriseStatusDto
{
    [Required]
    public string Status { get; set; }
}

public class RecyclingEnterpriseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    public string Address { get; set; }
    public string Status { get; set; }

    public Guid RepresentativeId { get; set; }
    public string RepresentativeName { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}
namespace Application.Contract.DTOs
{
    public class CreateRecyclingEnterpriseDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(300)]
        public string Address { get; set; }

        [Required]
        public Guid RepresentativeId { get; set; }
    }

    public class UpdateRecyclingEnterpriseDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(300)]
        public string Address { get; set; }
    }

    public class UpdateEnterpriseStatusDto
    {
        [Required]
        public string Status { get; set; }
    }

    public class RecyclingEnterpriseDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }

        public Guid RepresentativeId { get; set; }
        public string RepresentativeName { get; set; }
    }
    public class RecyclingEnterpriseFilterDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

