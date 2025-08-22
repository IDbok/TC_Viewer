namespace TC_WinForms.WinForms.Win6.Healpers;

public static class DgvRoles
{
    public static DataGridViewColumn WithRole(this DataGridViewColumn c, ColumnRole role)
    { c.Tag = role; return c; }

    public static bool IsRole(this DataGridViewColumn c, ColumnRole role)
    { return Equals(c.Tag, role); }
}

public enum ColumnRole { None, TimeOfMechanism, Remarks }
