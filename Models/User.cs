using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailStore.Models;
[Table("users")]
public partial class User
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("username")]
    public string Username { get; set; } = null!;

    [Column("password")]
    public string Password { get; set; } = null!;

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("role")]
    public string? Role { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}
