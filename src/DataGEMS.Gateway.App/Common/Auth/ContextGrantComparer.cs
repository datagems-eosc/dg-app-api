namespace DataGEMS.Gateway.App.Common.Auth;

public class ContextGrantComparer : IEqualityComparer<ContextGrant>
{
	public bool Equals(ContextGrant x, ContextGrant y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x is null && y is null) return true;
		if (x is null || y is null) return false;
		return x.PrincipalId == y.PrincipalId && x.PrincipalType == y.PrincipalType &&
			x.TargetType == y.TargetType && x.TargetId == y.TargetId && x.Role == y.Role;
	}
	public int GetHashCode(ContextGrant obj)
	{
		return HashCode.Combine(obj.PrincipalId, obj.PrincipalType,
			obj.TargetType, obj.TargetId, obj.Role
		);
	}
}
