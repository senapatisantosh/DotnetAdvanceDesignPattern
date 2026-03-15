using DesignPatterns.Creational.Prototype;
using FluentAssertions;

namespace DesignPatterns.Creational.Tests;

public class PrototypeTests
{
    private static FeatureFlagConfig CreateSampleConfig()
    {
        return new FeatureFlagConfig
        {
            FlagKey = "new-checkout-flow",
            Name = "New Checkout Flow",
            Description = "Redesigned checkout experience with fewer steps",
            IsEnabled = true,
            DefaultRolloutPercentage = 25.0,
            Variants = new Dictionary<string, string>
            {
                ["control"] = "Original checkout",
                ["variant-a"] = "3-step checkout",
                ["variant-b"] = "1-page checkout"
            },
            Tags = new Dictionary<string, string>
            {
                ["team"] = "growth",
                ["sprint"] = "2024-Q4"
            },
            TargetingRules =
            [
                new TargetingRule
                {
                    RuleId = "rule-1",
                    Description = "Enterprise customers get variant-a",
                    Priority = 1,
                    RolloutPercentage = 100.0,
                    VariantKey = "variant-a",
                    TargetSegments =
                    [
                        new UserSegment
                        {
                            SegmentId = "seg-enterprise",
                            Name = "Enterprise Tier",
                            IncludedUserIds = ["user-100", "user-200"],
                            ExcludedUserIds = ["user-blocked"],
                            AttributeFilters = new Dictionary<string, string>
                            {
                                ["plan"] = "enterprise",
                                ["region"] = "US"
                            }
                        }
                    ]
                },
                new TargetingRule
                {
                    RuleId = "rule-2",
                    Description = "Beta users get variant-b",
                    Priority = 2,
                    RolloutPercentage = 50.0,
                    VariantKey = "variant-b",
                    TargetSegments =
                    [
                        new UserSegment
                        {
                            SegmentId = "seg-beta",
                            Name = "Beta Testers",
                            IncludedUserIds = ["user-beta-1", "user-beta-2"],
                            AttributeFilters = new Dictionary<string, string>
                            {
                                ["beta"] = "true"
                            }
                        }
                    ]
                }
            ]
        };
    }

    // --- Deep Clone Independence Tests ---

    [Fact]
    public void DeepClone_CreatesIndependentCopy()
    {
        var original = CreateSampleConfig();

        var clone = original.DeepClone();

        clone.FlagKey.Should().Be(original.FlagKey);
        clone.Name.Should().Be(original.Name);
        clone.IsEnabled.Should().Be(original.IsEnabled);
        clone.DefaultRolloutPercentage.Should().Be(original.DefaultRolloutPercentage);
        clone.Should().NotBeSameAs(original);
    }

    [Fact]
    public void DeepClone_ModifyingClone_DoesNotAffectOriginal()
    {
        var original = CreateSampleConfig();
        var clone = original.DeepClone();

        // Modify the clone.
        clone.FlagKey = "modified-key";
        clone.Name = "Modified Name";
        clone.IsEnabled = false;
        clone.DefaultRolloutPercentage = 99.0;

        // Original is unchanged.
        original.FlagKey.Should().Be("new-checkout-flow");
        original.Name.Should().Be("New Checkout Flow");
        original.IsEnabled.Should().BeTrue();
        original.DefaultRolloutPercentage.Should().Be(25.0);
    }

    [Fact]
    public void DeepClone_ModifyingNestedCollections_DoesNotAffectOriginal()
    {
        var original = CreateSampleConfig();
        var clone = original.DeepClone();

        // Modify clone's targeting rules.
        clone.TargetingRules[0].Description = "CHANGED";
        clone.TargetingRules[0].RolloutPercentage = 0;
        clone.TargetingRules.Add(new TargetingRule
        {
            RuleId = "rule-new",
            Description = "New rule"
        });

        // Original targeting rules are unchanged.
        original.TargetingRules.Should().HaveCount(2);
        original.TargetingRules[0].Description.Should().Be("Enterprise customers get variant-a");
        original.TargetingRules[0].RolloutPercentage.Should().Be(100.0);
    }

    [Fact]
    public void DeepClone_ModifyingNestedSegments_DoesNotAffectOriginal()
    {
        var original = CreateSampleConfig();
        var clone = original.DeepClone();

        // Modify a deeply nested user segment in the clone.
        var cloneSegment = clone.TargetingRules[0].TargetSegments[0];
        cloneSegment.Name = "CHANGED SEGMENT";
        cloneSegment.IncludedUserIds.Add("new-user");
        cloneSegment.AttributeFilters["plan"] = "free";

        // Original's nested segment is unchanged.
        var origSegment = original.TargetingRules[0].TargetSegments[0];
        origSegment.Name.Should().Be("Enterprise Tier");
        origSegment.IncludedUserIds.Should().HaveCount(2);
        origSegment.AttributeFilters["plan"].Should().Be("enterprise");
    }

