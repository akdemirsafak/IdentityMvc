﻿namespace IdentityMvc.Areas.Admin.ViewModels;

public class RoleAssignViewModel
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool Exist { get; set; }
}