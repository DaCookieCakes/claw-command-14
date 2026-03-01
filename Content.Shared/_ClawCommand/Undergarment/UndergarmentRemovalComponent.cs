using Content.Shared.Body;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Prototypes;

namespace Content.Shared._ClawCommand.Undergarment;

[RegisterComponent]
public sealed partial class UndergarmentRemovalComponent : Component
{
    public bool Removed;
    public Dictionary<ProtoId<OrganCategoryPrototype>, Dictionary<HumanoidVisualLayers, List<Marking>>> SavedMarkings = new();
}
