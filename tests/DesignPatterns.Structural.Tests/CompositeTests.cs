using DesignPatterns.Structural.Composite;
using FluentAssertions;

namespace DesignPatterns.Structural.Tests;

public class CompositeTests
{
    [Fact]
    public void Permission_HasPermission_ReturnsTrueForExactMatch()
    {
        var permission = new Permission("orders:read", "View orders");

        permission.HasPermission("orders:read").Should().BeTrue();
    }

    [Fact]
    public void Permission_HasPermission_IsCaseInsensitive()
    {
        var permission = new Permission("orders:read", "View orders");

        permission.HasPermission("Orders:Read").Should().BeTrue();
    }

    [Fact]
    public void Permission_HasPermission_ReturnsFalseForNonMatch()
    {
        var permission = new Permission("orders:read", "View orders");

        permission.HasPermission("orders:write").Should().BeFalse();
    }

    [Fact]
    public void Permission_GetAllPermissions_ReturnsSingleItem()
    {
        var permission = new Permission("orders:read", "View orders");

        permission.GetAllPermissions().Should().ContainSingle()
            .Which.Should().Be("orders:read");
    }

    [Fact]
    public void PermissionGroup_HasPermission_FindsNestedPermission()
    {
        var group = new PermissionGroup("OrderManagement", "Order ops")
            .Add(new Permission("orders:read", "View orders"))
            .Add(new Permission("orders:write", "Create orders"))
            .Add(new Permission("orders:delete", "Cancel orders"));

        group.HasPermission("orders:write").Should().BeTrue();
        group.HasPermission("inventory:read").Should().BeFalse();
    }

    [Fact]
    public void PermissionGroup_DeeplyNested_FindsPermission()
    {
        var refunds = new PermissionGroup("Refunds", "Refund operations")
            .Add(new Permission("refunds:create", "Create refund"))
            .Add(new Permission("refunds:approve", "Approve refund"));

        var orders = new PermissionGroup("Orders", "Order operations")
            .Add(new Permission("orders:read", "Read orders"))
            .Add(refunds);

        var root = new PermissionGroup("AdminRole", "Full access")
            .Add(orders);

        // Should find deeply nested permission
        root.HasPermission("refunds:approve").Should().BeTrue();
        root.HasPermission("orders:read").Should().BeTrue();
        root.HasPermission("users:manage").Should().BeFalse();
    }

    [Fact]
    public void PermissionGroup_GetAllPermissions_FlattensTree()
    {
        var group = new PermissionGroup("Manager", "Manager role")
            .Add(new PermissionGroup("Orders", "Order ops")
                .Add(new Permission("orders:read", "Read"))
                .Add(new Permission("orders:write", "Write")))
            .Add(new PermissionGroup("Inventory", "Inv ops")
                .Add(new Permission("inventory:read", "Read")));

        var all = group.GetAllPermissions();

        all.Should().HaveCount(3);
        all.Should().Contain(["orders:read", "orders:write", "inventory:read"]);
    }

    [Fact]
    public void PermissionGroup_GetAllPermissions_DeduplicatesSharedPermissions()
    {
        var readPermission = new Permission("orders:read", "Read orders");
        var group = new PermissionGroup("Root", "Root")
            .Add(new PermissionGroup("Group A", "A").Add(readPermission))
            .Add(new PermissionGroup("Group B", "B").Add(readPermission));

        var all = group.GetAllPermissions();

        all.Should().HaveCount(1);
    }

    [Fact]
    public void PermissionGroup_Remove_ExcludesPermission()
    {
        var writePermission = new Permission("orders:write", "Write orders");
        var group = new PermissionGroup("Orders", "Orders")
            .Add(new Permission("orders:read", "Read"))
            .Add(writePermission);

        group.HasPermission("orders:write").Should().BeTrue();

        group.Remove(writePermission);

        group.HasPermission("orders:write").Should().BeFalse();
    }

    [Fact]
    public void PermissionGroup_PreventsCyclicReference()
    {
        var groupA = new PermissionGroup("A", "Group A");
        var groupB = new PermissionGroup("B", "Group B");

        groupA.Add(groupB);

        // Adding groupA to groupB would create a cycle
        var act = () => groupB.Add(groupA);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*circular reference*");
    }

    [Fact]
    public void PermissionEvaluator_IsAuthorized_ChecksSinglePermission()
    {
        var permissions = new PermissionGroup("UserRole", "Standard user")
            .Add(new Permission("orders:read", "Read"))
            .Add(new Permission("orders:write", "Write"));

        var evaluator = new PermissionEvaluator(permissions);

        evaluator.IsAuthorized("orders:read").Should().BeTrue();
        evaluator.IsAuthorized("admin:manage").Should().BeFalse();
    }

    [Fact]
    public void PermissionEvaluator_IsAuthorizedForAll_RequiresEveryPermission()
    {
        var permissions = new PermissionGroup("UserRole", "User")
            .Add(new Permission("orders:read", "Read"))
            .Add(new Permission("orders:write", "Write"));

        var evaluator = new PermissionEvaluator(permissions);

        evaluator.IsAuthorizedForAll("orders:read", "orders:write").Should().BeTrue();
        evaluator.IsAuthorizedForAll("orders:read", "admin:manage").Should().BeFalse();
    }

    [Fact]
    public void PermissionEvaluator_IsAuthorizedForAny_RequiresAtLeastOne()
    {
        var permissions = new PermissionGroup("UserRole", "User")
            .Add(new Permission("orders:read", "Read"));

        var evaluator = new PermissionEvaluator(permissions);

        evaluator.IsAuthorizedForAny("orders:read", "admin:manage").Should().BeTrue();
        evaluator.IsAuthorizedForAny("admin:manage", "settings:write").Should().BeFalse();
    }

    [Fact]
    public void PermissionEvaluator_GetMissingPermissions_IdentifiesGaps()
    {
        var permissions = new PermissionGroup("UserRole", "User")
            .Add(new Permission("orders:read", "Read"));

        var evaluator = new PermissionEvaluator(permissions);

        var missing = evaluator.GetMissingPermissions("orders:read", "orders:write", "admin:manage");

        missing.Should().HaveCount(2);
        missing.Should().Contain(["orders:write", "admin:manage"]);
    }

    [Fact]
    public void SampleHierarchy_AdminRole_HasAllPermissions()
    {
        var adminRoot = PermissionEvaluator.BuildSampleHierarchy();
        var evaluator = new PermissionEvaluator(adminRoot);

        evaluator.IsAuthorized("orders:read").Should().BeTrue();
        evaluator.IsAuthorized("refunds:approve").Should().BeTrue();
        evaluator.IsAuthorized("users:manage").Should().BeTrue();
        evaluator.IsAuthorized("settings:manage").Should().BeTrue();

        var allPermissions = evaluator.GetAllGrantedPermissions();
        allPermissions.Should().HaveCountGreaterOrEqualTo(11);
    }
}
