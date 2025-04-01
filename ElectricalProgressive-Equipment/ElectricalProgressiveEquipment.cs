using Vintagestory.API.Common;
using Vintagestory.API.Client;
using ElectricalProgressive.Content.Item.Armor;
using ElectricalProgressive.Content.Item.Weapon;



[assembly: ModDependency("game", "1.20.0")]
[assembly: ModDependency("electricalprogressivecore", "0.9.2")]
[assembly: ModDependency("electricalprogressivebasics", "0.9.2")]
[assembly: ModDependency("electricalprogressiveqol", "0.9.2")]
[assembly: ModInfo(
    "Electrical Progressive: Equipment",
    "electricalprogressiveequipment",
    Website = "https://github.com/tehtelev/ElectricalProgressiveEquipment",
    Description = "Brings electricity into the game!",
    Version = "0.9.2",
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


    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        this.api = api;

        api.RegisterItemClass("EArmor", typeof(EArmor));
        api.RegisterItemClass("EWeapon", typeof(EWeapon));
        api.RegisterItemClass("EShield", typeof(EShield));


        if (api.ModLoader.IsModEnabled("combatoverhaul"))
            combatoverhaul = true;


        //патч бронм для CO
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
}