    [Fact]
    public void DeepClone_ModifyingDictionaries_DoesNotAffectOriginal()
    {
        var original = CreateSampleConfig();
        var clone = original.DeepClone();

        clone.Variants["new-variant"] = "Something new";
        clone.Tags["team"] = "platform";

        original.Variants.Should().NotContainKey("new-variant");
        original.Tags["team"].Should().Be("growth");
    }

    // --- Shallow Clone Danger Test ---

    [Fact]
    public void ShallowClone_SharesReferences_DangerousForMutableObjects()
    {
        var original = CreateSampleConfig();
        var shallow = original.ShallowClone();

        // Shallow clone shares the same list reference!
        shallow.TargetingRules.Should().BeSameAs(original.TargetingRules);
        shallow.Variants.Should().BeSameAs(original.Variants);

        // Modifying the shallow clone's list ALSO modifies the original.
        shallow.TargetingRules.Add(new TargetingRule { RuleId = "danger", Description = "This affects original!" });
        original.TargetingRules.Should().HaveCount(3, "shallow clone shares the list reference");
    }

    // --- CloneAsAbTestVariant Tests ---

    [Fact]
    public void CloneAsAbTestVariant_CreatesVariantWithModifiedKey()
    {
        var original = CreateSampleConfig();

        var variant = original.CloneAsAbTestVariant("variant-b", 50.0);

        variant.FlagKey.Should().Be("new-checkout-flow-variant-b");
        variant.Name.Should().Contain("variant-b");
        variant.DefaultRolloutPercentage.Should().Be(50.0);
        variant.Tags.Should().ContainKey("ab-test-variant");
        variant.Tags["ab-test-parent"].Should().Be("new-checkout-flow");
    }

    [Fact]
    public void CloneAsAbTestVariant_DoesNotAffectOriginal()
    {
        var original = CreateSampleConfig();

        var variant = original.CloneAsAbTestVariant("test", 75.0);

        original.FlagKey.Should().Be("new-checkout-flow");
        original.DefaultRolloutPercentage.Should().Be(25.0);
        original.Tags.Should().NotContainKey("ab-test-variant");
    }

    // --- Registry Tests ---

    [Fact]
    public void Registry_RegisterAndCreate_ProducesIndependentClone()
    {
        var registry = new FeatureFlagRegistry();
        var template = CreateSampleConfig();
        registry.Register("gradual-rollout", template);

        var clone1 = registry.CreateFromTemplate("gradual-rollout");
        var clone2 = registry.CreateFromTemplate("gradual-rollout");

        clone1.Should().NotBeSameAs(clone2);
        clone1.FlagKey.Should().Be(clone2.FlagKey);
    }

    [Fact]
    public void Registry_RegisteredTemplate_IsIndependentOfSource()
    {
        var registry = new FeatureFlagRegistry();
        var template = CreateSampleConfig();
        registry.Register("template", template);

        // Modify the source after registration.
        template.FlagKey = "CHANGED";

        var clone = registry.CreateFromTemplate("template");
        clone.FlagKey.Should().Be("new-checkout-flow", "registry stored a deep clone of the original");
    }

    [Fact]
    public void Registry_CreateFromTemplate_WithCustomization()
    {
        var registry = new FeatureFlagRegistry();
        registry.Register("base", CreateSampleConfig());

        var custom = registry.CreateFromTemplate("base", config =>
        {
            config.FlagKey = "custom-flag";
            config.IsEnabled = false;
            config.DefaultRolloutPercentage = 0;
        });

        custom.FlagKey.Should().Be("custom-flag");
        custom.IsEnabled.Should().BeFalse();
        custom.DefaultRolloutPercentage.Should().Be(0);
    }

    [Fact]
    public void Registry_ThrowsForUnknownTemplate()
    {
        var registry = new FeatureFlagRegistry();

        var act = () => registry.CreateFromTemplate("nonexistent");

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Registry_HasTemplate_ReturnsCorrectly()
    {
        var registry = new FeatureFlagRegistry();
        registry.Register("exists", CreateSampleConfig());

        registry.HasTemplate("exists").Should().BeTrue();
        registry.HasTemplate("nope").Should().BeFalse();
    }

    [Fact]
    public void Registry_Unregister_RemovesTemplate()
    {
        var registry = new FeatureFlagRegistry();
        registry.Register("temp", CreateSampleConfig());

        registry.Unregister("temp").Should().BeTrue();
        registry.HasTemplate("temp").Should().BeFalse();
    }
}
