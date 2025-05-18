using Vintagestory.API.Common;
using Vintagestory.API.Client;
using ElectricalProgressive.Content.Item.Armor;
using ElectricalProgressive.Content.Item.Weapon;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;



[assembly: ModDependency("game", "1.20.0")]
[assembly: ModDependency("electricalprogressivecore", "1.0.1")]
[assembly: ModDependency("electricalprogressivebasics", "1.0.1")]
[assembly: ModDependency("electricalprogressiveqol", "1.0.1")]
[assembly: ModInfo(
    "Electrical Progressive: Equipment",
    "electricalprogressiveequipment",
    Website = "https://github.com/tehtelev/ElectricalProgressiveEquipment",
    Description = "Brings electricity into the game!",
    Version = "1.0.1",
    Authors = new[] {
        "Tehtelev",
        "Kotl"
    }
)]

namespace ElectricalProgressive;

public class ElectricalProgressiveEquipment : ModSystem
{

    public static bool combatoverhaul = false;                        //установлен ли combatoverhaul
    private ICoreAPI api = null!;
    private ICoreClientAPI capi = null!;
    

    private readonly string[] _targetFiles =
    {
        "itemtypes/armor/static-armor.json",
        "itemtypes/armor/static-boots.json",
        "itemtypes/armor/static-helmet.json"
    };


    // Fix for CS1503: The second argument of RegisterEntityClass should be of type EntityProperties, not System.Type.
    // To resolve this, we need to create an instance of EntityProperties and pass it as the second argument.

    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        this.api = api;

        api.RegisterItemClass("EArmor", typeof(EArmor));
        api.RegisterItemClass("EWeapon", typeof(EWeapon));
        api.RegisterItemClass("ESpear", typeof(ESpear));
        api.RegisterItemClass("EShield", typeof(EShield));

        api.RegisterEntity("EntityESpear", typeof(EntityESpear));

        if (api.ModLoader.IsModEnabled("combatoverhaul"))
            combatoverhaul = true;

        // Patch armor for CO
        if (combatoverhaul)
        {
            var processor = new ArmorAssetProcessor(api);
            processor.ProcessAssets("electricalprogressiveequipment", _targetFiles);
        }
    }



    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        this.capi = api;
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);

    }
}